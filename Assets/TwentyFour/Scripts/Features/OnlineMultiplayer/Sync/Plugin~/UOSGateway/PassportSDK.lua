local MuninnPlugin = require("MuninnPlugin")
local PassportSDK = {}
local json = require("Utils.json")
local Endpoints = require("UOSGateway.Endpoints")
local Common = require("UOSGateway.Common")


function PassportSDK.GetPersona(personaID)
  local url = "/v1/personas/" .. personaID
  -- local headers = Common.GetBasicAuthorizationHeaders()

  local params = {
    personaID = personaID
  }
  local req = {
    Method = "GET",   -- GET, POST
    Url = Endpoints.Passport .. url,
    Params = params,       -- 参数
    -- Headers = headers
  }
  local resp = MuninnPlugin.SyncHttp(req)
  return  json.decode(resp.Data).data.Persona or {}
end 

function PassportSDK.UpdateAchievement(personaId, slugName, action, value)
  local url = "/v1/personas/" .. personaId .. "/achievements"
  local body = {
    slugName = slugName,
    action = action,
    value = value
  }
  local req = {
    Method = "POST",   -- GET, POST
    Params = {},
    Url = Endpoints.Passport .. url,  
    Data = json.encode(body), -- body，lua字符串和二进制串是一致的
  }
  return MuninnPlugin.SyncHttp(req)
end

function PassportSDK.UnlockAchievement(personaId, slugName)
  local url = "/v1/personas/" .. personaId .. "/achievements/meta/" .. slugName .. "/unlock"
  local req = {
    Method = "POST",   -- GET, POST
    Params = {},
    Url = Endpoints.Passport .. url,   
    Data = json.encode({}), -- body，lua字符串和二进制串是一致的
  }
  return MuninnPlugin.SyncHttp(req)
end

PassportSDK.AchievementAction = {
  Increase = "Increase",
  Reduce = "Reduce",
  Reset = "Reset",
  Accumulate = "Accumulate"
}

return PassportSDK