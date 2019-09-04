using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Prj_LineBot.Models
{
    public class UploadStatus
    {
        public int Id { get; set; }

        [StringLength(25)]
        public string CommandStr { get; set; }

        public string UserId { get; set; }
    }
}