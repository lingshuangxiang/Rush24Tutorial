// node common/wechatAPI.js
const axios = require("axios");
const uosConfig = require("./uosConfig")
const baseURL = "https://api.weixin.qq.com";

const wechatAPI = {
  // 获取微信 access_token
  async getAccessToken() {
    const url = `/cgi-bin/token`;

    const appid = uosConfig.wechatAppID;
    const secret = uosConfig.wechatAppSecret;

    if (!appid || !secret) {
      throw new Error("微信 appid 和 secret 配置为空");
    }
    const response = await axios({
      method: "GET",
      baseURL,
      params: {
        grant_type: "client_credential",
        appid,
        secret,
      },
      url,
    });

    const { access_token: accessToken, expires_in: expiresIn } = response.data;
    return { accessToken, expiresIn };
  },

  // 发送消息
  async sendSubscribeMessage(data) {
    const url = `/cgi-bin/message/subscribe/send`;
    const { accessToken } = await this.getAccessToken();
    console.log(`【微信通知】sendSubscribeMessage  ${data.touser}` );
    console.log(data);
    const response = await axios({
      method: "POST",
      baseURL,
      params: {
        access_token: accessToken,
      },
      url,
      data,
    });
    console.log(`【微信通知】sendSubscribeMessage  ${data.touser} 返回值` );
    console.log(response.status);
    console.log(response.data);

    return response.data;
  },
};

// async function test() {
//   const resp = await wechatAPI.getAccessToken();
//   console.log(resp)
// }
// test()

module.exports = wechatAPI;
