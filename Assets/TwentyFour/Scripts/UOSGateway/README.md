# 服务器部署指南

#### 1. 部署 func 云函数（JS）

##### 操作步骤

在 Assets/TwentyFour/Scripts/UOSGateway/FuncStateless~ 目录下，执行对应环境的命令
```js
// 测试环境
// node upload.js test
// 正式环境
// node upload.js prod
// node upload.js prod2
```

##### 配置说明
> 注意：func 线上环境氛围 prod 和 prod2，因为每次部署会导致旧版云函数失效，所以不要在线上使用的云函数环境上部署新版。（即：若线上使用 prod，则本次上线部署 prod2）

prod：https://uos.unity.cn/services/3e2310e5-e991-45e4-8890-1cf58af45eee/settings
prod2：https://uos.unity.cn/services/07543d80-f88a-40f8-a6d1-2af532c19d60/settings

##### func 中的环境配置包含

- 数据库id，密码，内网地址
  - 线上环境指向 Math24Dev UOS APP 的数据库
  - 测试环境指向 TestCRUD&Func UOS APP 的数据库。
- func 部署的目标 UOS APP
  - 线上环境 prod 指向 FuncStatelessJS UOS APP
  - 线上环境 prod2 指向 FuncStatelessJS-V2 UOS APP
  - 测试环境指向 TestCRUD&Func UOS APP

#### 2. 部署 Lua 脚本
##### 操作步骤
在 Assets/TwentyFour/Scripts/UOSGateway/Sync/Plugin~ 目录下，执行对应环境的命令
```lua
-- 正式环境
-- lua Pack.lua prod
-- lua Pack.lua prod2
-- 测试环境
-- lua Pack.lua test
```
将生成的 .zip 包，上传到对应的 UOS APP 上。
正式环境上传到 Math24Dev UOS APP，测试环境上传到 TestTestTest UOS APP 上。

##### 配置说明
- 24点客户端逻辑所在的 UOS APP
  - 线上环境指向 Math24Dev UOS APP
  - 测试环境指向 TestCRUD&Func UOS APP
- 部署后的云函数的 URL
  - 线上环境 prod 填写 FuncStatelessJS UOS APP 部署后的云函数链接前缀
  - 线上环境 prod2 填写 FuncStatelessJS-V2 UOS APP 部署后的云函数链接前缀
  - 测试环境填写 TestCRUD&Func UOS APP 部署后的云函数链接前缀