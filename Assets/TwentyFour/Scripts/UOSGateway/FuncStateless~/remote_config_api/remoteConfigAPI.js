const axios = require("axios");
const { headers } = require("../passport/auth")
const endpoints = require("../common/endpoints")

const remoteConfigAPI = {
  async getSettings(keys, types) {
    const response = await axios({
      method: "POST",
      url: `/v1/settings`,
      baseURL: endpoints.remoteConfig,
      headers,
      data: {
        keys, types
      }
    });
    console.log(response.data);
    return response.data;
  }

}

module.exports = remoteConfigAPI