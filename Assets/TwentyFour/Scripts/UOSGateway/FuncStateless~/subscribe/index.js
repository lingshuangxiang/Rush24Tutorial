const { connection } = require("../crud/crud");
const subscribeAPI = require("./subscribeAPI");

async function main(event, context) {
  try {
    const { httpMethod } = event;
    const client = await connection(context);

    const body = await subscribeAPI[httpMethod.toLowerCase()](client, event, context);
    console.log(body);

    return {
      isBase64Encoded: false,
      statusCode: 200,
      headers: { "Content-Type": "application/json" },
      body,
    };
  } catch (error) {
    console.error(error);

    return {
      isBase64Encoded: false,
      status: 400,
      body: {
        error: error.message,
      },
    };
  }
}

exports.main = main;
