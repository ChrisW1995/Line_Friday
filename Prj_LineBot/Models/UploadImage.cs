using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Prj_LineBot.Models
{
    public class UploadImage
    {
        public int Id { get; set; }

        public string imageUrl { get; set; }

        public bool flg { get; set; }

        public string userId { get; set; }

        [StringLength(50)]
        public string userName { get; set; }

        public DateTime addTime { get; set; }
    }
}