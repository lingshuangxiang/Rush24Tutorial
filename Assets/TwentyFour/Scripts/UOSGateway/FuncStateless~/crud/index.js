const matches = require("./matches")
const { connection } = require("./crud")

async function main (event, context){
  const { body, queryString, queryStringParameters, path, httpMethod, headers } = event;
  const { method } = queryString;

  // If you do not specify a databaseId, the system will automatically search for and connect to an available database.
  // 如果没有指定 databaseId 的话，sdk 会自动获取可用的数据库进行连接。
  // Please note: if uos is authorized to store your database password in an encrypted form, this SDK will automatically fill in the password, if not authorized, you will need to specify the database password in your code
  // 注意：如果uos被授权以加密形式存储您的数据库密码，本sdk将自动填入密码，如果未被授权，您需要在代码中指定数据库密码。
  // Please note: if there are multiple available postgresql databases, you must use the databaseId to specify the desired database you wish to connect to.
  // 注意：如果有多个可用的 postgresql 数据库的话，请在代码中指定要用于连接的数据库的 databaseId
  
  try {
    const client = await connection(context);

    const m = matches[method]
    if(!m) {
      throw new Error(`method ${method} in params is unrecognized`)
    }
    const body = await m(client, event, context);
    return {
      "isBase64Encoded":  false,
      "statusCode": 200,
      "headers": {"Content-Type":"application/json"},
      body,
    }
  } catch (error) {
    console.error(error);
    
    
    return {
      "isBase64Encoded":  false,
      "status": 400,
      body: {
        error: error.message
      }
    }
  }
};

exports.main =  main