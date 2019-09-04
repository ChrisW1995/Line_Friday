using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prj_LineBot.Models
{
    public class UserQuestion
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string QuestionContent { get; set; }
    }
}