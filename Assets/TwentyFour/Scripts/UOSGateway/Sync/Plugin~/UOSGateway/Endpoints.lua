local UOSConfig = require("UOSGateway.UOSConfig")
local Endpoints = {
  Passport = UOSConfig.FuncUrl .."/passport",
  CRUD = UOSConfig.FuncUrl .. "/crud",
  Tournament = UOSConfig.FuncUrl .. "/tournament",
}

return Endpoints;
