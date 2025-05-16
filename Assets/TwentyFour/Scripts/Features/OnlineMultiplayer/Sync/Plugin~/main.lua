local json = require("Utils.json")
local server = require("Server.server")
local robot = require("Server.robot")
local utils = require("Utils.utils")

local ClientMessageType = server.ClientMessageType

local MuninnPlugin = require("MuninnPlugin")
local MessageType = MuninnPlugin.MessageType
local ReturnType = MuninnPlugin.ReturnType
local Timer = require("Utils.timer")

-- 倒计时
local TotalTime = 185;
RemainTime = TotalTime;
local taskId = nil

-- 游戏对战中
gameing = false; -- 全局变量
local currentRoom = nil;

local function EndGame()
  if not gameing then
    return
  end
  gameing = false
  server.HandleBattleEnd()
  -- 下发游戏结束消息
  MuninnPlugin.BroadcastMessage(json.encode(server.GetEndGameData()))
end

-- [生命周期] 创建房间成功后
function OnCreateRoom(room)
  currentRoom = room;
end

-- 定时任务函数
function SendMessage()
end

-- [生命周期] 关闭房间前
function BeforeCloseRoom(room)
  MuninnPlugin.LogInfo("[plugin] BeforeCloseRoom")
end

-- [生命周期] 关闭房间后
function OnClose(room)
  MuninnPlugin.LogInfo("[plugin] OnClose")
end

-- [生命周期] 玩家加入房间前
function BeforeJoin(player)
  MuninnPlugin.LogInfo("[plugin] BeforeJoin")
  return MuninnPlugin.ReturnType.CONTINUE
end

-- [生命周期] 玩家加入房间后
function OnJoin(player)
  MuninnPlugin.LogInfo("[plugin] OnJoin")
end

-- [生命周期] 玩家离开房间
function OnLeave(player)
  MuninnPlugin.LogInfo("[plugin] OnLeave")
end

-- [生命周期] 玩家发起的事件
function OnMessage(msg)
  local decodedMsg = json.decode(msg.Data)  
  if decodedMsg.type == ClientMessageType.StartGame then
    -- 开始游戏，分发牌组
    StartGame(decodedMsg)
  elseif decodedMsg.type == ClientMessageType.SubmitAnswer then
    -- 处理提交答案
    HandleSubmitAnswer(msg.SenderId, decodedMsg)    
  elseif decodedMsg.type == ClientMessageType.CustomOnceMoreRequest then
    local serverMessage = {
      type = ClientMessageType.CustomOnceMoreResponse,
      senderId = msg.SenderId,
    }
    MuninnPlugin.LogInfo("---- CustomOnceMoreResponse ----")
    MuninnPlugin.BroadcastMessage(json.encode(serverMessage))
  elseif decodedMsg.type == ClientMessageType.SyncStatus then
    MuninnPlugin.BroadcastMessage(json.encode(decodedMsg))
  end

  return MuninnPlugin.ReturnType.CONTINUE
end

-- 处理提交答案
function HandleSubmitAnswer(senderId, decodedMsg)
  -- 当前时间
  local currentTime = Timer.GetCurrentSeconds();
  MuninnPlugin.LogInfo("current time is: " .. tostring(currentTime))
  -- 玩家提交答案
  local judgeResult = server.judgeAnswer(decodedMsg.answerExpressions, server.CurrentQuestions[decodedMsg.answerIndex])
  local serverMessage = {
    judgeResult = judgeResult,
    type = ClientMessageType.JudgeResult
  }
  -- 将结果下发给提交结果的人
  MuninnPlugin.LogInfo("send to " .. tostring(senderId))
  MuninnPlugin.SendMessage(senderId, json.encode(serverMessage))
  -- 给所有人下发比分和进度同步事件
  -- 题目完成时才触发
  if judgeResult then

    local newProgress = server.updateProgress(decodedMsg.personaID, decodedMsg.answerIndex, currentTime)
    -- 有新进展才同步
    if not newProgress then
      return
    end
    local progress = server.getProgress();

    SendProgressMessage();

    -- 通知机器人进展
    if robot.IsRobotRoom then 
      robot.NotifyProgress(progress)
    end
    
    -- 游戏结束处理
    if progress.allResolved then
      -- allResolved 不是指所有题目完成，是指游戏已经结束（3:1时游戏也结束）
      EndGame()
      CancelRemainTask();
    end
  end
end

-- 下发进度同步消息
function SendProgressMessage()
  local progress = server.getProgress();
  progress.type = ClientMessageType.SyncProgress
  -- 下发进度
  MuninnPlugin.LogInfo("---- sync progress ----")
  MuninnPlugin.LogInfo(json.encode(progress))
  MuninnPlugin.BroadcastMessage(json.encode(progress))
end

-- 开始游戏
function StartGame(decodedMsg)
  -- 已经在游戏中
  if gameing then
    return;
  end
  Timer.Reset()
  currentRoom = MuninnPlugin.GetRoom()
  robot.Init(currentRoom);

  MuninnPlugin.LogInfo("is robot room: " .. tostring(robot.IsRobotRoom))

  -- 机器人房间，获取队伍信息
  if robot.IsRobotRoom then
    robot.SetRobotInfo(decodedMsg.blueTeamProgress.teamPlayers[1])
  end

  server.GenQuestions(decodedMsg.size or 5, decodedMsg.useAdvanceQuestionsRate or 0)
  -- 牌局数据和组队数组
  server.InitProgress(decodedMsg, currentRoom)
  local progress = server.getProgress();
  local serverMessage = {
    battleMode = server.GetBattleMode(),
    type = ClientMessageType.Distribute,
    stages = server.CurrentQuestions,
    redTeamProgress = progress.redTeamProgress,
    blueTeamProgress = progress.blueTeamProgress,
    resolvedStatus = progress.resolvedStatus,
  }
  MuninnPlugin.BroadcastMessage(json.encode(serverMessage))
  -- 开始倒计时
  if decodedMsg.customTime ~= nil and decodedMsg.customTime ~= -1 then
    TotalTime = decodedMsg.customTime;
  end
  RemainTime = TotalTime;
  CancelRemainTask()
  taskId, err = MuninnPlugin.ScheduleRepeat("CountDown", 1000)
  gameing = true;

  if robot.IsRobotRoom then
    robot.StartRobot()
  end
end

-- 取消遗留的 Task
function CancelRemainTask()
  if taskId ~= nil then
    MuninnPlugin.CancelTask(taskId)
    taskId = nil
  end
end

-- 倒计时
function CountDown()
  -- MuninnPlugin.LogInfo("RemainTime: " .. RemainTime)
  RemainTime = RemainTime - 1;
  if RemainTime < 0 then
    RemainTime = 0;
    CancelRemainTask()
    -- 计时结束
    EndGame()
  end
  local serverMessage = {
    type = ClientMessageType.CountDown,
    remainTime = RemainTime,
    battleMode = server.GetBattleMode(),
  }
  MuninnPlugin.BroadcastMessage(json.encode(serverMessage))
end