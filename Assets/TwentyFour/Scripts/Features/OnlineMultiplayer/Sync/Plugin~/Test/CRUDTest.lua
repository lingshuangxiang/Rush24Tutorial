-- lua Test/CRUDTest.lua
local json = require("Utils.json")
local MuninnPlugin = require("MuninnPlugin")
local Endpoints = require("UOSGateway.Endpoints")

local CRUDTest = {}

function CRUDTest.test()
  local params = {
      method = "get_matches",
      uniqueId = "1000006001"
  }
  local req = {
    Method = "GET",   -- GET, POST
    Url = Endpoints.CRUD,
    Params = params,
    -- Data = json.encode(body), -- body，lua字符串和二进制串是一致的
  }
  MuninnPlugin.LogInfo("req send by UpdateScore")
  MuninnPlugin.LogInfo(json.encode(req))
  return MuninnPlugin.SyncHttp(req)
end

local result = CRUDTest.test()
print(json.encode(result.Data))


return CRUDTest;
