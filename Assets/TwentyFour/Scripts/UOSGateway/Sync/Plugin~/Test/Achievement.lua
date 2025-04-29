-- lua Test/Achievement.lua
local Achievement = require("../Server/Achievement")

local personaId = "1000049003"
local score = 4

Achievement.UpdateAchievements(personaId, score)