local robot = {}
local MuninnPlugin = require("MuninnPlugin")
local json = require("Utils.json")
local Utils = require("Utils.utils")
local Server = require("Server.server")
local RankData = require("Data.RankData")
local RobotAbility = require("Server.RobotAbility")

-- 机器人参数配置
local RobotConfig = {
  MinRandomTime = -3,
  MaxRandomTime = 10,
  LowerTimeBound = 3, -- 最少的完成一道题目的时间
  UpperTimeBound = 40, -- 在 UpperTimeBound 时间内未完成，则放弃
  ResolutionRateBound = 40, -- 解决率低于 ResolutionRateBound 的题目放弃
}

-- 全局参数，避免函数传参
local currentIndex = 0
local currentQuestion
local currentAverage
local currentResolutionRate
local useRandomOrderToAnserQuestion = true -- 是否使用随机顺序回答问题

local currentTaskId = nil

-- 生成排名数据 map
local RankDataMap = {}
function GenRankDataMap()
  for _, item in ipairs(RankData) do
    -- 使用每一项的 Key 作为 result_table 的键
    local key = item.Key
    -- 将整个项作为值
    RankDataMap[key] = item
  end
end

-- 初始化机器人状态，名称等
function robot.Init(room)
  robot.IsRobotRoom = room.Properties.ISROBOT == "true"
  Server.IsRobotRoom = robot.IsRobotRoom
  -- 读取其他参数
  if room.Properties.ROBOTCONFIG ~= nil then
    local config = json.decode(room.Properties.ROBOTCONFIG)
    RobotConfig = config
    RobotAbility.Init(config)
  end
  useRandomOrderToAnserQuestion = Utils.RandomPass(RobotConfig.UseRandomOrderToAnserQuestionRate or 100)
  MuninnPlugin.LogInfo("\n-------- robot robot.Init -------")
  MuninnPlugin.LogInfo("useRandomOrderToAnserQuestionRate: " .. tostring(RobotConfig.UseRandomOrderToAnserQuestionRate))
  MuninnPlugin.LogInfo("useRandomOrderToAnserQuestion: " .. tostring(useRandomOrderToAnserQuestion))
  MuninnPlugin.LogInfo("IsRobotRoom: " .. tostring(robot.IsRobotRoom))
  MuninnPlugin.LogInfo("------robot robot.Init end------\n")

end

function robot.SetRobotInfo(player)
  robot.currentId = player.uniqueId
  robot.displayName = player.displayName
  MuninnPlugin.LogInfo("robot info")
  MuninnPlugin.LogInfo(json.encode(player))
end

-- 提交答案的消息体
function SubmitAnswerMessage(index)
  local serverMessage =  {
    personaID = robot.currentId,
    stages = {},
    type = "SubmitAnswer",
    answerExpressions = 
    {{Left= 3,
    Right=0,
    Operator = { name = 1},Result = 3.0},
    {Left= 3,
    Right=0,
    Operator = { name = 1},Result = 3.0},
    {Left= 3,
    Right=0,
    Operator = { name = 1},Result = 24.0}},
    answerIndex = index - 1
  }
  return serverMessage
end

function SyncStatusToAll()
  local message = {
    type = Server.ClientMessageType.SyncStatus,
    currentIndex = currentIndex - 1,
    personaID = robot.currentId,
  }
  MuninnPlugin.BroadcastMessage(json.encode(message))
end

-- 启动机器人
function robot.StartRobot()
  GenRankDataMap()
  MuninnPlugin.ScheduleOnce("TryAnswerQuestion", 6000)
end

-- 尝试完成题目
function TryAnswerQuestion()
  local battleData = Server.GetAllBattleData()

  -- 游戏已经结束
  if not gameing then
    MuninnPlugin.LogInfo("\n------robot TryAnswerQuestion-----")
    MuninnPlugin.LogInfo('game over!!!')
    MuninnPlugin.LogInfo("------robot TryAnswerQuestion end-----\n")
    return
  end

  -- 选择题目
  local selectedQuestionIndex
  if useRandomOrderToAnserQuestion then
    selectedQuestionIndex = GetRandomUnresolvedQuestionIndex(battleData.resolvedStatus)
  else
    selectedQuestionIndex = GetNextUnresolvedQuestionIndex(battleData.resolvedStatus, currentIndex)
  end
  currentIndex = selectedQuestionIndex
  currentQuestion = Server.CurrentQuestions[selectedQuestionIndex].question.cards
  -- 同步选择题目的状态
  SyncStatusToAll()

  -- 解题
  AnswerQuestion()
end

