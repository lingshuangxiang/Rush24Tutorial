local Endpoints = require("UOSGateway.Endpoints")
local MuninnPlugin = require("MuninnPlugin")
local json = require("Utils.json")

local CRUD = {}

function CRUDCallBack(context, rsp, err)
  MuninnPlugin.LogInfo("上传战绩表结果")
  MuninnPlugin.LogInfo(json.encode(rsp))
end

function CRUD.SetMatch(data, callBackFunctionName, context) 
  local params = {
    method = "set_match",
  }
  local req = {
    Method = "POST",   -- GET, POST
    Url = Endpoints.CRUD,
    Params = params,
    Data = json.encode(data), -- body，lua字符串和二进制串是一致的
  }
  MuninnPlugin.LogInfo(json.encode(req))
  MuninnPlugin.AsyncHttp(req, callBackFunctionName, context)
end

return CRUD;