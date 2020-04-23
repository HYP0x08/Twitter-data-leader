using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace XOX_Twitter
{
    public static class Tools
    {
        public delegate void AsyncEventHandler();
        public static string DicToPostData(Dictionary<string, string> Vale)
        {
            //降序排列
            string PostDataSign = "";
            foreach (var OutData in Vale)
            {
                PostDataSign += OutData.Key + "=" + UrlEncode(OutData.Value) + "&";
            }
            //返回PostData
            return PostDataSign.Substring(0, PostDataSign.Length - 1);
        }
        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if (HttpUtility.UrlEncode(c.ToString()).Length > 1)
                {
                    sb.Append(HttpUtility.UrlEncode(c.ToString(), Encoding.UTF8).ToUpper());
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public static string GetTimeStamp()
        {
            DateTime dateStart = new DateTime(1970, 1, 1, 8, 0, 0);
            int timeStamp = Convert.ToInt32((DateTime.Now - dateStart).TotalSeconds);
            return timeStamp.ToString();
        }
        public static string MidStrEx_New(string sourse, string startstr, string endstr)
        {
            Regex rg = new Regex("(?<=(" + startstr + "))[.\\s\\S]*?(?=(" + endstr + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(sourse).Value;
        }
        public static string GetChineseWord(string oriText)
        {
            string x = @"[\u4E00-\u9FFF]+";
            MatchCollection Matches = Regex.Matches
            (oriText, x, RegexOptions.IgnoreCase);
            StringBuilder sb = new StringBuilder();
            foreach (Match NextMatch in Matches)
            {
                sb.Append(NextMatch.Value);
            }
            return sb.ToString();
        }
        public static string GetEnglishWord(string oriText)
        {
            string x = @"[A-Za-z0-9]+";
            MatchCollection Matches = Regex.Matches
            (oriText, x, RegexOptions.IgnoreCase);
            StringBuilder sb = new StringBuilder();
            foreach (Match NextMatch in Matches)
            {
                sb.Append(NextMatch.Value);
            }
            return sb.ToString();
        }
        public static string GetEnglishWord2(string oriText)
        {
            string x = @"[A-Za-z]+";
            MatchCollection Matches = Regex.Matches
            (oriText, x, RegexOptions.IgnoreCase);
            StringBuilder sb = new StringBuilder();
            foreach (Match NextMatch in Matches)
            {
                sb.Append(NextMatch.Value);
            }
            return sb.ToString();
        }
        public static void OutDataimage(ShowDataList ReadShow,TwitterDataList ReadData)
        {
            var install_Family = new FontCollection().Install("SourceHanSansCN.ttf");
            var SuiYingfont = new Font(install_Family, 55);  //字体
            var Tetitfont = new Font(install_Family, 40);  //字体
            var Strfont = new Font(install_Family, 35);  //字体
            foreach (var Temp_Show in ReadShow.m_showlist)
            {
                int White = 750;
                int DelWhite = 240;
                int imageHead = ((73 + 20) + (60 * Temp_Show.Value.m_screen_name.Count));
                int TilteLend = (GetChineseWord(Temp_Show.Key).Length * 40) + (GetEnglishWord(Temp_Show.Key).Length * 23);
                using (Image<Rgba32> image = new Image<Rgba32>(White, imageHead))
                {
                    int AddTextLine = 80;
                    image.Mutate(x => x.BackgroundColor(Color.White));
                    image.Mutate(x => x.DrawText("今日" + Temp_Show.Value.m_name + "股票趋势", Tetitfont, Rgba32.Black, new Vector2(((White - TilteLend) - DelWhite) / 2, 20)));
                    image.Mutate(x => x.DrawText("八兆木悟志出品", SuiYingfont, Rgba32.FromHex("00000042"), new Vector2(195, imageHead / 2 - 25)));
                    foreach (var Temp in Temp_Show.Value.m_screen_name)
                    {
                        int OutData = ReadData.m_data[Temp].m_now24 - ReadData.m_data[Temp].m_old24;
                        string OutText = GetChineseWord(ReadData.m_data[Temp].m_name).Replace("公式", "").Replace("発売中", "").Replace("発売", "").Replace("運努勘感", "").Replace("堀", "");
                        if(OutText.Length > 0)
                            OutText = OutText + "：" + ReadData.m_data[Temp].m_now24;
                        else
                            OutText = GetEnglishWord2(ReadData.m_data[Temp].m_name) + "：" + ReadData.m_data[Temp].m_now24;
                        image.Mutate(x => x.DrawText(OutText, Strfont, Rgba32.Black, new Vector2(45, AddTextLine)));
                        if (OutData > 0)
                        {
                            image.Mutate(x => x.DrawText("↑ +" + OutData.ToString(), Strfont, Rgba32.FromHex("4cd01e"), new Vector2(500, AddTextLine)));
                        }
                        else
                        {
                            image.Mutate(x => x.DrawText("↓  " + OutData.ToString(), Strfont, Rgba32.FromHex("e20000"), new Vector2(500, AddTextLine)));
                        }
                        AddTextLine += 60;
                    }
                    image.Save("/usr/share/nginx/html/data/" + Temp_Show.Value.m_name + ".png");
                }
            }
        }
    }
}
