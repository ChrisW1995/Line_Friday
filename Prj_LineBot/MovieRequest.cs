using isRock.LineBot.Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prj_LineBot
{
    public class MovieRequest : ConversationEntity
    {
        [Question("請問您要請的假別是?")]
        [Order(1)]
        public string 假別 { get; set; }

        [Question("請問您的代理人是誰?")]
        [Order(2)]
        public string 代理人 { get; set; }

        [Question("請問您的請假日期是?")]
        [Order(3)]
        public DateTime 請假日期 { get; set; }

        
    }
}