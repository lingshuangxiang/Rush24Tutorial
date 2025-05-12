-- 正式环境
-- lua Pack.lua prod
-- lua Pack.lua prod2
-- 测试环境
-- lua Pack.lua test
local json = require("Utils.json")

-- step0: 环境配置
local env = {
    test = {
        -- https://uos.unity.cn/services/910a804f-e607-4c8f-a627-aa44c6bc83be
        AppID = "910a804f-e607-4c8f-a627-aa44c6bc83be",
        AppServiceSecret = "46f4544810b5492e919b27079e27a134",
        FuncUrl = "https://stateless.unity.cn/release/7fa14adf-6df8-4672-8a48-a16118ae5887",
        -- FuncUrl = "https://stateless.unity.cn/release/07543d80-f88a-40f8-a6d1-2af532c19d60"
    },
    prod = {
        -- https://uos.unity.cn/services/cbe05e3b-6d6f-4e08-b0d6-80ba62cd790d
        AppID = "cbe05e3b-6d6f-4e08-b0d6-80ba62cd790d",
        AppServiceSecret = "91cf11bce9ae400281c27452b7cf61fb",
        -- func 云函数版本1
        FuncUrl = "https://stateless.unity.cn/release/12a1a357-52df-4a73-a888-9cf28ed75017",
    },
    prod2 = {
        AppID = "cbe05e3b-6d6f-4e08-b0d6-80ba62cd790d",
        AppServiceSecret = "91cf11bce9ae400281c27452b7cf61fb",
        -- func 云函数版本2
        FuncUrl = "https://stateless.unity.cn/release/07543d80-f88a-40f8-a6d1-2af532c19d60",
    }
}

local config;
-- step1: 生成 UOSConfig.lua 文件
if #arg < 1 then
    config = env.test
else
    -- 从命令行参数中获取 param1 的值
    local envName = arg[1]
    config = env[envName]
end

print(json.encode(config))


local function SetConfigToFile(uosConfig)
-- 打开文件以写入模式，如果文件不存在则创建
    local file, err = io.open("UOSGateway/UOSConfig.lua", "w")
    if not file then
        print("无法打开文件: " .. err)
        return
    end

    local fileContent = [[
local Config = {}
Config.AppID = "]] .. uosConfig.AppID .. [["
Config.AppServiceSecret = "]] .. uosConfig.AppServiceSecret .. [["
Config.FuncUrl = "]] ..uosConfig.FuncUrl .. [["
return Config;
]]

    -- 写入文件内容
    file:write(fileContent)

    -- 关闭文件
    file:close()
    print("文件生成成功，文件名为: config.lua")
end

SetConfigToFile(config)

-- step2: 自动打包为 zip
-- 获取当前时间并格式化，用于生成文件名
local current_time = os.date("%Y%m%d%H%M%S")
local zip_name = "main" .. current_time .. ".zip"

-- 获取当前工作目录
local current_dir = io.popen("pwd"):read("*l")

-- 定义要排除的文件或文件夹列表
local exclude_list = {
    "*.meta",  -- 排除所有 .log 文件
    "MuninnPlugin",  -- 排除名为 temp_folder 的文件夹
    ".DS_Store",
    "*.zip"
}

-- 构建排除参数
local exclude_param = ""
for _, pattern in ipairs(exclude_list) do
    exclude_param = exclude_param .. " -x " .. pattern
end


-- 构建 zip 命令，-r 表示递归地将目录及其子目录下的所有文件添加到压缩包中
local command = string.format("zip -r %s %s %s", zip_name, " . ", exclude_param)

print(current_dir)
print(zip_name)
print(exclude_param)

-- 执行命令
local result = os.execute(command)
print("打包成功，生成的 ZIP 文件名为: " .. zip_name)

-- step3: 将文件改回 test 环境
SetConfigToFile(env.test)

-- TODO:
-- 自动上传脚本