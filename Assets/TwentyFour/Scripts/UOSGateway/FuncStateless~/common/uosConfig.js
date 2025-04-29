// node common/uosConfig.js
const path = require('path');

const currentEnv = "dev";

require('dotenv').config({
  path: path.resolve(__dirname, `../.env.${currentEnv}`)
});

// 写入环境变量
const uosConfig = {
  databaseId: process.env.CRUD_DATABASE_ID,
  databasePassword: process.env.CRUD_DATABASE_PASSWORD,
  address: process.env.CRUD_ADDRESS,
  math24AppID: process.env.RUSH_24_APP_ID,
  math24AppServiceSecret: process.env.RUSH_24_APP_SECRET,
  wechatAppID: process.env.WECHAT_APP_ID,
  wechatAppSecret: process.env.WECHAT_APP_SECRET
}

console.log(uosConfig);

module.exports = uosConfig;