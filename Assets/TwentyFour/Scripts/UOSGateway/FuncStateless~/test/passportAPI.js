// node test/passportAPI.js

const passportAPI = require("../passport_api/passportAPI")

async function test() {
  const personaId = "1000025001"
  const resourceSlug = "VIT"
  // passportAPI.depositInventory(personaId, resourceSlug, 5)
  // const resp = await passportAPI.searchInventory(personaId, resourceSlug)
  // console.log(resp?.inventory?.[0]?.quantity)
  // 消耗资源
  passportAPI.consumeByResourceSlugInventory(personaId, resourceSlug, 50)
  // const getResourceResp = await passportAPI.getResourceInventory(resourceSlug);
  // const resource = getResourceResp?.resource;
  // console.log(resource)
}

test()