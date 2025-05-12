const axios = require("axios");
const { headers } = require("../passport/auth")
const endpoints = require("../common/endpoints")

const achievementAction = {
  "Increase": "Increase",
  "Reduce": "Reduce",
  "Reset": "Reset",
  "Accumulate": "Accumulate"
}

const achievement = {
  async updateAchievements(personaId, slugName, action, value) {
    const url = `/v1/personas/${personaId}/achievements`
    const response = await axios({
      method: "POST",
      url,
      baseURL: endpoints.passport,
      headers,
      params: {
        personaId,
      },
      data: {
        slugName,
        action,
        value
      }
    });
    console.log(response);
    return response;
  },

}

module.exports = { achievement, achievementAction };