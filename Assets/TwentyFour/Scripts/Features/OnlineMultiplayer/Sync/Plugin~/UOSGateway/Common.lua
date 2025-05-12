local Common = {}

local UOSConfig = require("UOSGateway.UOSConfig")
local base64 = require("Utils.base64")

function Common.GetBasicAuthorizationHeaders()
  local credentials = UOSConfig.AppID .. ":" .. UOSConfig.AppServiceSecret
  local encoded_credentials = base64.encode(credentials)
  return { Authorization = "Basic " .. encoded_credentials }
end

return Common
