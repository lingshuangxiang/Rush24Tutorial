const wechatAPI = require("../common/wechatAPI");

const TimeGap = 2 * 60; // 秒

function isWithinXSeconds(targetTime) {
  console.log({
    targetTime,
  });
  // 获取当前时间的时间戳（以秒为单位）
  const currentTime = Math.floor(Date.now() / 1000);

  // 将目标时间转换为时间戳（以秒为单位）
  const targetTimeStamp = Math.floor(targetTime.getTime() / 1000);

  // 判断间隔是否在 x 秒内
  return Math.abs(currentTime - targetTimeStamp) <= TimeGap;
}

async function notify(client, event, context) {
  let result = null;
  try {
    await client.query("BEGIN");

    const query = `
    SELECT 
      wn.wechat_notify_id,
      wn.template_id,
      wn.touser,
      wn.page,
      wn.miniprogram_state,
      wn.lang,
      wn.data,
      wn.notify_time
    FROM wechat_notify wn
    WHERE wn.notified = false`;

    const { rows } = await client.query(query);

    for(const row of rows) {
      if (isWithinXSeconds(row.notify_time)) {
        console.log(`【通知】notify user ${row.touser} in ${row.notify_time}`);
        await wechatAPI.sendSubscribeMessage(row);

        const update = `
          UPDATE wechat_notify wn
          SET notified = true
          WHERE wn.wechat_notify_id = $1`;
        await client.query(update, [row.wechat_notify_id]);
      } else {
        console.log(`【不通知】don't notify user ${row.touser} in ${row.notify_time}`);
      }
    }

    await client.query("COMMIT");
  } catch (err) {
    await client.query("ROLLBACK");
    throw err;
  } finally {
    await client.release();
  }
  return result;
}

module.exports = notify;
