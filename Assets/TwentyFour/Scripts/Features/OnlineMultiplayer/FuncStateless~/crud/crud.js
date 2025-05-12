const uosConfig = require("../common/uosConfig")

async function connection(context) {
  context.database = require('func-stateless-postgresql-sdk-nodejs').database;  
  const [host, port] = uosConfig.address.split(":");
  const database = await context.database(uosConfig.databaseId, uosConfig.databasePassword, "data", {
    username : 'prod',
    host,
    port,
    minSize : 4,
    maxSize : 20
    });

  // 本地测试2
  // 本地使用公网调试
  // const database = await context.database(uosConfig.databaseId, uosConfig.databasePassword, "data", {
  //   username : 'prod',
  //   host : '111.229.160.149',
  //   port : 7010,
  //   minSize : 4,
  //   maxSize : 20
  // });
  console.log("start connection")
  const client = await database.connection();
  console.log("after connection")
  return client;
}

exports.connection = connection;