// 测试部署结果
// node test/playerInfo.js

const axios = require("axios")
const url = "https://stateless.unity.cn/release/7fa14adf-6df8-4672-8a48-a16118ae5887/player_info"

async function GetInfo() {
  const response = await axios({
    method: "GET",
    params: {
      "uniqueId": "1000002001"
    },
    url
  })
  console.log("GetInfo响应")
  console.log(response.data)
}

GetInfo();