const battleData = require('./data.json');

function generateUUID() {
  // 生成随机数的函数
  function randomHex() {
      return Math.floor((1 + Math.random()) * 0x10000)
          .toString(16)
          .substring(1);
  }

  // 生成 UUID v4
  return (
      randomHex() + randomHex() + '-' +
      randomHex() + '-' +
      randomHex() + '-' +
      randomHex() + '-' +
      randomHex() + randomHex() + randomHex()
  );
}

function getBattleData() {
  battleData.roomID = generateUUID()
  return battleData;
}

module.exports = getBattleData;