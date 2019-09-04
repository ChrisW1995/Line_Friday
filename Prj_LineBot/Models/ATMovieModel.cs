using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prj_LineBot.Models
{

    public class ATMovieModel
    {
        public string Name { get; set; }

        public DateTime ReleaseDate { get; set; }

        public List<PeriodShowtime> PeriodShowtime { get; set; }

    }

    public class PeriodShowtime
    {
        public string AssistantName { get; set; }

        public DateTime PlayingDate { get; set; }

        public List<string> Showtimes { get; set; }


    }

}