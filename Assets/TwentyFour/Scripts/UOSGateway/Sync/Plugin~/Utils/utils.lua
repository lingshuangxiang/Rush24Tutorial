local utils = {}

-- 读取文件内容的函数
function utils.read_file(file_path)
  local file = io.open(file_path, "r")  -- 以只读模式打开文件
  if not file then
      error("Could not open file: " .. file_path)
  end

  local content = file:read("*a")  -- 读取文件的所有内容
  file:close()  -- 关闭文件

  return content  -- 返回文件内容
end

-- 生成 n 位随机数字的函数
function utils.generate_random_number(n)
  if n <= 0 then
      return "Invalid input. n must be greater than 0."
  end

  -- 生成前导数字，确保不以0开头
  local first_digit = math.random(1, 9)
  local random_number = tostring(first_digit)  -- 将第一个数字转换为字符串

  -- 生成后续的 n-1 位随机数字
  for i = 2, n do
      local digit = math.random(0, 9)
      random_number = random_number .. digit  -- 连接后续数字
  end

  return random_number
end

-- utils：随机选择数组中的n项（不重复）
function utils.randomPick(array, n)
  local result = {}
  local len = #array
  for i = 1, n do
      local index = math.random(1, len)
      table.insert(result, array[index])
      -- 将最后一个元素移到被选中的位置，以避免重复选取
      array[index] = array[len]
      len = len - 1
      array[len + 1] = nil
  end
  return result
end

function utils.Shuffle(list)
  -- 创建一个新的列表
  local shuffledList = {}
  -- 获取原列表的长度
  local n = #list
  
  -- 将原列表的元素复制到新的列表
  for i = 1, n do
      shuffledList[i] = list[i]
  end

  -- 打乱新的列表
  for i = n, 2, -1 do
      local j = math.random(i)
      -- 交换元素
      shuffledList[i], shuffledList[j] = shuffledList[j], shuffledList[i]
  end
  
  return shuffledList
end

function utils.ClipInterval(a, b, minBound, maxBound)
  -- 确保 a 是区间的起始点
  if a > b then
      a, b = b, a
  end

  -- 检查区间是否在 bounds 内
  if a >= minBound and b <= maxBound then
      return a, b  -- 返回原区间
  elseif a > maxBound or b < minBound then
      -- 区间完全不在 bounds 内
      return minBound, maxBound  -- 返回 bounds
  else
      -- 裁剪区间
      local clippedA = math.max(a, minBound)
      local clippedB = math.min(b, maxBound)
      return clippedA, clippedB
  end
end

function utils.RandomPass(rate)
  local number = math.random(1, 99)
  return number < rate
end

GlobalXValueForCalculateExpression = 0;

-- 定义计算表达式的函数
function utils.CalculateExpression(expression, x)
  -- 使用 load 函数将表达式编译成一个函数
  local newExpression = string.gsub(expression, "x", "GlobalXValueForCalculateExpression")

  -- 兼容新版本 lua
  if loadstring == nil then 
    return x;
  end

  local func, err = loadstring("return " .. newExpression)
  
  if not func then
      print("错误: " .. err)
      return nil
  end
  GlobalXValueForCalculateExpression = x
  -- 设置 x 的值并计算结果
  local result = func()
  return result
end

function utils.GetFormattedTime()
  local utc_time = os.date("!*t")

  -- 格式化为可读字符串
  local formatted_utc_time = string.format(
      "%04d-%02d-%02dT%02d:%02d:%02dZ",
      utc_time.year,
      utc_time.month,
      utc_time.day,
      utc_time.hour,
      utc_time.min,
      utc_time.sec
  )
  return formatted_utc_time
end


return utils


