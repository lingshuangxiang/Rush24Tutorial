local Endpoints = require("UOSGateway.Endpoints")
local MuninnPlugin = require("MuninnPlugin")
local json = require("Utils.json")

local Tournament = {}

function Tournament.GetTournamentScore(tournamentSlugName, uniqueId)
  local params = {
    tournamentSlugName = tournamentSlugName,
    uniqueId =  uniqueId
  }
  local req = {
    Method = "GET",   -- GET, POST
    Url = Endpoints.Tournament,
    Params = params,
  }
  MuninnPlugin.LogInfo(json.encode(req))
  local resp = MuninnPlugin.SyncHttp(req)
  MuninnPlugin.LogInfo(json.encode(json.decode(resp.Data)[1]))
  return json.decode(resp.Data)[1]; 
end

return Tournament;