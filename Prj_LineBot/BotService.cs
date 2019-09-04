using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Prj_LineBot.Models;
using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Data.Entity;
using System.Security.Cryptography;

namespace Prj_LineBot
{
    public class BotService
    {
        string[] answerArr;

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        public static int RandomNumber(int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(0, max);
            }
        }

        Dictionary<string, string> MovieKeys = new Dictionary<string, string>() {
            {"國賓", "AT"}, {"威秀", "VS"}
        };
        //威秀
        Dictionary<string, string> vsMovieDict = new Dictionary<string, string>() {
            {"信義", "TP"}, {"日新", "XM"},
            {"京站", "QS"}, {"板橋大遠百", "BQ" }
        };
        //國賓
        Dictionary<string, string> atMovieDict = new Dictionary<string, string>() {
            { "西門國賓", "84b87b82-b936-4a39-b91f-e88328d33b4e"},
            { "威風國賓", "5c2d4697-7f54-4955-800c-7b3ad782582c"},
            { "中和環球國賓", "84b87b82-b936-4a39-b91f-e88328d33b4e"},
            { "長春國賓", "453b2966-f7c2-44a9-b2eb-687493855d0e"},
            { "林口國賓", "9383c5fa-b4f3-4ba8-ba7a-c25c7df95fd0"},
            { "新莊國賓", "3301d822-b385-4aa8-a9eb-aa59d58e95c9"}

        };
        static BotService()
        {
        }

        public static string GetUrlPath()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/Image"));
            FileInfo[] files = directoryInfo.GetFiles("*.*");
            int fileCount = files.Count();

