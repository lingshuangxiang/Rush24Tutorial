using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.UOS.TwentyFour.Editor
{
    public static class WXUploadHelper
    {
      private static string JSString = @"const ci = require('miniprogram-ci')
;(async () => {
  const project = new ci.Project({
    appid: 'APPID',
    type: 'miniGame',
    projectPath: 'PROJECT_PATH',
    privateKeyPath: 'KEY_PATH',
    ignores: ['node_modules/**/*'],
  })
  const uploadResult = await ci.upload({
    project,
    version: 'PROJECT_VERSION',
    desc: 'DESCRIPTION',
    setting: {
      es6: true,
    },
    robot:BOT_NUM,
    onProgressUpdate: console.log,
  })
  console.log(uploadResult)
  process.exit(0)
})()";
      public static string GenerateJSString(string project, string commitInfo)
      {
        var projectPath = project.Replace("\\", "/");
        //Debug.Log(projectPath);
        string jsPath = $"{Directory.GetCurrentDirectory()}\\Tools\\upload.js".Replace("\\", "/");
        string keyPath = $"{Directory.GetCurrentDirectory()}\\Tools\\wx_upload.key".Replace("\\", "/");
        string appId = "wx2c4ddbaa2bde570f";
        string version = PlayerSettings.bundleVersion;
        string desc = $"自动上传版本:{version} Commit:[{commitInfo} {DateTime.Now.ToLocalTime()}]";
        string bot = version.Split('.').ToList().Last();
        if (int.TryParse(bot, out int botInt))
        {
          bot = $"{(botInt % 2) +1}";
        }
        else
        {
          return string.Empty;
        }
        var result = JSString.Replace("APPID", appId).
          Replace("PROJECT_PATH", projectPath).
          Replace("KEY_PATH", keyPath).
          Replace("PROJECT_VERSION", version).
          Replace("DESCRIPTION", desc).
          Replace("BOT_NUM", bot);
        File.WriteAllText(jsPath, result);
        Debug.Log($"已生成JS文件:{jsPath}");
        return jsPath;
      }

      
    }
}