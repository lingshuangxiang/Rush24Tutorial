using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inbox;
using Unity.Passport.Runtime;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;


public static class InBoxHelper
{
    public static List<InboxMessage> InBoxMessages = new List<InboxMessage>();
    public static Action<bool> OnViewInbox;
    public static bool NewMessageFound;
    public static async Task ReceiveMessages()
    {
        await PassportFeatureSDK.Inbox.ReceiveMessages();
    }
    // 查看收件箱
    // start: 搜索的起始位置，默认为0
    // count: 搜索的目标数量，默认为20，最大为50
    public static async Task<ViewInboxResponse> ViewInbox(uint start = 0, uint count = 20)
    {
        InBoxMessages?.Clear();
        var viewInboxResponse = await PassportFeatureSDK.Inbox.ViewInbox(start, count);
        InBoxMessages = viewInboxResponse.Messages.ToList();
        bool newMessageFound = false;
        foreach (var message in InBoxMessages)
        {
            if (message.Status == MessageStatusType.Unread)
            {
                newMessageFound = true;
                break;
            }
        }
        NewMessageFound = newMessageFound;
        OnViewInbox?.Invoke(newMessageFound);
        return viewInboxResponse;
    }
 
    // 阅读邮件
    // messageId: 邮件ID
    public static async Task<ReadMessageResponse> ReadMessage(string messageId)
    {
        var readMessageResponse = await PassportFeatureSDK.Inbox.ReadMessage(messageId);
        bool newMessageFound = false;
        foreach (var message in InBoxMessages)
        {
            if (message.Id == messageId)
            {
                message.Status = readMessageResponse.Message.Status;
            }
            if (message.Status == MessageStatusType.Unread)
            {
                newMessageFound = true;
            }
        }
        NewMessageFound = newMessageFound;
        OnViewInbox?.Invoke(newMessageFound);
        return readMessageResponse;
    }
 
    // 领取邮件
    // messageId: 邮件ID
    public static async Task<ConsumeMessageResponse> ConsumeMessage(string messageId, Action<Exception> failedAction = null)
    {
        try
        {
            var consumeMessageResponse = await PassportFeatureSDK.Inbox.ConsumeMessage(messageId);
            return consumeMessageResponse;
        }
        catch (Exception e)
        {
            failedAction?.Invoke(e);
            Logger.LogError($"{e.Message} {e.Data}");
            throw;
        }
        
    }
 
    // 删除邮件
    // messageId: 邮件ID
    public static async Task DeleteMessage(string messageId)
    {
        await PassportFeatureSDK.Inbox.DeleteMessage(messageId);
    }
 
    // 阅读全部邮件
    public static async Task<ReadMessagesResponse> ReadAllMessages()
    {
        var readMessagesResponse = await PassportFeatureSDK.Inbox.ReadAllMessages();
        return readMessagesResponse;
    }
 
    // 领取全部带有附件的邮件
    public static async Task<ConsumeMessagesResponse> ConsumeAllMessages(Action<Exception> failedAction = null)
    {
        try
        {
            var consumeMessagesResponse = await PassportFeatureSDK.Inbox.ConsumeAllMessages();
            return consumeMessagesResponse;
        }
        catch (Exception e)
        {
            failedAction?.Invoke(e);
            Logger.LogError($"{e.Message} {e.Data}");
            throw;
        }
        
    }
 
    // 删除所有已完成的邮件
    public static async Task<DeleteMessagesResponse> DeleteAllMessages()
    {
        var deleteMessagesResponse = await PassportFeatureSDK.Inbox.DeleteAllMessages();
        return deleteMessagesResponse;
    }
}
