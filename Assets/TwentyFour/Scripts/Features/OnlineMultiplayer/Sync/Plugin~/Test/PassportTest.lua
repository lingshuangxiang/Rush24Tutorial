
-- passport 接口测试
-- lua Test/PassportTest.lua
local json = require("Utils.json")

local PassportTest = {}
local PassportSDK = require("UOSGateway.PassportSDK")
local Passport = require("Server.Passport")
-- 获得的数据格式
-- {"HttpStatusCode":200,"Data":"{\"status\":200,\"statusText\":\"OK\",\"data\":{\"scores\":[]}}"}

function PassportTest.Test()
  print("PassportTest")
  -- local resp = PassportSDK.UpdateScore("test", "1000003001", 2)
  -- print(json.encode(resp))
  -- local resp = PassportSDK.GetMemberLeaderboard("test", "1000003001")
  -- local scores = json.decode(resp.Data).data.scores
  -- local score = #scores > 0 and scores[1].score or 0
  -- print("testScore " .. score)

  -- local resp = PassportSDK.GetPersona("1000024001")
  -- local properties = json.decode(resp.Data).data.Persona.properties
  -- print(json.encode(properties))

  local personaId = "1000049003"
  local slugName = "SEASON202504_BRONZE"
  local action = PassportSDK.AchievementAction.Accumulate
  local value = 1
  local resp = PassportSDK.UnlockAchievement(personaId, slugName)
  print(json.encode(resp.Data))

  local resp2 = PassportSDK.UpdateAchievement(personaId, slugName, action, value)
  print(json.encode(resp2.Data))


end

function PassportTest.TestPerfectWin()
  Passport.SetPerfectWinLeaderboard({teamPlayers = {{ uniqueId = "12345"}}})
end

function PassportTest.TestFastSolveSingle()
  local resolvedStatus = {{
    resolved = true,
    resolvedPersonaID = 12344,
    resolvedTime = 160,
    resolvedTeam = 0,
    costTime = 2.444,
  }, {
    resolved = true,
    resolvedPersonaID = 111,
    resolvedTime = 160,
    resolvedTeam = 0,
    costTime = 5,
  }, {
    resolved = true,
    resolvedPersonaID = 111,
    resolvedTime = 160,
    resolvedTeam = 0,
    costTime = 5,
  }, {
    resolved = false,
    resolvedPersonaID = 111,
    resolvedTime = 160,
    resolvedTeam = 0,
    costTime = 5,
  }, {
    resolved = false,
    resolvedPersonaID = 111,
    resolvedTime = 160,
    resolvedTeam = 0,
    costTime = 5,
  }, }
  Passport.SetFastSolveSingleLeaderboard(resolvedStatus)
end

function PassportTest.SetFastestAverageTime(team)
  Passport.SetFastestAverageTime(team)
end

-- PassportTest.TestPerfectWin()
-- PassportTest.TestFastSolveSingle()
PassportTest.Test()
-- PassportTest.SetFastestAverageTime({teamPlayers = {{ uniqueId = "20251000006001"}}})
return PassportTest