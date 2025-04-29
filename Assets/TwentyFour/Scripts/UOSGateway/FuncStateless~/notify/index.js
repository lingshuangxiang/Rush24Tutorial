const { connection } = require("../crud/crud")
const notify = require("./notify")

async function main(event, context) {
  try {
    const client = await connection(context);
    const body = await notify(client, event, context);
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