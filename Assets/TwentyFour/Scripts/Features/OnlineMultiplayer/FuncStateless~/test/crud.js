// 本地访问线上部署接口
// 部署：
// usf-cli -a --id=7fa14adf-6df8-4672-8a48-a16118ae5887 --secret=f7e72d1425bb453daf8fceea4eb06a04
// usf-cli -d --installDependency
// 测试部署结果：
// node test/crud.js

const axios = require("axios")
const url = "https://stateless.unity.cn/release/7fa14adf-6df8-4672-8a48-a16118ae5887/crud"

const getBattleData = require('../test/getBattleData');
const data = getBattleData();
console.log(data)

async function setMatch() {
  const response = await axios({
    method: "POST",
    params: {
      "method": "set_match"
    },
    url,
    data,
  })
  console.log("setMatch响应")
  console.log(JSON.stringify(response.data, null, 2));
}

async function getMatches() {
  const response = await axios({
    method: "GET",
    params: {
      "method": "get_matches",
      "uniqueId": "20251000006001",
      "page": 1,
      "pageSize": 1
    },
    url,
  })
  console.log("getMatches响应")
  console.log(JSON.stringify(response.data, null, 2));
}

getMatches()
// setMatch()