-- 随机选择一道未完成的题目
function GetRandomUnresolvedQuestionIndex(data)
  local false_indices = {}

  -- 遍历数据列表，收集所有 "resolved" 为 false 的索引
  for index, item in ipairs(data) do
      if not item.resolved then
          table.insert(false_indices, index)
      end
  end

  -- 如果没有找到 "resolved" 为 false 的项
  if #false_indices == 0 then
      return nil
  end

  -- 随机选择一个索引
  local random_index = false_indices[math.random(1, #false_indices)]
  return random_index
end

-- 找到编号最小的一道未完成的题目
function GetNextUnresolvedQuestionIndex(items, currentIndex)
  local totalCount = #items
  local startIndex = (currentIndex + 1) % totalCount  -- 从当前索引的下一个开始

  for i = startIndex, startIndex + totalCount - 1 do
      local index = (i - 1) % totalCount  -- 循环索引
      if items[index + 1].resolved == false then
          return index + 1  -- 返回 Lua 中的 1 基索引
      end
  end

  return currentIndex  -- 如果没有找到，返回当前索引
end

function GetQuestionKey(data)
  -- 排序
  table.sort(data, function(a, b)
    return a.number < b.number
  end)

  -- 提取排序后的 number
  local numbers = {}
  for _, item in ipairs(data) do
      table.insert(numbers, item.number)
  end

  local question_key = table.concat(numbers, "-")
  return question_key
end

-- 解答题目
function AnswerQuestion()
  MuninnPlugin.LogInfo("\n--------- robot AnswerQuestion " .. tostring(currentIndex) .. "----------")
  local key = GetQuestionKey(currentQuestion)

  -- 获取平均耗时
  local currentRankData = RankDataMap[key]
  local Average = math.ceil(tonumber(currentRankData.Average))
  
  -- 计算随机时间
  local minTime = Average + RobotConfig.MinRandomTime
  local maxTime = Average + RobotConfig.MaxRandomTime

  minTime, maxTime = Utils.ClipInterval(minTime, maxTime, RobotConfig.LowerTimeBound, RobotConfig.UpperTimeBound)

  MuninnPlugin.LogInfo("minTime" .. tostring(minTime) .. " maxTime" .. tostring(maxTime))
  local tmpTime = minTime + (maxTime - minTime) * math.random()
  MuninnPlugin.LogInfo("Random time for robot is" .. tostring(tmpTime))
  local randomTime = RobotAbility.GetTime(tmpTime)

  -- 计算解决率
  local tmpRate = tonumber(currentRankData.ResolutionRate:sub(1, -2)) -- 去掉最后的百分号并转换为数字
  currentResolutionRate = RobotAbility.GetResolutionRate(tmpRate)  
  -- log
  MuninnPlugin.LogInfo("time: "..tostring(tmpTime) .." time for my ability: " .. tostring(randomTime))
  MuninnPlugin.LogInfo("resolve rate: " .. tostring(tmpRate) .. " rate for my ability: " .. tostring(currentResolutionRate))

  MuninnPlugin.LogInfo("sleep time: " .. tostring(randomTime))
  MuninnPlugin.LogInfo("--------- robot AnswerQuestion sleep----------\n")
  if currentTaskId ~= nil then
    MuninnPlugin.CancelTask(currentTaskId)
  end
  currentTaskId, err = MuninnPlugin.ScheduleOnce("TrySubmitAnswer", randomTime * 1000)
end

-- 尝试提交答案，并开始下一题
function TrySubmitAnswer()
  if not gameing then
    return
  end
  MuninnPlugin.LogInfo("\n----------- robot TrySubmitAnswer -----------")
  currentTaskId = nil
  -- 根据自身解决率决定是否提交答案
  if Utils.RandomPass(currentResolutionRate) then
    -- 提交答案
    MuninnPlugin.LogInfo('submit answer')
    local serverMessage = SubmitAnswerMessage(currentIndex)
    HandleSubmitAnswer(robot.currentId, serverMessage)
  end
  MuninnPlugin.LogInfo("----------- robot TrySubmitAnswer end-----------\n")

  -- 回答新的题目
  MuninnPlugin.ScheduleOnce("TryAnswerQuestion", 1000)
end

-- 通知最新进展
function robot.NotifyProgress(progress)
  if progress.currentResolvedQuestionIndex == currentIndex - 1 then
    -- 对手已完成当前正在处理的题目
    if currentTaskId ~= nil then
      MuninnPlugin.CancelTask(currentTaskId)
    end
    MuninnPlugin.ScheduleOnce("TryAnswerQuestion", 3000)
  end
end

return robot

