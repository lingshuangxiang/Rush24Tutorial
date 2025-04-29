-- lua Test/TournamentTest.lua
local json = require("Utils.json")
local MuninnPlugin = require("MuninnPlugin")
local Endpoints = require("UOSGateway.Endpoints")
local Tournament = require("Server.Tournament")
local Test = {}

function Test.test()
  Tournament.GetTournamentScore("UOSTournamentSlug", "20251000006001")
end

local result = Test.test()

return Test;
