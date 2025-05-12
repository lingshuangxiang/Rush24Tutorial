local Utils = require("Utils.utils")
local MuninnPlugin = require("MuninnPlugin")

local RobotAbility = {}

local AbilityCoefficient = nil
local TimeFormula = nil
local ResolutionRateFormula = nil

function RobotAbility.Init(config)
  AbilityCoefficient = config.AbilityCoefficient or 0.7
  TimeFormula = config.TimeFormula or "-3*x+3.2"
  ResolutionRateFormula = config.ResolutionRateFormula or "3*x+0.1"
  MuninnPlugin.LogInfo("AbilityCoefficient " .. tostring(AbilityCoefficient) .. " TimeFormula:" .. tostring(TimeFormula) .. " ResolutionRateFormula:" .. tostring(ResolutionRateFormula))
end

function RobotAbility.GetTime(currentTime)
  return currentTime * Utils.CalculateExpression(TimeFormula, AbilityCoefficient)
end

function RobotAbility.GetResolutionRate(currentRate)
  return currentRate * Utils.CalculateExpression(ResolutionRateFormula, AbilityCoefficient)
end

return RobotAbility