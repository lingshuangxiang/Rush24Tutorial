// 测试部署结果
// node test/subscribe.js

const axios = require("axios")
const url = "https://stateless.unity.cn/release/7fa14adf-6df8-4672-8a48-a16118ae5887/subscribe"

async function TestPost() {
  const response = await axios({
    method: "POST",
    data: {
      "templateId": "C-nFuOVu1xiVju__Docj-XQPSMtnLSwBH-JtFGAJvhk",
      "toUser": "o1CXs61xhr_u1XIImE2wyDE_YNKw",
      "page": null,
      "data": "{\"number01\":{\"value\":\"339208499\"},\"date01\":{\"value\":\"2015年01月05日\"},\"site01\":{\"value\":\"TIT创意园\"},\"site02\":{\"value\":\"广州市新港中路397号\"}}",
      "miniProgramState": "normal",
      "lang": "zh_CN",
      "notifyTime": "2025-04-20 10:27:17+08",
      "slugName": "UOSTournament"
    },
    url
  })
  console.log(response.data)
}

async function TestGet() {
  const response = await axios({
    method: "GET",
    params: {
      "toUser": "o1CXs6wyFYYikiJUV1PxpESR9lI4",
      "slugName": "UOSGodzillaTournamentSlug"
    },
    url
  })
  console.log(response.data)
}


TestGet();