            int index = RandomNumber(fileCount + 1);
            return $"/Image/{files[index].Name}";
        }

     
        public string GetAnimalImg()
        {

            DirectoryInfo directoryInfo = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/Image/animal"));
            FileInfo[] files = directoryInfo.GetFiles("*.*");
            int fileCount = files.Count();
            Random random = new Random();
            int index = random.Next(1, fileCount + 1);
            return $"/Image/animal/{files[index].Name}";

        }

        public string GetHoroscope(string _cate)
        {
            string msg = "";
            var link = "";
            try
            {
                using (WebClient wc = new WebClient())
                {
                    byte[] bResult = wc.DownloadData("http://www.daily-zodiac.com/mobile");
                    string result = Encoding.UTF8.GetString(bResult);
                    var htmlDoc = new HtmlDocument();

                    htmlDoc.LoadHtml(result);

                    var section = htmlDoc.DocumentNode.SelectSingleNode($"//*[@id=\"mobile\"]/ul");
                    link = section.SelectNodes("li").Where(x => x.InnerText.Contains(_cate)).SingleOrDefault().SelectSingleNode("a").Attributes["href"].Value;
                    // link = section.Where(x => x.InnerText.Contains(_cate)).FirstOrDefault().Attributes["href"].Value;

                    bResult = wc.DownloadData($"http://www.daily-zodiac.com{link}");
                    result = Encoding.UTF8.GetString(bResult);
                    htmlDoc.LoadHtml(result);
                    var content = htmlDoc.DocumentNode.SelectSingleNode($"//*[@class=\"middle\"]");
                        //.InnerText.Replace("\n", "").Replace(" ", "");
                    //var title = content.Substring(0, content.IndexOf('今')).Insert(3, " - ") + "\r\n";
                    //var title2 = content.Substring(content.IndexOf("今"), content.IndexOf(")") + 1 - (content.IndexOf("今"))) + "\r\n==================\r\n";
                    //var article = content.Substring(content.IndexOf(")") + 1);
                    //msg += title + title2 + article;
                    var ps = content.SelectSingleNode($"//*[@class=\"name\"]").SelectNodes("p");
                    var lis = content.SelectSingleNode($"//*[@class=\"today\"]").SelectNodes("li"); ;
                    foreach (var item in ps)
                    {
                        msg += item.InnerText.Replace("\n", "").Trim() + " ";
                    }
                    msg += "\r\n";
                    foreach (var item in lis)
                    {
                        msg += item.InnerText.Replace(" ", "").Trim() + " ";
                    }
                    msg = msg.Insert(msg.IndexOf("("), " ");
                    var article = content.SelectNodes("div")[2].SelectSingleNode("section").InnerText.Replace("\n", "").Trim();
                    msg += "\r\n===================\r\n" + article;


                }

            }
            catch (Exception e)
            {
                msg += e.ToString().Substring(0, 100);
                throw;
            }

            return msg;
        }

        public string AddQuestion(string userId)
        {
            using (var db = new LineModel())
            {
                if (HaveQuestion(userId) != 0) return "";
                var question = db.MovieQuestion.OrderBy(x => x.Id).FirstOrDefault();
                var instance = new QuestionProcess
                {
                    M_Order = question.Id,
                    UserId = userId,
                    QuestionStatus = false,
                    ErrorCount = 0
                };
                db.QuestionProcess.Add(instance);
                db.SaveChanges();
                return question.M_Question;
            }

        }

        public int HaveQuestion(string userId)
        {

            using (var db = new LineModel())
            {

                var instances = db.QuestionProcess.Where(x => x.UserId == userId).ToList();
                if (instances.Count > 1)
                {
                    db.QuestionProcess.RemoveRange(instances);
                    db.SaveChanges();
                }
                    
                return instances.Count == 1 ? instances[0].M_Order : 0;
            }

        }

        public string NextQuestion(string userId, string msg)
        {
            var reply = "";

            using (var db = new LineModel())
            {
                var current_question = db.QuestionProcess.Where(x => x.UserId == userId).SingleOrDefault();
                int movie_next_order = current_question.M_Order + 1;

                if (!msg.ToLower().Contains("取消"))
                {
                    switch (current_question.M_Order)
                    {
                        case 1:
                            if (msg == "威秀" || msg== "國賓")
                            {
                                
                                current_question.M_Order = movie_next_order;
                                current_question.Answer += msg;
                                reply = db.MovieQuestion.Where(x => x.Id == movie_next_order).SingleOrDefault().M_Question + "," + movie_next_order + "," + MovieKeys[msg];
                            } 
                            else
                            {
                                reply = "請點選下方之選項！,1";
                                current_question.ErrorCount += 1;
                            }

                            break;
                        case 2:
                            switch (current_question.Answer)
                            {
                                case "威秀":
                                    if (!vsMovieDict.ContainsKey(msg))
                                    {
                                        reply = "請選擇當前有的選項！,2,VS";
                                        current_question.ErrorCount += 1;
                                    }
                                    else
                                    {
                                        current_question.M_Order = movie_next_order;
                                        current_question.Answer += "," + msg;
                                        reply = db.MovieQuestion.Where(x => x.Id == movie_next_order).SingleOrDefault().M_Question + "," + movie_next_order + ",VS";

                                    }
                                    break;

                                case "國賓":
                                    if(msg == "新莊"|| msg == "林口")
                                    {
                                        msg = msg + "國賓";
                                    }
                                    if (atMovieDict.ContainsKey(msg))
                                    {         
                                        current_question.M_Order = movie_next_order;
                                        current_question.Answer += "," + msg;
                                        reply = db.MovieQuestion.Where(x => x.Id == movie_next_order).SingleOrDefault().M_Question + "," + movie_next_order + ",AT";

                                    }
                                    else
                                    {
                                        reply = "請選擇當前有的選項！,2,AT";
                                        current_question.ErrorCount += 1;
                                    }
                                    break;
                            }
                            
                            break;

                        case 3:
                            if (msg.Length < 2)
                                return "重新輸入一次。請輸入長度至少二字以上查詢電影！或輸入'取消'結束對話。,-1";
                            var movie_str = "";
                            answerArr = current_question.Answer.Split(',');

                            switch (answerArr[0])
                            {
                                case "威秀":
                                    movie_str = GetVSMovieTime(answerArr[1], msg);
                                    break;

                                case "國賓":
                                    movie_str =  GetATMovieTimeAsync(answerArr[1], msg);
                                    break;
                            }
                            
                            if (movie_str != "")
                            {
                                reply += movie_str + ",-1";
                                db.QuestionProcess.Remove(current_question);
                            }
                            else
                            {
                                reply = $"關鍵字'{msg}'查無結果請重新輸入。或輸入'取消'結束對話,-1";

                            }
                            break;

                        default:
                            reply = "ERROR";
                            break;
                    }
                    if (current_question.ErrorCount > 3)
                    {
                        reply = "錯誤次數過多，請重新呼叫一次,-1";
                        db.QuestionProcess.Remove(current_question);
                    }

                }
                else
                {
                    reply = "break!,-1";
                    db.QuestionProcess.Remove(current_question);
                }
                db.SaveChanges();
            }
            return reply;
        }

        public string GetATMovieTimeAsync(string location, string keyword)
        {
            LocalTimeService timeService = new LocalTimeService();
            var today = timeService.GetLocalDateTime(LocalTimeService.CHINA_STANDARD_TIME).Date;
            var str = "";
            for(int count = 0; count < 3; count++)
            {
                var movieDate = today.AddDays(count).ToString("yyyy/MM/dd");
                var url = $"http://www.ambassador.com.tw/ambassadorsite.webapi/api/Movies/GetShowtimeListForTheater/?theaterId={atMovieDict[location]}&showingDate={movieDate}";
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetStringAsync(url).Result;
                    var json = JsonConvert.DeserializeObject<List<ATMovieModel>>(response).Where(x => x.Name.Contains(keyword)).ToList();
                    for (int i = 0; i < json.Count; i++)
                    {
                        str += json[i].PeriodShowtime[0].PlayingDate.ToString("yyyy/MM/dd") + "\r\n";
                        foreach (var item in json[i].PeriodShowtime)
                        {
                            str += item.AssistantName + "\r\n";
                            var timeArray = string.Join(" | ", item.Showtimes);
                            str += timeArray + "\r\n";
                        }
                        str += "============\r\n";
                    }
                }
            }
           

            return str;
        }

        public string GetVSMovieTime(string location, string keyword)
        {
            var str = "";
            var html = "";

            using (HttpClient client = new HttpClient())
            {
                // 指定 authorization header
                client.DefaultRequestHeaders.Add("authorization", "token {api token}");
                // 準備寫入的 data
                VsPostModel model = new VsPostModel { CinemaCode = vsMovieDict[location] };
                // 將 data 轉為 json
                string json = JsonConvert.SerializeObject(model);
                // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                // 發出 post 並取得結果
                HttpResponseMessage response = client.PostAsync("http://www.vscinemas.com.tw/ShowTimes//ShowTimes/GetShowTimes", contentPost).Result;
                // 將回應結果內容取出並轉為 string 再透過 linqpad 輸出
                html = response.Content.ReadAsStringAsync().Result;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var section = htmlDoc.DocumentNode.SelectNodes($"/div").Where(x => x.InnerText.Contains(keyword)).ToList();
            if (section.Count == 0) return "";

            foreach (var div in section)
            {
                str += div.Elements("strong").FirstOrDefault().InnerText.Trim() + "\r\n";
                var innerDate = div.Element("div").Elements("strong").Where(x=>!x.Attributes["class"].Value.Contains("LangEN")).Take(3).ToList();
                var innerTime = div.Element("div").Elements("div").Where(x => x.Attributes["class"].Value.Contains("SessionTimeInfo")).Take(3).ToList();
                for(int i = 0; i< innerDate.Count(); i++)
                {
                    str += innerDate[i].InnerText.Trim().Replace("\r\n", " ") + "\r\n";
                    var timeArray = string.Join(" | ",innerTime[i].InnerText.Replace("\r\n", "").Split(' ').Where(x=>!string.IsNullOrEmpty(x)).ToList());
                    str += timeArray + "\r\n";
                }
                str += "============\r\n";
            }

            return str;
        }
    
        public string GetMovieList()
        {
            string msg = "";
            List<string[]> strArrList = new List<string[]> ();
            try
            {
                using (WebClient wc = new WebClient())
                {
                    byte[] bResult = wc.DownloadData("https://movies.yahoo.com.tw/chart.html?cate=rating");
                    string result = Encoding.UTF8.GetString(bResult);
                    var htmlDoc = new HtmlDocument();

                    htmlDoc.LoadHtml(result);

                    var rows = htmlDoc.DocumentNode.SelectNodes($"//div[@class=\"tr\"]").ToList();
                    foreach (var _row in rows)
                    {
                        var tds = _row.Elements("div").Select(x=>x.InnerText.Replace("\n","").Replace(" ","").Trim()).ToArray();
                        //msg += $"{tds[1]} {tds[2]} {tds[4].Substring(0,3)}\r\n";
                        strArrList.Add(tds);
                    }
                    var groupedList = strArrList.GroupBy(x => x[2]).OrderByDescending(x=>x.Key).ToList();
                    foreach (var item in groupedList)
                    {
                        msg += $"上映日期：{item.Key}\r\n";
                        for(int i = 0; i< item.Count(); i++)
                        {
                            msg += $"{item.ElementAt(i)[1]} -- {item.ElementAt(i)[4].Substring(0, item.ElementAt(i)[4].IndexOf("共"))}/5\r\n";
                        }
                        msg += "==================\r\n";
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
            return msg;
        }

        public string GetRandomGuy()
        {
            var img_url = "";
            using(var db = new LineModel())
            {
                var list = db.UploadImage.Where(x => x.flg == true).ToList();
                if(list.Count != 0)
                {
                    int index = RandomNumber(list.Count + 1);
                    img_url = list[index].imageUrl;
                }
            }
            return img_url;
        }

        public static string GetAppsettingsStr(string text)
        {
            string settingStr = System.Configuration.ConfigurationManager.AppSettings[text];
            return settingStr;
        }
    }
}