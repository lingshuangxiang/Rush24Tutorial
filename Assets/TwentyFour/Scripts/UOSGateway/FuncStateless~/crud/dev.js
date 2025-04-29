// 开发测试
// 本地使用公网连接数据库
// 先切换到测试环境: node upload.js test
// node crud/dev.js
const { main } = require("./index")

async function testFunc() {
  const getBattleData = require('../test/getBattleData');
  const battleData = getBattleData();
  const res1 = await main({
    httpMethod: "POST",
    queryString: {
      "method": "set_match"
    },
    body: JSON.stringify(battleData)
  }, {})
  console.log("res1")
  console.log(res1)

  // const res2 = await main({
  //   httpMethod: "GET",
  //   queryString: {
  //     method: "get_matches",
  //     uniqueId:"1000006001"
  //   }
  // }, {})
  // console.log("res2")
  // console.log(JSON.stringify(res2.body.data, null, 2))
}

testFunc()
