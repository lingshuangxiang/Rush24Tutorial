const axios = require("axios");
const { headers } = require("../passport/auth")
const endpoints = require("../common/endpoints")
const passportAPI = require("../passport_api/passportAPI")

const playerInfo = {
  // 获取个人统计数据
  async getStatics(client, event, context) {
    let result = null;
    const { uniqueId } = event.queryString;
    try {
      await client.query("BEGIN");

      const query = `
      SELECT 
        p.total_resolved,
        p.highest_score,
        p.highest_tier,
        p.total_time
      FROM player_info p
      WHERE p.persona_id = $1`;

      const { rows } = await client.query(query, [uniqueId]);

      // 如果没有记录，则查询 passport
      if (!rows.length) {
        try {
          const response = await passportAPI.getPersona(uniqueId);
          const personaProperties = response?.data?.Persona?.properties ?? {}

          result = {
            totalResolved: personaProperties.totalSolved || 0,
            totalTime: personaProperties.totalTime || 0,
            highestScore: personaProperties.battle_highest_score,
            highestTier: personaProperties.battle_highest_tier
          }

        } catch (error) {
          console.error("Error:", error);
          return error;
        }
      } else {
        result = rows.map((item) => ({
          totalResolved: item.total_resolved,
          highestScore: item.highest_score,
          highestTier: item.highest_tier,
          totalTime: item.total_time,
        }))[0];
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

module.exports = playerInfo;
