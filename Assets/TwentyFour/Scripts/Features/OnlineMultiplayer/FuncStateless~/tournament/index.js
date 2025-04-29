// 本地调试
// usf-cli -r --function=tournament

const { connection } = require("../crud/crud")
const tournament = require("./tournament")

async function main(event, context) {
  const { body, queryString, queryStringParameters, path, httpMethod, headers } = event;
  try {
    const client = await connection(context);
    const body = await tournament.getScore(client, queryString);

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