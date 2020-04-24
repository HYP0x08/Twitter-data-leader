using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        public static List<string> PaiMingYagoo(List<string> Vale)
        {
            //读取临时数据
            TwitterDataList ReadData;
            lock (Twitter_API.DataLock)
            { ReadData = Twitter_API.dataList; }
            List<string> TempStr = new List<string>();
            List<TwitterData> TempData = new List<TwitterData>();
            foreach (string Temp in Vale)
            { TempData.Add(ReadData.m_data[Temp]); }
            TempData.Sort((TwitterData h1, TwitterData h2) =>
            { return h2.m_now24.CompareTo(h1.m_now24); });
            foreach (var TempDatas in TempData)
            { TempStr.Add(TempDatas.m_screen_name); }
            return TempStr;
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
            string x = @"[\u0800-\u9FFF]+";
            MatchCollection Matches = Regex.Matches
            (oriText, x, RegexOptions.IgnoreCase);
            StringBuilder sb = new StringBuilder();
            foreach (Match NextMatch in Matches)
            {
                sb.Append(NextMatch.Value);
            }
            if (sb.ToString().Length >= 5)
                return DelEmoji(sb.ToString().Substring(0,5));
            return DelEmoji(sb.ToString());
        }
        public static string DelEmoji(string str)
        {
            string result = Regex.Replace(str, @"\p{Cs}", "");
            return result;
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
        public static void ReadConfig()
        {
            Twitter_API.showList.m_showlist.Clear();
            Console.WriteLine("Read Config：Loging......");
            if (!File.Exists("TwitterConfig.conf"))
            {
                //写入配置文件
                File.WriteAllText("TwitterConfig.conf", null);
                File.WriteAllText("ReadMe", "Hi Write Config Line a One.");
                Console.WriteLine("Out Write Config：Ok");
                Console.WriteLine("Plese，Close reset run");
                Console.ReadKey();
                return;
            }
            string Temp_team = "";
            ShowData Temp_Show = null;
            string[] Str = File.ReadAllLines("TwitterConfig.conf");
            foreach (string GetName in Str)
            {
                //初始化获取列表
                if (GetName.Contains("["))
                {
                    if (Temp_Show != null)
                    {
                        Twitter_API.showList.Add(Temp_team, Temp_Show);
                    }
                    Temp_team = GetName.Replace("[", "").Replace("]", "");
                    if (Temp_team != "End")
                        Temp_Show = new ShowData(Temp_team);
                    continue;
                }
                Temp_Show.Add(GetName);
                if (!Twitter_API.dataList.Searh(GetName))
                {
                    Twitter_API.dataList.Add(GetName);
                }
            }
            Console.WriteLine("Read Config：Ok");
            Console.WriteLine("导入组合数：{0}", Twitter_API.showList.m_showlist.Count.ToString());
            Console.WriteLine("导入偶像数：{0}", Twitter_API.dataList.m_data.Count.ToString());
        }
        public static void OutImage(bool OutImageOk = false)
        {
            Console.WriteLine("正在构造基础函数....");
            Twitter_API.Getgraphql();
            //读取后再操作, 方便处理
            Console.WriteLine("UTC时间:{0}", DateTime.UtcNow.ToString());
            Console.WriteLine("正在获取数据并输出....");
            Dictionary<string, TwitterData> ReadData;
            lock (Twitter_API.DataLock)
            { ReadData = new Dictionary<string, TwitterData>(Twitter_API.dataList.m_data); }
            foreach (var Temp in ReadData)
            {
                //异步获取数据
                Twitter_API.GetUserLike(Temp.Key);
            }
            while (true)
            {
                int ConutOK = 0;
                Thread.Sleep(1000);
                lock (Twitter_API.DataLock)
                { ReadData = new Dictionary<string, TwitterData>(Twitter_API.dataList.m_data); }
                foreach (var Temp in ReadData)
                {
                    if (Temp.Value.m_Getok)
                        ConutOK++;
                }
                if (ConutOK == ReadData.Count)
                    break;
            }
            var ReadShow = Twitter_API.showList;
            if(!OutImageOk)
            OutDataimage(ReadShow, ReadData);
        }
        public static void OutDataimage(ShowDataList ReadShow, Dictionary<string, TwitterData> ReadData)
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
                    image.Mutate(x => x.DrawText("今日" + Temp_Show.Value.m_name + "推特趋势", Tetitfont, Rgba32.Black, new Vector2(((White - TilteLend) - DelWhite) / 2, 20)));
                    image.Mutate(x => x.DrawText("八兆木悟志出品", SuiYingfont, Rgba32.FromHex("00000042"), new Vector2(195, imageHead / 2 - 25)));
                    foreach (var Temp in PaiMingYagoo(Temp_Show.Value.m_screen_name))
                    {
                        int OutData = ReadData[Temp].m_now24 - ReadData[Temp].m_old24;
                        string OutText = GetChineseWord(ReadData[Temp].m_name).Replace("公", "").Replace("発", "").Replace("売", "").Replace("運", "").Replace("堀", "").Replace("中", "");
                        if(OutText.Length > 0)
                            OutText = OutText + "：" + ReadData[Temp].m_now24;
                        else
                            OutText = GetEnglishWord2(ReadData[Temp].m_name).Replace("MotoakiTanigo","") + "：" + ReadData[Temp].m_now24;
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
