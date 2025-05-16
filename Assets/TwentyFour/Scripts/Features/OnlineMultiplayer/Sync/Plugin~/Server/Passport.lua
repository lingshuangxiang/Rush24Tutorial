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




return Passport;