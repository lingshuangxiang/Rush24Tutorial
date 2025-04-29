const axios = require("axios");
const { headers } = require("../passport/auth");
const endpoints = require("../common/endpoints");
const { achievement } = require("./achievementAPI");

const passportAPI = {
  // 成就 API
  achievement,
  // 获取 persona 信息
  async getPersona(personaID) {
    const response = await axios({
      method: "GET",
      url: `/v1/personas/${personaID}`,
      baseURL: endpoints.passport,
      headers,
      params: {
        personaID,
      },
    });
    console.log(response);
    return response;
  },
  // 经济系统 - 搜索背包信息
  async searchInventory(personaId, resourceSlug, start = 0, count = 10) {
    const response = await axios({
      method: "POST",
      url: `/v1/inventory/search`,
      baseURL: endpoints.passport,
      headers,
      data: {
        personaId,
        resourceSlug,
        start,
        count,
      },
    });
    console.log(JSON.stringify(response.data, null, 2));
    return response.data;
  },
  // 经济系统 - 增加角色背包内资源
  async depositInventory(
    personaId,
    resourceSlug,
    depositQuantity,
    customData = {}
  ) {
    const url = `/v1/personas/${personaId}/inventory/deposit`;

    const response = await axios({
      method: "POST",
      url,
      baseURL: endpoints.passport,
      headers,
      data: {
        depositResources: [
          {
            resourceSlug,
            depositQuantity,
            customData,
          },
        ],
      },
    });
    console.log(JSON.stringify(response.data, null, 2));
    return response.data;
  },
  // 经济系统 - 消耗角色背包内资源
  async consumeInventory(personaId, inventoryItemId, consumeQuantity) {
    const url = `/v1/personas/${personaId}/inventory/consume`;

    const response = await axios({
      method: "POST",
      url,
      baseURL: endpoints.passport,
      headers,
      data: {
        inventoryItems: [
          {
            inventoryItemId,
            consumeQuantity,
          },
        ],
      },
    });
    console.log(JSON.stringify(response.data, null, 2));
    return response.data;
  },
  async consumeByResourceSlugInventory(
    personaId,
    resourceSlug,
    consumeQuantity
  ) {
    const resp = await this.searchInventory(
      personaId,
      resourceSlug,
      (start = 0),
      (count = 1)
    );
    console.log(resp);
    const inventoryItemId = resp?.inventory?.[0]?.inventoryItemId;
    if (!inventoryItemId) {
      throw new Error("unknown resource slug name");
    }
    return this.consumeInventory(personaId, inventoryItemId, consumeQuantity);
  },
  // 经济系统 - 获取资源信息
  async getResourceInventory(resourceSlug) {
    const url = `/v1/resources/${resourceSlug}`;
    const response = await axios({
      method: "GET",
      url,
      baseURL: endpoints.passport,
      headers,
    });
    console.log(JSON.stringify(response.data, null, 2));
    return response.data;
  },
};

module.exports = passportAPI;
