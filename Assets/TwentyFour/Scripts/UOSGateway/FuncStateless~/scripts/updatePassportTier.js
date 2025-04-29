// node scripts/updatePassportTier.js
// 更新段位排行榜配置

const axios = require("axios");
const { headers } = require("../passport/auth")
const endpoints = require("../common/endpoints")

// 配置：
const tiersConfig = ['石头', '青铜', '白银', '黄金', '钻石']
const levelsConfig = [ 'I', 'II', 'III']
const levelDiff = 4
const slugName = 'SEASON202504Leaderboard'


const tierStrategyEnum = {
  'Score': 'Score',
  'Rank': 'Rank',
  'Percent': 'Percent'
}

function genTiersConfig() {
  let level = 0;
  const result = []

  for(const tierName of tiersConfig) {
    if(tierName == "钻石") {
      const config = {
        name: `${tierName}`,
        from: level,
        to: null
      }
      result.push(config)
      return result;
    }
    for(const levelName of levelsConfig) {
      const config = {
        name: `${tierName}${levelName}`,
        from: level,
        to: level + levelDiff
      }
      result.push(config)
      level += levelDiff;
    }
  }
  result[result.length - 1].to = null;
  return result;
}

async function updateLeaderboard(slugName) {
  const url = `/v1/leaderboards/${slugName}`
  const tiers = genTiersConfig();
  const body = {
    tierStrategy: tierStrategyEnum.Score,
    tiers
  }
  const response = await axios({
    method: "PUT",
    url,
    baseURL: endpoints.passport,
    headers,
    data: body
  });
  console.log({body})
  console.log(JSON.stringify(response.data, null, 2));
  return response.data;
}

updateLeaderboard(slugName)