// 本地测试
// usf-cli -r --function=vitality

const { connection } = require("../crud/crud")
const vitality = require("./vitality")

async function main(event, context) {
  try {
    const { path } = event;
    const client = await connection(context);
    // 通过子路径找到对应函数
    const regex = /vitality\/([^\/]+)/; // 匹配 vitality/ 后面的部分
    const match = path.match(regex);
    if (match && match[1]) {
      const subPath = match[1]; // 提取到的子路径
      const body = await vitality[subPath](client, event, context);

      return {
        "isBase64Encoded":  false,
        "statusCode": 200,
        "headers": {"Content-Type":"application/json"},
        body,
      }
    } else {
      throw new Error("unknown sub path")
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