-- 游戏主流程测试
-- lua Test/Test.lua
local main = require("main") -- 必须引入

local function generateUUID()
  local random = math.random
  -- UUID 模板：xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx
  local template = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'
  local uuid = template:gsub('[xy]', function(c)
      if c == 'x' then
          -- 生成 0-15 的随机十六进制数
          return string.format('%x', random(0, 0xF))
      else
          -- y 需要是 8、9、a、b 中的一个（即二进制 10xx）
          return string.format('%x', random(8, 0xB)) -- 8 <= value <= 11
      end
  end)
  return uuid
end

-- 模拟开始游戏
OnCreateRoom(
  {
    UosAppId = "",    -- uos的appid
    Id = generateUUID(),          -- 房间id
    Name = "",        -- 房间名称
    OwnerId = "",     -- 开发者侧的userId，房主
    Namespace = "",   -- 命令空间
    Visibility = "",  -- 可见性
    JoinCode = "",    -- 邀请码
    MaxPlayers = 10,  -- 房间的最大用户数
    Properties = {
      ISROBOT = "false",
      ROBOTCONFIG = '{"MinRandomTime":-3,"MaxRandomTime":10,"LowerTimeBound":5,"UpperTimeBound":40,"ResolutionRateBound":40, "AbilityCoefficient": 0.7, "TimeFormula":"-3*x+3.2", "ResolutionRateFormula": "3*x", "UseRandomOrderToAnserQuestionRate": 50}',
      tournament_slug_name = "UOSTournamentSlug",
      TournamentLeaderboardSlug_Main = "UOSTournamentLeaderboardSlugMain",
      TournamentLeaderboardSlug_PerfectWin = "UOSTournamentLeaderboardSlugPerfectWin",
      TournamentLeaderboardSlug_FastSolveSingle = "UOSTournamentLeaderboardSlugFastSolveSingle",
      TournamentLeaderboardSlug_FastestAverageTime = "UOSTournamentLeaderboardSlugFastestAverageTime"
    },  -- 房间的自定义属性, 是一个table类型
}
)
local persona1 = "1000049003"
local persona2 = "1000032002"
OnMessage({ Data = [[
    {"type":"StartGame",
    "stages":[],
    "size":5,
    "useAdvanceQuestionsRate": 0,
    "battleMode": 4,
    "answerExpressions":[],"answerIndex":0,"judgeResult":false,
    "personaID": ]] .. persona1 .. [[,
    "resolvedStatus":[{"resolved":false,"resolvedPersonaID":11111, "resolvedTime": 0, "resolvedTeam": 0, "costTime": 0 },
    {"resolved":false,"resolvedPersonaID":11111, "resolvedTime": 0, "resolvedTeam": 0, "costTime": 0 },
    {"resolved":false,"resolvedPersonaID":11111, "resolvedTime": 0, "resolvedTeam": 0, "costTime": 0 },
    {"resolved":false,"resolvedPersonaID":11111, "resolvedTime": 0, "resolvedTeam": 0, "costTime": 0 },
    {"resolved":false,"resolvedPersonaID":11111, "resolvedTime": 0, "resolvedTeam": 0, "costTime": 0 }],
    "redTeamProgress":{
      "score":0,
      "resolved":[false,false,false,false,false],
      "teamPlayers":
      [{"uniqueId": ]] .. persona1 .. [[,"currentIndex":0, "displayName": "white"}]
    },
    "blueTeamProgress":{"score":0,"resolved":[false,false,false,false,false],
      "teamPlayers":[{
      "uniqueId": ]] .. persona2 .. [[,
      "currentIndex":0, "displayName": "momo" }]}
    }]]
})
-- -- 模拟断线重连
-- OnJoin({Id = "132456"})

RemainTime = 60
CountDown()

-- -- -- 模拟提交答案
OnMessage({
  Data = [[ {
  "personaID":  ]] .. persona1 .. [[,
  "stages":[],"type":
  "SubmitAnswer","answerExpressions":
  [{"Left":3,"Right":0,"Operator":{"name":1},"Result":3.0},
  {"Left":2,"Right":0,"Operator":{"name":3},"Result":3.0},
  {"Left":0,"Right":1,"Operator":{"name":2},"Result":24.0}],
  "answerIndex":0} ]]
})

RemainTime = 58
CountDown()

-- 模拟提交答案
OnMessage({
  Data = [[ {
  "personaID":  ]] .. persona1 .. [[,
  "stages":[],"type":
  "SubmitAnswer","answerExpressions":
  [{"Left":3,"Right":0,"Operator":{"name":1},"Result":3.0},
  {"Left":2,"Right":0,"Operator":{"name":3},"Result":3.0},
  {"Left":0,"Right":1,"Operator":{"name":2},"Result":24.0}],
  "answerIndex":1} ]]
})

RemainTime = 54
CountDown()

OnMessage({
  Data = [[ {
  "personaID":  ]] .. persona1 .. [[,
  "stages":[],"type":
  "SubmitAnswer","answerExpressions":
  [{"Left":3,"Right":0,"Operator":{"name":1},"Result":3.0},
  {"Left":2,"Right":0,"Operator":{"name":3},"Result":3.0},
  {"Left":0,"Right":1,"Operator":{"name":2},"Result":24.0}],
  "answerIndex":2} ]]
})

RemainTime = 48
CountDown()

OnMessage({
  Data = [[ {
  "personaID":  ]] .. persona1 .. [[,
  "stages":[],"type":
  "SubmitAnswer","answerExpressions":
  [{"Left":3,"Right":0,"Operator":{"name":1},"Result":3.0},
  {"Left":2,"Right":0,"Operator":{"name":3},"Result":3.0},
  {"Left":0,"Right":1,"Operator":{"name":2},"Result":24.0}],
  "answerIndex":3} ]]
})

-- RemainTime = 40
-- CountDown()

OnMessage({
  Data = [[ {
  "personaID":  ]] .. persona1 .. [[,
  "stages":[],"type":
  "SubmitAnswer","answerExpressions":
  [{"Left":3,"Right":0,"Operator":{"name":1},"Result":3.0},
  {"Left":2,"Right":0,"Operator":{"name":3},"Result":3.0},
  {"Left":0,"Right":1,"Operator":{"name":2},"Result":24.0}],
  "answerIndex":4} ]]
})

-- -- 模拟时间结束
-- RemainTime = 0
-- CountDown()