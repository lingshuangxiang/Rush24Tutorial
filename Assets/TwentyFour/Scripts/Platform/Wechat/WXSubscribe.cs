using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Passport.Runtime.UI;
using UnityEngine;
using UnityEngine.Networking;
using Logger = Unity.UOS.TwentyFour.Common.Logger;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif

public static class WXSubscribe
{
    public static string OpenID;
    public static bool Subscribed;
    private static string currentEnvVersion;
    const string env_developer = "developer";
    const string env_trial = "trial";
    const string env_formal = "formal";
    private static string url = $"{UosAppConfigs.GetBaseStatelessUrl()}subscribe";

    public static void Init(string openId)
    {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        OpenID = openId;
        currentEnvVersion = WX.GetAccountInfoSync().miniProgram.envVersion;
#endif
    }
    public static IEnumerator SendPostRequest(string templateIdValue,string startTime,
        string endTime,string sendTime,string tournamentSlug,Action OnSucceed = null)
    {
        
        // 创建要发送的数据
        string dataBody = JsonConvert.SerializeObject(
            new
            {
                thing1 = (new { value = "锦标赛开赛提醒" }),
                time8 = (new { value = startTime }),
                time9 = (new { value = endTime }),
            });
        Debug.Log(dataBody);
        var env = GetSubscribeEnv();
        string jsonData = JsonConvert.SerializeObject(
            new
            {
                templateId = templateIdValue,
                toUser = OpenID,
                miniProgramState = env,
                lang = "zh_CN",
                notifyTime = sendTime,
                data = dataBody,
                slugName = tournamentSlug,
            });

        Debug.Log(jsonData);
        // 创建 UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // 设置请求体
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 设置请求头
            request.SetRequestHeader("Content-Type", "application/json");

            // 发送请求并等待响应
            yield return request.SendWebRequest();

            // 检查请求是否出错
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                OnSucceed?.Invoke();
                Debug.Log("Response: " + request.downloadHandler.text);
            }
        }
    }

    public static IEnumerator SendGETRequest(string tournamentSlug,Action OnSucceed = null)
    {
        
        // string jsonData = JsonConvert.SerializeObject(
        //     new
        //     {
        //         toUser = OpenID,
        //         slugName = tournamentSlug,
        //     });
        //
        // Debug.Log(jsonData);
        var geturl = $"{url}?toUser={OpenID}&slugName={tournamentSlug}";
        using (UnityWebRequest request = UnityWebRequest.Get(geturl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
                Subscribed = false;
            }
            else
            {
                //Debug.LogError("Response: " + request.downloadHandler.text);
                var data = JsonUtility.FromJson<GetSubscribeData>(request.downloadHandler.text);
                //Debug.LogError(data.count);
                Subscribed = data.count > 0;
            }
        }
    }
    public static void Subscribe(string tempID, Action OnSucceed = null)
    {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        var option = new RequestSubscribeMessageOption();
        option.tmplIds = new string[] { tempID };
        option.success = (res) =>
        {
            UIMessage.Show("订阅成功");
            OnSucceed?.Invoke();
            Debug.Log("订阅成功");
        };
        option.fail = (res) =>
        {
            UIMessage.Show($"订阅失败 {tempID} msg:{res.errMsg} code:{res.errCode}");
            Debug.Log($"订阅失败 {tempID} msg:{res.errMsg} code:{res.errCode}");
        };
        WX.RequestSubscribeMessage(option);
#endif
    }

    public static void HasPermission(string templateId, Action suc = null, Action<string> fail = null)
    {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WeChatWASM.GetSettingOption option = new WeChatWASM.GetSettingOption();
        option.withSubscriptions = true;
        option.success += (WeChatWASM.GetSettingSuccessCallbackResult r) =>
        {
            WeChatWASM.SubscriptionsSetting set = r.subscriptionsSetting;
            Dictionary<string, string> dict = set.itemSettings;
            foreach (var item in dict)
            {
                Debug.LogError(item.Key + ":" + item.Value);
            }

            if (r.subscriptionsSetting != null && r.subscriptionsSetting.mainSwitch && dict.ContainsKey(templateId) &&
                dict[templateId].Equals("accept"))
            {
                Debug.Log($"{templateId} Has permission");
                suc?.Invoke();
            }
            else
            {
                Debug.LogError($"{templateId} No permission");

                fail?.Invoke("No permission");
            }
        };
        option.fail += (error) => { fail.Invoke(error.errMsg); };
        WeChatWASM.WX.GetSetting(option);
#endif
    }

    static string GetSubscribeEnv()
    {
        Logger.Log($"wx env:{currentEnvVersion}");
        switch (currentEnvVersion)
        {
            case "develop":
                return env_developer;
            case "trial":
                return env_trial;
            case "release":
                return env_formal;
            default:
                return env_formal;
        }
        
    }

    public static void Dispose()
    {
        OpenID = string.Empty;
        Subscribed = false;
    }
}
[Serializable]
public class GetSubscribeData
{
    public int count;
}