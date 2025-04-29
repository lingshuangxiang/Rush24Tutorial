// node test/remoteConfigAPI.js

const remoteConfigAPI = require("../remote_config_api/remoteConfigAPI")

async function test() {
  remoteConfigAPI.getSettings(["VitalityConfig"], ["JSON"])
}

test()