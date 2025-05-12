// 测试
// usf-cli -r --function=player_info

const { connection } = require("../crud/crud")
const playerInfo = require("./playerInfo")

async function main(event, context) {
  try {
    const client = await connection(context);
    const body = await playerInfo.getStatics(client, event, context);
    console.log(body)

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
}

exports.main =  main