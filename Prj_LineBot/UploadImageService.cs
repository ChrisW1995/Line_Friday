using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prj_LineBot.Models;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using System.IO;

namespace Prj_LineBot
{
    public class UploadImageService
    {
        string IMGUR_CLIENT_ID = "6b408e6c7dd89bb";
        string IMGUR_CLIENT_SECRET = "fec86fa7ee890b15ee2c6ade149ab3e54d30fa78";
        LocalTimeService timeService = new LocalTimeService();

        public bool isUploading(string userId)
        {
            using(var db = new LineModel())
            {
                var check = db.UploadStatus.Where(i => i.UserId == userId && i.CommandStr=="--upload").FirstOrDefault();
                return (check != null);
            }
        }

        public string addUploadStatus(string userId)
        {
            var reply = "";
            try
            {
                using(var db = new LineModel())
                {
                    var instance = new UploadStatus
                    {
                        UserId = userId,
                        CommandStr = "--upload"
                    };
                    db.UploadStatus.Add(instance);
                    db.SaveChanges();
                }
                reply = "Okay! I got a \"upload\" command, plz upload a image to me.  :)";
            }
            catch (Exception e)
            {

                throw;
            }

            return reply;
        }

        public string UploadImage(byte[] msgArray, string user_id, string display_name)
        {
            var today = timeService.GetLocalDateTime(LocalTimeService.CHINA_STANDARD_TIME);
            var client = new ImgurClient(IMGUR_CLIENT_ID, IMGUR_CLIENT_SECRET);
            var endpoint = new ImageEndpoint(client);
            IImage image;

            Stream stream = new MemoryStream(msgArray);
            using (stream)
            {
                image = endpoint.UploadImageStreamAsync(stream).GetAwaiter().GetResult();
            }
            using (var db = new LineModel())
            {
                var instance = new UploadImage
                {
                    addTime = today,
                    flg = false,
                    imageUrl = image.Link,
                   userId = user_id,
                  userName = display_name
                };
                db.UploadImage.Add(instance);
                db.SaveChanges();

                var status = db.UploadStatus.Where(x => x.UserId == user_id && x.CommandStr == "--upload").ToList();
                db.UploadStatus.RemoveRange(status);

                db.SaveChanges();
            }
            return image.Link;

        }



    }
}