local server = {}
local json = require("Utils.json")
local MuninnPlugin = require("MuninnPlugin")
local utils = require("Utils.utils")
local Questions = require("Server.Questions")
local Passport = require("Server.Passport")
local CRUD = require("Server.CRUD")
local Achievement = require("Server.Achievement")

-- 记录比分和进度数据
local redTeamProgress;
local blueTeamProgress;
local resolvedStatus;
local currentResolvedTeam;
local currentResolvedPersonaID;
local currentResolvedQuestionIndex;
local resolvedCount = 0;
local stageSize = 0;
local battleMode = nil;
local lastResolvedTimeMap = {}; -- 上次回答问题的秒数，用于计算当前题目花费的秒数。根据玩家 id 进行 map

local currentRoom = {};
local startTime = "";
local endTime = "";

server.CurrentQuestions = {}

local ClientMessageType = {
  StartGame = "StartGame",       -- 开始游戏
  Distribute = "Distribute",     -- 【下发】题目
  SubmitAnswer = "SubmitAnswer", -- 提交答案
  JudgeResult = "JudgeResult",   -- 【下发】结果
  SyncProgress = "SyncProgress", -- 【下发】团队进度
  CountDown = "CountDown",        -- 【下发】倒计时
  AllBattleData = "AllBattleData", -- 对局信息
  CustomOnceMoreRequest = "CustomOnceMoreRequest", -- 再来一局请求
  CustomOnceMoreResponse = "CustomOnceMoreResponse", -- 再来一局相应
  SyncStatus = "SyncStatus", -- 【转发】同步选题状态
  EndGame = "EndGame", -- 【下发】结束游戏
}

server.ClientMessageType = ClientMessageType

local TeamTag = {
  RED = 0,
  BLUE = 1
}

local BattleMode = {
  None = 0,
  OneOnOne = 1,
  OneOnOneCustom = 2,
  TwoOnTwo = 3,
  Tournament = 4
}

-- 判断 Player 是否在指定队伍中
local function hasCurrentPlayer(playerId, teamProgress)
  for i = 1, #teamProgress.teamPlayers do
    local player = teamProgress.teamPlayers[i]
    if player.uniqueId == playerId then
      return "true"
    end
  end
  return "false"
end

-- 找到指定玩家
local function findPlayer(playerId)
  for i = 1, #redTeamProgress.teamPlayers do
    local player = redTeamProgress.teamPlayers[i]
    if player.uniqueId == playerId then
      return player
    end
  end

  for i = 1, #blueTeamProgress.teamPlayers do
    local player = blueTeamProgress.teamPlayers[i]
    if player.uniqueId == playerId then
      return player
    end
  end
end

-- 生成对战卡片组
function server.GenQuestions(n, useAdvanceQuestionsRate)
  local useAdvanceQuestions = utils.RandomPass(useAdvanceQuestionsRate)
  stageSize = n
  server.CurrentQuestions = Questions.GenQuestions(n, useAdvanceQuestions)
end

-- 校验答案是否正确
function server.judgeAnswer(answerExpressions, stage)
  if #answerExpressions == 3 and math.abs(answerExpressions[3].Result - 24) < 0.01 then
    return true
  end
  return false
end

-- 仅测试使用
function server.SetBattleMode(newBattleMode)
  battleMode = newBattleMode
end

function server.GetBattleMode()
  return battleMode;
end

-- 初始化队伍比分和进度信息
function server.InitProgress(clientMessage, room)
  redTeamProgress = clientMessage.redTeamProgress;
  blueTeamProgress = clientMessage.blueTeamProgress;
  resolvedStatus = clientMessage.resolvedStatus;
  currentResolvedTeam = nil;
  currentResolvedPersonaID = nil;
  currentResolvedQuestionIndex = 0;
  resolvedCount = 0;
  battleMode = clientMessage.battleMode;
  currentRoom = room;
  lastResolvedTimeMap = {};

  -- 设置锦标赛信息
  local roomProperties = room.Properties
  MuninnPlugin.LogInfo(json.encode(roomProperties))
  server.tournamentSlugName = roomProperties.tournament_slug_name or ""
  Passport.Init(roomProperties); -- 根据房间 properties 设置排行榜信息

  -- 获取玩家信息
  Passport.GetPersonaInfo(clientMessage.redTeamProgress)
  Passport.GetPersonaInfo(clientMessage.blueTeamProgress)

  startTime = utils.GetFormattedTime();
end

