local Passport = {}
local PassportSDK = require("UOSGateway.PassportSDK")
local MuninnPlugin = require("MuninnPlugin")
local Tournament = require("Server.Tournament")

local json = require("Utils.json")
local defaultLeaderboardSlugName = "SEASON202504Leaderboard"
local leaderboardSlugName = defaultLeaderboardSlugName
local perfectWinSlugName = "UOSTournamentLeaderboardSlugPerfectWin"
local fastSolveSingleSlugName = "UOSTournamentLeaderboardSlugFastSolveSingle"
local FastestAverageTimeSlugName = "UOSTournamentLeaderboardSlugFastestAverageTime"

local tournamentSlugName = "";

local MinScore = 0

function Passport.Init(roomProperties)
  if roomProperties.tournament_slug_name ~= nil and roomProperties.tournament_slug_name ~= "" then
    leaderboardSlugName = roomProperties.TournamentLeaderboardSlug_Main
    perfectWinSlugName = roomProperties.TournamentLeaderboardSlug_PerfectWin
    fastSolveSingleSlugName = roomProperties.TournamentLeaderboardSlug_FastSolveSingle
    FastestAverageTimeSlugName = roomProperties.TournamentLeaderboardSlug_FastestAverageTime
    tournamentSlugName = roomProperties.tournament_slug_name
  else
    leaderboardSlugName = defaultLeaderboardSlugName
  end
end

-- 增加完胜次数
function Passport.SetPerfectWinLeaderboard(team)
  -- 完胜次数+1
  for i = 1, #team.teamPlayers do
    local player = team.teamPlayers[i]
    if not player.isRobot then
      MuninnPlugin.LogInfo("add perfect win count for ".. tostring(player.uniqueId))
      local resp = PassportSDK.UpdateScore(perfectWinSlugName, tostring(player.uniqueId), 1)
      MuninnPlugin.LogInfo(json.encode(resp))
    end
  end
  
end

-- 单题最快
function Passport.SetFastSolveSingleLeaderboard(resolvedStatus)
  for i = 1, #resolvedStatus do
    local status = resolvedStatus[i]
    if status.resolved then
      MuninnPlugin.LogInfo("resolved by ".. tostring(status.resolvedPersonaID) .. " cost time: " .. tostring(status.costTime))
      local resp = PassportSDK.UpdateScore(fastSolveSingleSlugName, tostring(status.resolvedPersonaID), status.costTime)
      MuninnPlugin.LogInfo(json.encode(resp))
    end
  end
  
end

-- 更新平均耗时
function Passport.SetFastestAverageTime(team)
  for i = 1, #team.teamPlayers do
    local player = team.teamPlayers[i]
      local myScore = Tournament.GetTournamentScore(tournamentSlugName, player.uniqueId)
      if myScore.avgTime == nil then
        return;
      end
      local avgTime = myScore.avgTime;
      local resp = PassportSDK.UpdateScore(FastestAverageTimeSlugName, tostring(player.uniqueId), avgTime)
      MuninnPlugin.LogInfo(json.encode(resp))
  end
end

function Passport.GetPersonaInfo(team)
  for i = 1, #team.teamPlayers do
    local player = team.teamPlayers[i]
    -- 仅非机器人获取排名
    if not player.isRobot then
      local persona = PassportSDK.GetPersona(player.uniqueId)
      player.properties = persona.properties
      MuninnPlugin.LogInfo("get persona properties: " .. json.encode(persona.properties))
    end
  end
end

function Passport.GetScores(team)
  for i = 1, #team.teamPlayers do
    local player = team.teamPlayers[i]
    -- 仅非机器人获取排名
    if not player.isRobot then
      local resp = PassportSDK.GetMemberLeaderboard(leaderboardSlugName, player.uniqueId)
      MuninnPlugin.LogInfo(json.encode(resp))
      if resp.Data == nil or json.decode(resp.Data).data == nil then
        MuninnPlugin.LogInfo("fail to get user score")
        player.previousScore = 0
        return
      end
      local scores = json.decode(resp.Data).data.scores
      local score = #scores > 0 and scores[1].score or 0
      local tier = #scores > 0 and scores[1].tier or "石头"
      MuninnPlugin.LogInfo("player " .. player.uniqueId .. " score: " .. score)
      player.previousTier = tier
      player.previousScore = score
      player.currentTier = tier
      player.currentScore = score
    end
  end
end

function Passport.UpdateScore(team, changedScore)
  for i = 1, #team.teamPlayers do
    local player = team.teamPlayers[i]
    -- 更新每个玩家的分数变化： 0 平局，-1 -2 失败，+1 +2 成功，-2 完败，+2 完胜
    player.battleResult = changedScore
    if not player.isRobot then
      local score = player.previousScore + changedScore
      if score < MinScore then
        score = MinScore
        -- if player.previousScore == 0 then
        --   MuninnPlugin.LogInfo(player.displayName .. " 之前没有分数，且最新分数低于限制最低分，则不更新")
        --   -- 如果之前没有分数，且最新分数低于限制最低分，则不更新
        --   return
        -- end
      end

      print(leaderboardSlugName, player.uniqueId, score)
      local resp = PassportSDK.UpdateScore(leaderboardSlugName, tostring(player.uniqueId), score)
      MuninnPlugin.LogInfo(json.encode(resp))
      if resp.Data == nil or json.decode(resp.Data).data == nil then
        MuninnPlugin.LogInfo("fail to get user score")
        return
      end
      local data = json.decode(resp.Data).data
      local score = data.score or 0
      local tier = data.tier or ""
      player.currentTier = tier
      player.currentScore = score
      MuninnPlugin.LogInfo(json.encode(resp))
    end
  end
end

return Passport;