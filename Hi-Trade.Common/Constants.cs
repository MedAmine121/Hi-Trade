using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi_Trade.Common
{
    public class Constants
    {
        public const string FinnHubApi = "https://finnhub.io/api/v1/quote?symbol=";
        public const string StaticFilesPath = "wwwroot/";
        public const string ProfilePicsPath = "ProfilePictures/";
        public static string GetFileExtension(string base64String)
        {
            var data = base64String.Substring(0, 5);

            return data.ToUpper() switch
            {
                "IVBOR" => "png",
                "/9J/4" => "jpg",
                "AAAAF" => "mp4",
                "JVBER" => "pdf",
                "AAABA" => "ico",
                "UMFYI" => "rar",
                "E1XYD" => "rtf",
                "U1PKC" => "txt",
                "MQOWM" or "77U/M" => "srt",
                _ => string.Empty,
            };
        }
    }
}
