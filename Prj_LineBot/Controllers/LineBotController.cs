using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Configuration;
using isRock.LineBot;
using Line.Messaging;
using Line.Messaging.Webhooks;
using System.IO;
using Newtonsoft.Json;
using isRock.LineBot.Conversation;

namespace Prj_LineBot.Controllers
{
    public class LineBotController : LineWebHookControllerBase
    {
        BotService service = new BotService();
        UploadImageService imageService = new UploadImageService();
        [HttpPost]
        public IHttpActionResult webhook()
        {

            string ChannelAccessToken = "pm8zf310EYYHjvJEkM347wenUZDiP8IBf/lMfqeoBIroaBQkcyNcg2Me+dQR1lmVDRBPatCB0MjIanxkFa8taRPYX9g+GcmeEuWfz2hf8Gx4SZcxRiYMLcjFVmpw3GhkejYKTkJX/uqjQoDndGNo9wdB04t89/1O/w1cDnyilFU=";
            try
            {
                this.ChannelAccessToken = ChannelAccessToken;
                string Message = "";
                var item = this.ReceivedMessage.events.FirstOrDefault();
                isRock.LineBot.Event LineEvent = null;

                LineUserInfo UserInfo = null;
                if (item.source.type.ToLower() == "room")
                    UserInfo = Utility.GetRoomMemberProfile(
                        item.source.roomId, item.source.userId, ChannelAccessToken);
                if (item.source.type.ToLower() == "group")
                    UserInfo = Utility.GetGroupMemberProfile(
                        item.source.groupId, item.source.userId, ChannelAccessToken);
                if (item.source.type.ToLower() == "user")
                    UserInfo = Utility.GetUserInfo(item.source.userId, ChannelAccessToken);

                if (item != null)
                {
                    switch (item.type)
                    {
                        case "join":
                            //Message = $"有人把我加入{item.source.type}中了，大家好啊~";
                            Message = $"初次見面多多指教，大家好ㄛ~";
                            //回覆用戶
                            Utility.ReplyMessage(ReceivedMessage.events[0].replyToken, Message, ChannelAccessToken);
                            break;

                        case "message":
                            int question_id = service.HaveQuestion(item.source.userId);
                            if (question_id != 0)
                            {
                                MessageAction[] actions = null;
                                var msgArray = service.NextQuestion(item.source.userId, ReceivedMessage.events[0].message.text).Split(',');
                                isRock.LineBot.ButtonsTemplate ButtonTemplate = null;
                                switch (msgArray[1])
                                {
                                    case "1":
                                        actions = new MessageAction[]
                                        {
                                            new MessageAction { label = "威秀影城", text = "威秀" },
                                            new MessageAction { label = "國賓影城", text = "國賓" }
                                        };
                                        ButtonTemplate = GetButtonsTemplate(msgArray[0], msgArray[0], $"請選擇要查詢之戲院", actions);
                                        Utility.ReplyTemplateMessage(item.replyToken, ButtonTemplate, ChannelAccessToken);
                                        break;
                                    case "2":
                                        if (msgArray[2] == "VS")
                                        {
                                            actions = new MessageAction[]
                                            {
                                                new MessageAction {label = "京站威秀", text = "京站"},
                                                new MessageAction {label = "信義威秀", text = "信義"},
                                                new MessageAction {label = "日新威秀", text = "日新"},
                                                new MessageAction {label = "板橋大遠百威秀", text = "板橋大遠百"}

                                            };
                                            ButtonTemplate = GetButtonsTemplate(msgArray[0], "請點選下方戲院", msgArray[0], actions);
                                            Utility.ReplyTemplateMessage(item.replyToken, ButtonTemplate, ChannelAccessToken);
                                        }
                                        else if (msgArray[2] == "AT")
                                        {
                                            actions = new MessageAction[]
                                           {
                                                new MessageAction {label = "西門國賓", text = "西門國賓"},
                                                new MessageAction {label = "威風國賓", text = "威風國賓"},
                                                new MessageAction {label = "長春國賓", text = "長春國賓"},
                                                new MessageAction {label = "中和環球國賓", text = "中和環球國賓"}
                                           };
                                            ButtonTemplate = GetButtonsTemplate(msgArray[0], "請點選下方戲院地點(林口國賓、新莊國賓請直接輸入)", msgArray[0], actions);
                                            Utility.ReplyTemplateMessage(item.replyToken, ButtonTemplate, ChannelAccessToken);
                                        }
                                        break;
                                    default:
                                        Utility.ReplyMessage(item.replyToken, msgArray[0], ChannelAccessToken);
                                        break;
                                }

                            }

                            if (item.message.type.ToLower() == "image")
                            {
                               
                                if (imageService.isUploading(item.source.userId))
                                {
                                    var byteArray = Utility.GetUserUploadedContent(item.message.id, ChannelAccessToken);
                                    var link = imageService.UploadImage(byteArray, item.source.userId, UserInfo.displayName);
                                    isRock.LineBot.TextMessage textMessage = new isRock.LineBot.TextMessage("---Completed!---");
                                    isRock.LineBot.ImageMessage imageMessage = new isRock.LineBot.ImageMessage(new Uri(link), new Uri(link));
                                    var Messages = new List<MessageBase>();
                                    Messages.Add(textMessage);
                                    Messages.Add(imageMessage);
                                    this.ReplyMessage(item.replyToken, Messages);

                                }
                            }
                            else if(item.message.type.ToLower() == "text")
                            {
                                switch (ReceivedMessage.events[0].message.text)
                                {
                                    case "--exit":
                                        Utility.ReplyMessage(item.replyToken, "bye-bye", ChannelAccessToken);

                                        //離開
                                        if (item.source.type.ToLower() == "room")
                                            Utility.LeaveRoom(item.source.roomId, ChannelAccessToken);
                                        else if (item.source.type.ToLower() == "group")
                                            Utility.LeaveGroup(item.source.groupId, ChannelAccessToken);

                                        break;
                                    case "--upload":
                                        var msg = imageService.addUploadStatus(item.source.userId);
                                        if (!string.IsNullOrEmpty(msg))
                                        {
                                            Message = msg;
                                            if(UserInfo != null)
                                            {
                                              
                                               Message += $"\r\n -Target user: \"{UserInfo.displayName}\"";
                                            }
                                            this.ReplyMessage(item.replyToken, Message);
                                        }
                                        else
                                        {
                                            Message = "Plz try again. :(";
                                        }
                                        break;

                                    case string str when str.Contains("抽老公") || str.Contains("抽帥哥"):
                                        string _url = service.GetRandomGuy();
                                        if (!string.IsNullOrEmpty(_url))
                                        {
                                            isRock.LineBot.ImageMessage imageMessage = new isRock.LineBot.ImageMessage(new Uri(_url), new Uri(_url));
                                            var Messages = new List<MessageBase>();
                                            Messages.Add(imageMessage);
                                            this.ReplyMessage(item.replyToken, Messages);
                                        }
                                        else
                                        {
                                            this.ReplyMessage(item.replyToken, "你沒帥哥或是正在維修中..請洽詢我老闆 :D");
                                        }
                                        break;
                                    case "-help":
                                        Message = "抽帥哥圖片（指令：抽帥哥/抽老公）\r\n========\r\n抽動物圖片（指令：抽動物）\r\n========\r\n查詢地方當前溫度（指令：xx氣溫 / xx溫度）\r\n========\r\n查詢星座今日運勢 （指令：xx座運勢）\r\n========\r\n" +
                                            "查詢電影（指令：查詢電影）如果要中斷請輸入\"取消\"\r\n========\r\n近期電影列表（指令：電影列表）";
                                        break;
                                    case string str when str.Contains("座運勢"):
                                        Message = service.GetHoroscope(str.Substring(0, 3));
                                        break;
                                    case string str when str.Contains("電影列表"):
                                        Message = service.GetMovieList();
                                        break;
                                    case "Hi":
                                        Message = "Hello";
                                        break;
                                    case string str when str.Contains("抽動物"):
                                        string ani_url = "https://" + HttpContext.Current.Request.Url.Host + service.GetAnimalImg();
                                        Utility.ReplyImageMessage(item.replyToken, ani_url, ani_url, ChannelAccessToken);
                                        break;
                                    case "查詢電影":
                                        var question = service.AddQuestion(item.source.userId);
                                        if (question != "")
                                        {
                                            MessageAction[] actions =
        {
                                            new MessageAction {label = "威秀影城", text = "威秀"},
                                            new MessageAction {label = "國賓影城", text = "國賓"}
                                        };
                                            var ButtonTemplate = GetButtonsTemplate(question, question, $"{question}", actions);
                                            Utility.ReplyTemplateMessage(item.replyToken, ButtonTemplate, ChannelAccessToken);

                                        }
                                        else
                                            Utility.ReplyMessage(item.replyToken, "請先結束當前問題!", ChannelAccessToken);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            
                            //取得用戶名稱 
                            //LineUserInfo UserInfo = null;
                            //if (item.source.type.ToLower() == "room")
                            //    UserInfo = Utility.GetRoomMemberProfile(
                            //        item.source.roomId, item.source.userId, ChannelAccessToken);
                            //if (item.source.type.ToLower() == "group")
                            //    UserInfo = Utility.GetGroupMemberProfile(
                            //        item.source.groupId, item.source.userId, ChannelAccessToken);
                            //顯示用戶名稱
                            //if (item.source.type.ToLower() != "user")
                            //    Message += "\n你是:" + UserInfo.displayName;
                            //回覆用戶
                            if (Message != "")
                            {
                                Utility.ReplyMessage(item.replyToken, Message, ChannelAccessToken);
                            }
                            break;
                        default:
                            break;
                    }
                }



            }
            catch (Exception e)
            {

                return Ok(HttpContext.Current.Request.Url.Host);
            }

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult GetPath()
        {
            BotService service = new BotService();
            var str = "天秤座運勢";
            var msg = "";
            if (str.Contains("座"))
            {
                msg = service.GetHoroscope(str.Substring(str.IndexOf("座") - 2, str.IndexOf("座")));
            }

            return Ok(msg);
        }

        public isRock.LineBot.ButtonsTemplate GetButtonsTemplate(string altText, string text, string title, MessageAction[] _actions)
        {
            var actions = new List<TemplateActionBase>();
            actions.AddRange(_actions);

            //單一Button Template Message
            var ButtonTemplate = new isRock.LineBot.ButtonsTemplate()
            {
                altText = altText,
                text = text,
                title = title,
                //設定圖片
                thumbnailImageUrl = new Uri("https://orig00.deviantart.net/a350/f/2013/221/7/7/jarvis_rotator_by_yash1331-d6hcqa3.png"),
                actions = actions //設定回覆動作
            };

            return ButtonTemplate;
        }


    }
}
