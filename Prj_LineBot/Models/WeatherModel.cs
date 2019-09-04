using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prj_LineBot.Models
{
    public class WeatherModel
    {
        public Records records { get; set; }
    }
    public class Records
    {
        public List<Location> location { get; set; }
    }

    public class Location
    {
        public string locationName { get; set; }
        public Time time { get; set; }
        public List<WeatherElement> weatherElement { get; set; }
        public List<Parameter> parameter { get; set; }
    }

    public class Time
    {
        public DateTime obsTime { get; set; }
    }

    public class WeatherElement
    {
        public string elementName { get; set; }

        public string elementValue { get; set; }
    }

    public class Parameter
    {
        public string parameterName { get; set; }

        public string parameterValue { get; set; }
    }
}