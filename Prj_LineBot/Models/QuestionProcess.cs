using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prj_LineBot.Models
{
    public class QuestionProcess
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int M_Order { get; set; }

        public bool QuestionStatus { get; set; }

        public string Answer { get; set; }

        public int ErrorCount { get; set; }
    }
}