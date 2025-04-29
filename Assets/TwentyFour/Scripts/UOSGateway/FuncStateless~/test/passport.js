// 本地访问线上部署接口
// 部署：
// usf-cli -a --id=3e2310e5-e991-45e4-8890-1cf58af45eee --secret=0924a6bf86704efcba466c86d3f887f7
// usf-cli -d --installDependency
// 测试部署结果：
// node test/passport.js

// func 测试 app
// usf-cli -a --id=7fa14adf-6df8-4672-8a48-a16118ae5887 --secret=f7e72d1425bb453daf8fceea4eb06a04
const axios = require("axios")
const baseURL = "https://service-5md58b0p-1301389817.sh.tencentapigw.com/release/3e2310e5-e991-45e4-8890-1cf58af45eee/passport"
const headers = {
  "Authorization": "Basic OTEwYTgwNGYtZTYwNy00YzhmLWE2MjctYWE0NGM2YmM4M2JlOjQ2ZjQ1NDQ4MTBiNTQ5MmU5MTliMjcwNzllMjdhMTM0"
}

async function updateScore() {
  const slugName = "test"
  const url = `/v1/leaderboards/${slugName}/scores`
  const response = await axios({
    method: "POST",
    baseURL,
    headers,
    params: {
      slugName
    },
    url,
    data: {"memberId":"1000006001","score":2},
  })
  console.log(JSON.stringify(response.data, null, 2));
}

async function ListLeaderboardScores() {
  const slugName = "test"
  const url = `/v1/leaderboards/${slugName}/scores`
  const response = await axios({
    method: "GET",
    baseURL,
    url,
    headers,
    params: {
      slugName,
      start: 1,
      count: 1
    }
  })
  console.log(JSON.stringify(response.data, null, 2));
}

async function main() {
  await updateScore();
  // await ListLeaderboardScores();
}

main();