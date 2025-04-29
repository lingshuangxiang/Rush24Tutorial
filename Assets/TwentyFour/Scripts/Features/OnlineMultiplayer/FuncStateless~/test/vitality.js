// 测试部署结果
// node test/vitality.js

const axios = require("axios")
const baseURL = "https://stateless.unity.cn/release/7fa14adf-6df8-4672-8a48-a16118ae5887/vitality"

const uniqueId = "1000032002"

let count = 0
let count2 = 0

async function query() {
  const response = await axios({
    method: "GET",
    params: {
      uniqueId,
    },
    baseURL,
    url: "/query"
  })
  console.log(new Date().toISOString() + "       查询" + (++count) + "   " + response.data.quantity)
}

async function consume() {
  const response = await axios({
    method: "POST",
    params: {
      uniqueId,
      "consumeQuantity": 5
    },
    baseURL,
    url: "/consume"
  })
  console.log(new Date().toISOString() + "   消耗" + (++count2) + "   " + response.data.quantity)
}


setInterval(async () => {
  await query();
}, 3 * 1000)



setInterval(async () => {
  await consume();
}, 28 * 1000)