-- 更新队伍比分和进度信息
function server.updateProgress(playerId, answerIndex, currentTime)
  -- 记录玩家 ID
  currentResolvedPersonaID = playerId
  currentResolvedQuestionIndex = answerIndex
  -- 找到所在队伍
  local inRedTeam = hasCurrentPlayer(playerId, redTeamProgress)
  local currentTeam;
  if inRedTeam == "true" then
    currentTeam = redTeamProgress
    currentResolvedTeam = TeamTag.RED
  else
    currentTeam = blueTeamProgress
    currentResolvedTeam = TeamTag.BLUE
  end

  MuninnPlugin.LogInfo(tostring(playerId) .. " is " .. currentResolvedTeam)
  local resolved = currentTeam.resolved[answerIndex + 1] or resolvedStatus[answerIndex + 1].resolved
  -- 已经统计过，不再更新分数
  if resolved then
    return false
  end
  resolvedCount = resolvedCount + 1
  resolvedStatus[answerIndex + 1].resolved = true
  resolvedStatus[answerIndex + 1].resolvedPersonaID = playerId
  resolvedStatus[answerIndex + 1].resolvedTime = currentTime
  resolvedStatus[answerIndex + 1].resolvedTeam = currentResolvedTeam
  currentTeam.resolved[answerIndex + 1] = true
  currentTeam.score = currentTeam.score + 1

  -- 更新玩家信息
  -- 更新解决题目数
  local currentPlayer = findPlayer(playerId)
  if currentPlayer.resolvedCount == nil then
    currentPlayer.resolvedCount = 0
  end
  currentPlayer.resolvedCount = currentPlayer.resolvedCount + 1

  -- 计算上次回答时间
  local key = tostring(playerId)
  local lastResolvedTime = lastResolvedTimeMap[key] or 0;
  resolvedStatus[answerIndex + 1].costTime = currentTime - lastResolvedTime;
  lastResolvedTimeMap[key] = currentTime;
  MuninnPlugin.LogInfo(json.encode(lastResolvedTimeMap))
  return true
end

-- 获取队伍比分和进度信息
function server.getProgress()
  
  -- 3:1直接结束游戏
  local redTeamScore = redTeamProgress.score
  local blueTeamScore = blueTeamProgress.score
  local redWin = redTeamScore == 3 and blueTeamScore >= 1
  local blueWin = blueTeamScore == 3 and redTeamScore >= 1
  -- 判断是否有一方以 3:1 或 3:2的比分获胜
  local hasWinner = redWin or blueWin
  MuninnPlugin.LogInfo("has winner: " .. tostring(hasWinner))

  return {
    battleMode = battleMode,
    redTeamProgress = redTeamProgress,
    blueTeamProgress = blueTeamProgress,
    resolvedStatus = resolvedStatus,
    currentResolvedPersonaID = currentResolvedPersonaID,
    currentResolvedTeam = currentResolvedTeam,
    currentResolvedQuestionIndex = currentResolvedQuestionIndex,
    allResolved = (resolvedCount == stageSize) or hasWinner
  }
end

-- 获取当前对局信息
function server.GetAllBattleData()
  local serverMessage = {
    battleMode = battleMode,
    type = ClientMessageType.AllBattleData,
    stages = server.CurrentQuestions,  -- 题目信息
    redTeamProgress = redTeamProgress, -- 做题进度
    blueTeamProgress = blueTeamProgress,
    resolvedStatus = resolvedStatus,
    allResolved = resolvedCount == stageSize,
    currentResolvedTeam = -1,
    currentResolvedPersonaID = -1,
    currentResolvedQuestionIndex = -1,
  }
  return serverMessage
end

-- 处理战局结束逻辑
function server.HandleBattleEnd()
  endTime = utils.GetFormattedTime();

  server.SetMatchData()
end

-- 上传战绩表
function server.SetMatchData()
  local matchRecord = {
    stages = server.CurrentQuestions,  -- 题目信息
    redTeamProgress = redTeamProgress, -- 做题进度
    blueTeamProgress = blueTeamProgress,
    resolvedStatus = resolvedStatus,
    allResolved = resolvedCount == stageSize,
    roomID = currentRoom.Id,
    startTime = startTime,
    endTime = endTime,
    battleMode = battleMode,
    tournamentSlugName = server.tournamentSlugName
  }
  print("战绩表信息：")
  print(json.encode(matchRecord))
  local context = {
    blueTeamProgress = blueTeamProgress,
    redTeamProgress = redTeamProgress,
  }
  CRUD.SetMatch(matchRecord, "UpdateAvgTimeAfterSetMatch", context)
end

-- 不能直接使用上下文变量，请在 context 中传递
function UpdateAvgTimeAfterSetMatch(context, rsp, err)
  if battleMode == BattleMode.Tournament then
    MuninnPlugin.LogInfo(rsp)
    Passport.SetFastestAverageTime(context.blueTeamProgress)
    Passport.SetFastestAverageTime(context.redTeamProgress)
  end

  -- 设置排位赛成就解锁逻辑
  if battleMode == BattleMode.OneOnOne then
    Achievement.UpdateTeamAchievement(context.blueTeamProgress)
    Achievement.UpdateTeamAchievement(context.redTeamProgress)
  end
end

function server.GetEndGameData()
    local serverMessage = {
      battleMode = battleMode,
      type = ClientMessageType.EndGame,
      stages = server.CurrentQuestions,  -- 题目信息
      redTeamProgress = redTeamProgress, -- 做题进度
      blueTeamProgress = blueTeamProgress,
      resolvedStatus = resolvedStatus,
      allResolved = resolvedCount == stageSize,
      currentResolvedTeam = -1,
      currentResolvedPersonaID = -1,
      currentResolvedQuestionIndex = -1,
    }
    return serverMessage
end


return server 