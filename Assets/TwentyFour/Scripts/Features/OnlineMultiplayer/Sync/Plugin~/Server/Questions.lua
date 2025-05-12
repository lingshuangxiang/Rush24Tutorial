local Questions = {}
local MuninnPlugin = require("MuninnPlugin")
local Utils = require("Utils.utils")
local json = require("Utils.json")

-- read battle data file
local AllBattleData = require("Data.AllBattleData")
local BaseBattleData = require("Data.BaseBattleData")

-- class card
Questions.card = {}
function Questions.card.new(number, index, suit)
  local self = {
    number = number,
    index = index,
    suit = suit,
  }
  return self
end

-- class question
Questions.question = {}
function Questions.question.new(question)
  local self = {
    cards = {}
  }
  for j = 1, #question do
    local card = Questions.card.new(question[j], j - 1, 0)
    table.insert(self.cards, card)
  end
  return self
end

Questions.stage = {}
function Questions.stage.new(question, index)
  local cards = Questions.question.new(question);
  local self = {
    index = index,
    question = cards
  }
  return self
end

-- 生成对战卡片组
function Questions.GenQuestions(n, useAdvanceQuestions)
  -- 选择题库
  local currentBattleData = BaseBattleData
  if useAdvanceQuestions then 
    currentBattleData = AllBattleData
  end
  MuninnPlugin.LogInfo("useAdvanceQuestions: " .. tostring(useAdvanceQuestions))

  local questions = Utils.randomPick(currentBattleData, n)
  local result = {}
  for i = 1, n do
    local currentQuestion = Utils.Shuffle(questions[i])
    local stage = Questions.stage.new(currentQuestion, i - 1)
    table.insert(result, stage)
  end
  return result;
end


return Questions;