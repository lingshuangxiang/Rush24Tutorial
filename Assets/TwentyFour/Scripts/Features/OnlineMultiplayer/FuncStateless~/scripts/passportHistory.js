// 迁移 passport 历史成绩到新的排行榜上
// node scripts/passportHistory.js

const axios = require("axios");
const { headers } = require("../passport/auth")
const endpoints = require("../common/endpoints")

const sourceLeaderboard = "test"
const targetLeaderboard = "SEASON202504Leaderboard"

async function getLeaderboard(slugName, start = 0, count = 10) {
  const url = `/v1/leaderboards/${slugName}/scores`
  const response = await axios({
    method: "GET",
    url,
    baseURL: endpoints.passport,
    headers,
    params: {
      slugName,
      start,
      count
    }
  });
  return response.data;
}

// 获取排行榜所有成绩
async function getLeaderboardAllScores() {
  // 先查询总数
  const { total } = await getLeaderboard(sourceLeaderboard);
  let scores = []
  console.log({total})
  const count = 50;
  const page = Math.ceil(total / count);
  for(let i = 0; i < page; i += 1) {
    const resp = await getLeaderboard(sourceLeaderboard, i * count, count);
    scores = scores.concat(resp.scores)
  }
  console.log({page, total, finalCount: scores.length})
  return scores;
}

async function getLeaderboard(slugName, start = 0, count = 10) {
  const url = `/v1/leaderboards/${slugName}/scores`
  const response = await axios({
    method: "GET",
    url,
    baseURL: endpoints.passport,
    headers,
    params: {
      slugName,
      start,
      count
    }
  });
  return response.data;
}

async function setLeaderboard(slugName, memberId, score) {
  const url = `/v1/leaderboards/${slugName}/scores`
  score = score - 3;

  if(score < 0) score = 0;

  if(score > 32) {
    score = 32;
  }

  const response = await axios({
    method: "POST",
    url,
    baseURL: endpoints.passport,
    headers,
    params: {
      slugName,
    },
    data: {
      memberId,
      score
    }
  });
  return response.data;
}

// 上传成绩到新的排行榜
async function uploadLeaderboard() {
  const scores = await getLeaderboardAllScores("sourceLeaderboard")
  for(let i = 0; i < scores.length; i += 1) {
    await setLeaderboard(targetLeaderboard, scores[i].memberId, scores[i].score);
  }
}

uploadLeaderboard();

