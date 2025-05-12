local MuninnPlugin = require("MuninnPlugin")


local timer = {}
local timeGap = 50

local currentMilliseconds = 0
local taskId = nil

function SetTimer()
  currentMilliseconds = currentMilliseconds + timeGap;
end

-- 重置计时器
function timer.Reset()
  if taskId ~= nil then
    MuninnPlugin.CancelTask(taskId)
  end
  currentMilliseconds = 0
  taskId = MuninnPlugin.ScheduleRepeat("SetTimer", timeGap)
end

-- 获取当前毫秒数
function timer.GetCurrentSeconds()
  return currentMilliseconds / 1000;
end

return timer