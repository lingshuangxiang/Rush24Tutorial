// 测试环境
// node upload.js dev
// 正式环境
// node upload.js prod
// node upload.js prod2

const fs = require('fs');
const path = require('path');
const shell = require('shelljs');
const process = require('node:process');

// 修改文件的函数
async function updateConfigFile(filePath, currentEnvName) {
    // 读取文件内容
    const promise = new Promise((resolve, reject) => {
      fs.readFile(filePath, 'utf8', (err, data) => {
        if (err) {
            console.error('读取文件时出错:', err);
            return;
        }

        // 使用正则表达式替换环境
        const updatedData = data
            .replace(/const currentEnv = ".*?"/, `const currentEnv = "${currentEnvName}"`)


        // 写入修改后的内容回文件
        fs.writeFile(filePath, updatedData, 'utf8', (err) => {
            if (err) {
                console.error('写入文件时出错:', err);
                reject('写入文件时出错:', err)
                return;
            }

            console.log('文件已成功更新！');
            resolve("文件已成功更新！")
        });
      });
    })
    return promise;
}

const env = {
  dev: {
    // func 所在 app
    AppID: "7fa14adf-6df8-4672-8a48-a16118ae5887",
    AppServiceSecret: "f7e72d1425bb453daf8fceea4eb06a04",
  },
  // prod: {
  //   AppID: "12a1a357-52df-4a73-a888-9cf28ed75017",
  //   AppServiceSecret: "5e74d255feaf4edf84c24f4fda4be9f7",
  // },
  prod2: {
    // func 所在 app
    AppID: "07543d80-f88a-40f8-a6d1-2af532c19d60",
    AppServiceSecret: "27c29c52cabd4588babe038859fdc35a",
  }
}

const currentEnvName = process.argv.slice(2)[0] ?? "dev"
// 使用示例
const configFilePath = path.join(__dirname, "common/uosConfig.js");
const envConfig = env[currentEnvName]
if(!envConfig) {
  throw new Error("环境不存在！")
}

async function main() {
  await updateConfigFile(configFilePath, currentEnvName);
  shell.exec(`usf-cli -a --id=${envConfig.AppID} --secret=${envConfig.AppServiceSecret}`);
  shell.exec(`usf-cli -d --installDependency`)
}

main()


