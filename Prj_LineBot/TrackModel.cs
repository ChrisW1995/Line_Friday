using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prj_LineBot
{

    public class TrackModel
    {
        public data data { get; set; }
    }
    public class data
    {
        public origin_info origin_info { get; set; }

        public string tracking_number { get; set; }

        public string status { get; set; }

        public string destination_country { get; set; }

        public string lastEvent { get; set; }

        public string lastUpdateTime { get; set; }


    }
    public class origin_info
    {
        public List<trackinfo> trackinfo { get; set; }
    }

    public class trackinfo
    {
        public DateTime Date { get; set; }

        public string StatusDescription { get; set; }

        public string Details { get; set; }

        public string checkpoint_status { get; set; }
    }
}