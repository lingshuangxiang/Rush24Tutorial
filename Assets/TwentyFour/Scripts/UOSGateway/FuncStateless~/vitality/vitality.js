const passportAPI = require("../passport_api/passportAPI");
const remoteConfigAPI = require("../remote_config_api/remoteConfigAPI");

const resourceSlug = "VIT";

// 计算当前体力
function calculateCurrentVitality(
  lastUpdateTime,
  currentTime,
  interval,
  points,
  vitality,
  limit
) {
  // 计算经过的时间（秒）
  const elapsedSeconds = (new Date(currentTime) - new Date(lastUpdateTime)) / 1000;
  console.log({elapsedSeconds,
    currentTime,
    lastUpdateTime
  })
  // 计算完整间隔次数
  const intervalsPassed = Math.floor(elapsedSeconds / interval);
  const lastRecoveredTime = new Date(new Date(lastUpdateTime).getTime() + intervalsPassed * interval * 1000).toISOString();
  // 恢复的体力点数
  const recoveredPoints = intervalsPassed * points;
  // 返回当前体力（确保不为负数）
  let currentVitality = Math.min(
    Math.max(vitality + recoveredPoints, 0),
    limit
  );

  console.log({ currentVitality, lastRecoveredTime, vitality, recoveredPoints, limit  })
  return { currentVitality, lastRecoveredTime };
}

// 获取远程配置 - 查询体力配置
async function getVitalityConfig() {
  const remoteConfigResp = await remoteConfigAPI.getSettings(
    ["VitalityConfig"],
    ["JSON"]
  );
  const vitalityConfigStr = remoteConfigResp?.settings?.VitalityConfig?.value;
  let vitalityConfig = {};
  const defaultConfig = {
    interval: 600,
    points: 5,
    limit: 50
  };
  try {
    vitalityConfig = JSON.parse(vitalityConfigStr) ?? defaultConfig;
  } catch (err) {
    vitalityConfig = defaultConfig;
  }
  return vitalityConfig;
}

function formatTime(timeStr) {
  return timeStr.split('.')[0] + 'Z'
}

async function SetResourceMaxValue(resource) {
  const vitalityConfig = await getVitalityConfig();
  const { interval = 600, points = 5, limit = 50 } = vitalityConfig;
  if(resource) {
    resource.maxValue = limit
  }
}

const vitality = {
  // 发放体力
  async deposit(client, event, context) {
    const { uniqueId } = event.queryString;

    // 查询数据库，获取上次更新时间 lastUpdateTime
    const currentTime = new Date().toISOString();
    const insert = `
      INSERT INTO player_vitality (persona_id, last_update_time)
      VALUES ($1, $2)
      ON CONFLICT (persona_id)
      DO UPDATE SET 
        last_update_time = player_vitality.last_update_time
      RETURNING persona_id, last_update_time, (xmax = 0) AS is_new;
    `;
    const { rows } = await client.query(insert, [uniqueId, currentTime]);
    console.log(rows[0]);
    let lastUpdateTime = rows[0]?.last_update_time.toISOString();
    const isNew = rows[0]?.is_new;

    // 获取体力远程配置
    const vitalityConfig = await getVitalityConfig();
    const { interval = 600, points = 5, limit = 50 } = vitalityConfig;

    // 查询体力和上限
    const inventoryResp = await passportAPI.searchInventory(
      uniqueId,
      resourceSlug
    );

    const vitality = inventoryResp?.inventory?.[0]?.quantity ?? 0;
    console.log({inventoryResp: JSON.stringify(inventoryResp, null, 2)})
    const getResourceResp = await passportAPI.getResourceInventory(resourceSlug);
    const resource = getResourceResp?.resource;
    await SetResourceMaxValue(resource);

    // 计算最新体力
    let { currentVitality, lastRecoveredTime } = calculateCurrentVitality(
      lastUpdateTime,
      currentTime,
      interval,
      points,
      vitality,
      limit
    );

    // 调用经济系统 API 发放体力
    let recoveredPoints = currentVitality - vitality;
    if(isNew) recoveredPoints = limit - vitality;
    let depositResp;
    if (recoveredPoints > 0) {
      depositResp = await passportAPI.depositInventory(
        uniqueId,
        resourceSlug,
        recoveredPoints
      );
    } else {
      // 体力可能已经超过上限
      currentVitality = vitality;
    }

    // 更新数据库最新时间
    const query = `
    UPDATE player_vitality
    SET last_update_time = $2
    WHERE persona_id = $1
    `;
    lastUpdateTime = lastRecoveredTime;
    await client.query(query, [uniqueId, lastRecoveredTime]);
    if (depositResp?.inventoryItems?.[0]?.quantity) {
      // 使用响应中最新的数据
      console.log("【预期】 " + (recoveredPoints + vitality));
      currentVitality = depositResp?.inventoryItems?.[0]?.quantity;
      console.log("【实际】 " + depositResp?.inventoryItems?.[0]?.quantity);
    }

    console.log({
      currentVitality,
      vitalityConfig,
      lastUpdateTime,
      previousVitality: vitality,
      limit,
    });

    return {
      quantity: currentVitality,
      resource,
      lastUpdateTime: formatTime(lastUpdateTime),
    };
  },
  async query(client, event, context) {
    let result = null;
    try {
      await client.query("BEGIN");
      result = this.deposit(client, event, context);

      await client.query("COMMIT");
    } catch (err) {
      await client.query("ROLLBACK");
      throw err;
    } finally {
      await client.release();
    }
    return result;
  },
  async consume(client, event, context) {
    let result = {
      resource: {},
      quantity: 0,
      lastUpdateTime: "",
    };
    const { uniqueId, consumeQuantity } = event.queryString;
    try {
      await client.query("BEGIN");
      const depositResult = await this.deposit(client, event, context);
      const getResourceResp = await passportAPI.getResourceInventory(resourceSlug);
      const resource = getResourceResp?.resource;
      result.resource = resource;
      await SetResourceMaxValue(result.resource);

      result.quantity = depositResult.quantity;
      const lastUpdateTime = new Date().toISOString();
      result.lastUpdateTime = formatTime(lastUpdateTime);

      // 调用经济系统 API 消耗体力
      try {
        const consumeResult = await passportAPI.consumeByResourceSlugInventory(
          uniqueId,
          resourceSlug,
          consumeQuantity
        );
        result.quantity = consumeResult?.inventoryItems?.[0]?.quantity;

        // 更新数据库最新时间
        // const query = `
        //   UPDATE player_vitality
        //   SET last_update_time = $2
        //   WHERE persona_id = $1
        //   `;
        // await client.query(query, [uniqueId, lastUpdateTime]);
        result.success = true;

        console.log("返回的结果");
        console.log(result);
      } catch (err) {
        result.success = false;
        console.log("消耗体力出错");
        console.log(err);
      }

      await client.query("COMMIT");
    } catch (err) {
      await client.query("ROLLBACK");
      throw err;
    } finally {
      await client.release();
    }
    return result;
  },
};

module.exports = vitality;
