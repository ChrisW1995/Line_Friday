using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Prj_LineBot.Models
{
    public class Draw
    {
        public int Id { get; set; }

        public string GroupId { get; set; }

        [StringLength(100)]
        public  string DrawTitle { get; set; }
    }
}