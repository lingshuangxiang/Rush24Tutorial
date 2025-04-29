const axios = require("axios");
const { headers } = require("../passport/auth");
const endpoints = require("../common/endpoints");
const subscribeAPI = {
  async post(client, event, context) {
    let result = null;
    const { body } = event;
    try {
      // 获取并校验参数
      let {
        templateId,
        toUser,
        page = "",
        data,
        miniProgramState = "normal",
        lang = "zh_CN",
        notifyTime,
        slugName
      } = JSON.parse(body);
      console.log({
        templateId,
        toUser,
        page,
        data,
        miniProgramState,
        lang,
        notifyTime,
      });
      if (!templateId) throw new Error("templateId is required.");
      if (!toUser) throw new Error("toUser is required.");
      if (!notifyTime) throw new Error("notifyTime is required.");
      if (!data) throw new Error("data is required.");
      if (!slugName) throw new Error("slugName is required.");

      await client.query("BEGIN");

      const query = `
      INSERT INTO wechat_notify
      (template_id, touser, page, data, miniprogram_state, lang, notify_time, slug_name) 
      VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
      RETURNING wechat_notify_id`;

      // 模拟时间
      // notifyTime = (new Date(new Date().getTime()  + 24 * 60 * 60 *1000)).toISOString()
      // console.log({notifyTime})

      const { rows } = await client.query(query, [
        templateId,
        toUser,
        page,
        data,
        miniProgramState,
        lang,
        notifyTime,
        slugName
      ]);
      // console.log(rows);
      result = rows.map((row) => ({
        wechatNotifyId: row.wechat_notify_id,
      }))[0];

      await client.query("COMMIT");
    } catch (err) {
      await client.query("ROLLBACK");
      throw err;
    } finally {
      await client.release();
    }
    return result;
  },
  async get(client, event, context) {
    let result = null;

    try {
      const { queryString } = event;
      const { toUser, slugName} = queryString;
      console.log({ toUser, slugName });

      await client.query("BEGIN");

      const query = `
      SELECT 
        wn.wechat_notify_id,
        wn.template_id,
        wn.touser,
        wn.page,
        wn.miniprogram_state,
        wn.lang,
        wn.data,
        wn.notify_time,
        wn.slug_name
      FROM wechat_notify wn
      WHERE wn.touser = $1
      AND wn.slug_name = $2
      AND wn.notified = false
      AND wn.notify_time >= $3
      `;

      const currentTime = new Date().toISOString();
      const { rows } = await client.query(query, [toUser, slugName, currentTime]);
      // console.log(rows);
      const list = rows.map((row) => ({
        wechatNotifyId: row.wechat_notify_id,
        templateId: row.template_id,
        toUser: row.touser,
        page: row.page,
        miniProgramState: row.miniprogram_state,
        lang: row.lang,
        notifyTime: row.notify_time,
        slugName: row.slug_name
      }));
      result = {
        list,
        count: list.length
      }

      await client.query("COMMIT");
    } catch (err) {
      await client.query("ROLLBACK");
      throw err;
    } finally {
      await client.release();
    }
    return result;
  },
};

module.exports = subscribeAPI;
