-- lua Test/UnitTest.lua
-- 单元测试
local lu = require("Test.luaunit")
local server = require("Server.server")

server.GenQuestions(5, 0)

function testGetScore()
  -- 平局
  local red, blue = server.GetTeamScore({score = 0}, {score = 0})
  lu.assertEquals(red, 0)
  lu.assertEquals(blue, 0)

  local red, blue = server.GetTeamScore({score = 1}, {score = 1})
  lu.assertEquals(red, 0)
  lu.assertEquals(blue, 0)

  local red, blue = server.GetTeamScore({score = 2}, {score = 2})
  lu.assertEquals(red, 0)
  lu.assertEquals(blue, 0)

  -- red 输
  local red, blue = server.GetTeamScore({score = 0}, {score = 1})
  lu.assertEquals(red, -1)
  lu.assertEquals(blue, 1)

  local red, blue = server.GetTeamScore({score = 0}, {score = 2})
  lu.assertEquals(red, -1)
  lu.assertEquals(blue, 1)

  local red, blue = server.GetTeamScore({score = 0}, {score = 3})
  lu.assertEquals(red, -1)
  lu.assertEquals(blue, 1)

  local red, blue = server.GetTeamScore({score = 0}, {score = 4})
  lu.assertEquals(red, -1)
  lu.assertEquals(blue, 1)


  local red, blue = server.GetTeamScore({score = 1}, {score = 2})
  lu.assertEquals(red, -1)
  lu.assertEquals(blue, 1)

  -- blue 输
  local red, blue = server.GetTeamScore({score = 1}, {score = 0})
  lu.assertEquals(red, 1)
  lu.assertEquals(blue, -1)

  local red, blue = server.GetTeamScore({score = 4}, {score = 0})
  lu.assertEquals(red, 1)
  lu.assertEquals(blue, -1)

  local red, blue = server.GetTeamScore({score = 3}, {score = 2})
  lu.assertEquals(red, 1)
  lu.assertEquals(blue, -1)

  -- 完胜
  local red, blue = server.GetTeamScore({score = 0}, {score = 5})
  lu.assertEquals(red, -2)
  lu.assertEquals(blue, 2)

  local red, blue = server.GetTeamScore({score = 5}, {score = 0})
  lu.assertEquals(red, 2)
  lu.assertEquals(blue, -2)
end

function TestTournament()
  server.SetBattleMode(4)
  -- 完胜
  local red, blue = server.GetTeamScore({score = 0}, {score = 5})
  lu.assertEquals(red, -1)
  lu.assertEquals(blue, 2)

  local red, blue = server.GetTeamScore({score = 5}, {score = 0})
  lu.assertEquals(red, 2)
  lu.assertEquals(blue, -1)

  -- 平局
  local red, blue = server.GetTeamScore({score = 0}, {score = 0})
  lu.assertEquals(red, 0)
  lu.assertEquals(blue, 0)

  -- red 输
  local red, blue = server.GetTeamScore({score = 0}, {score = 1})
  lu.assertEquals(red, 0)
  lu.assertEquals(blue, 1)

  -- blue 输
  local red, blue = server.GetTeamScore({score = 1}, {score = 0})
  lu.assertEquals(red, 1)
  lu.assertEquals(blue, 0)
end

-- testGetScore()
TestTournament()