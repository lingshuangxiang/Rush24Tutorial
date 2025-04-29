local module = {}
local ltn12 = require("ltn12")
local https = require("ssl.https")
local json = require("Utils.json")

local function GetParamsString(params)
  local paramsStr = ""
  for key, value in pairs(params) do
      if paramsStr ~= "" then
        paramsStr = paramsStr .. "&"
      end
      paramsStr = paramsStr .. key .. "=" .. tostring(value)
  end
  return paramsStr
end

module.MessageType = {

}

module.ReturnType = {
  "CONTINUE",
}

function module.LogInfo(msg)
  print(msg)
end


function module.BroadcastMessage(msg)
  module.LogInfo("BroadcastMessage\n" .. msg)
end

function module.SendMessage(id, msg)
  module.LogInfo("SendMessage")
end

function module.CancelTask(taskId)
  module.LogInfo('CancelTask')
end

function module.ScheduleRepeat(func, gap)
  module.LogInfo("ScheduleRepeat")
end

function module.ScheduleOnce(func, interval, table)
  module.LogInfo("ScheduleOnce")
  _G[func]()
end

function module.AsyncHttp(req, callback, context)
  local response_body                    = {}
  local url = req.Url
  local paramsString = GetParamsString(req.Params)
  if paramsString ~= "" then
    url = url .. "?" .. paramsString
  end
  local _, status_code, response_headers = https.request {
    url = url,
    method = req.Method,
    headers = req.Headers,
    source = ltn12.source.string(req.Data), -- 设置请求体
    sink = ltn12.sink.table(response_body)
  }
  _G[callback](context, response_body, status_code)
end

function module.SyncHttp(req)
  local response_body                    = {}
  local url = req.Url
  local paramsString = GetParamsString(req.Params)
  if paramsString ~= "" then
    url = url .. "?" .. paramsString
  end
  local _, status_code, response_headers = https.request {
    url = url,
    method = req.Method,
    headers = req.Headers,
    source = ltn12.source.string(req.Data), -- 设置请求体
    sink = ltn12.sink.table(response_body)
  }
  print(url)
  module.LogInfo(json.encode(req.Headers))

  return {
    Data = response_body[1],
    HttpStatusCode = status_code
  }
end

function module.GetRoom()
  return {
    Properties = {}
  }
end

return module