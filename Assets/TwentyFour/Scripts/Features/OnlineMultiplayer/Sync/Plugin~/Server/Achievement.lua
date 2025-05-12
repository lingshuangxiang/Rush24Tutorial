local PassportSDK = require("UOSGateway.PassportSDK")
local json = require("Utils.json")
local MuninnPlugin = require("MuninnPlugin")

local tierAchievementConfig = {
  "SEASON202504_BRONZE",
  "SEASON202504_SLIVER",
  "SEASON202504_GOLD",
  "SEASON202504_DIAMOND"
}

local Achievement = {}

function Achievement.UpdateAchievement(personaId, slugName, value)
  local action = PassportSDK.AchievementAction.Accumulate
  local resp = PassportSDK.UnlockAchievement(personaId, slugName)
  MuninnPlugin.LogInfo("unlock response: ")
  MuninnPlugin.LogInfo(json.encode(resp.Data))
  local resp2 = PassportSDK.UpdateAchievement(personaId, slugName, action, value)
  MuninnPlugin.LogInfo("update response: ")
  MuninnPlugin.LogInfo(json.encode(resp2.Data))
end

function Achievement.UpdateAchievements(personaId, value)
  -- 更新
  for index, slugName in ipairs(tierAchievementConfig) do
    MuninnPlugin.LogInfo("update " .. value)
    Achievement.UpdateAchievement(personaId, slugName, value)
  end
end

function Achievement.UpdateTeamAchievement(team)
  for i = 1, #team.teamPlayers do
    local player = team.teamPlayers[i]
    if not player.isRobot then
      Achievement.UpdateAchievements(player.uniqueId, player.currentScore )
    end
  end
end

return Achievement;