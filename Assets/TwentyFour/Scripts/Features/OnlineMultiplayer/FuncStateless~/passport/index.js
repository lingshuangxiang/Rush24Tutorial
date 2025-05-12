// 本地测试：
// usf-cli -r --function=passport

const passport = require("./passport")

async function main(event, context) {
  return await passport(event)
};

exports.main = main