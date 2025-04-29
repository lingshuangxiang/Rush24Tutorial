const axios = require("axios")
const endpoints = require("../common/endpoints")
const { headers } = require("./auth")
async function passport(event) {
  const { body = "{}", queryString, queryStringParameters, path, httpMethod } = event;
  const requestBody = JSON.parse(body)
  const regex = /\/passport(\/v1.*)/;
  const url = path.match(regex)[1];
  
  try {
    const response = await axios({
      method: httpMethod,
      url,
      baseURL: endpoints.passport,
      data: requestBody,
      headers,
      params: queryString
    })
    const { status, statusText, data } = response
    return { status, statusText, data };
  } catch(error) {
    console.error('Error:', error);
    return error;
  }
}

module.exports = passport