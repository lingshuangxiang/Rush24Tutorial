const uosConfig = require("../common/uosConfig")

function getBasicAuthorization(appId, appSecret) {
    // 获取 Basic Auth 的 Header
    const credentials = `${appId}:${appSecret}`;
    const encodedCredentials = Buffer.from(credentials).toString('base64');
    return { Authorization: `Basic ${encodedCredentials}` };
}

const headers = getBasicAuthorization(uosConfig.math24AppID, uosConfig.math24AppServiceSecret);

module.exports = {
    headers
}
