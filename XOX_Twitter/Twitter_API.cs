using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace XOX_Twitter
{
    public static class Twitter_API
    {
        static string TwitterAPI_data = "";
        static string TwitterAPI_Bearer = "";
        public static object DataLock = new object();
        public static ShowDataList showList = new ShowDataList();
        public static TwitterDataList dataList = new TwitterDataList();
        static string TwitterAPI_Urldata = "https://twitter.com/";
        static string TwitterAPI_GRAPHQL = "https://api.twitter.com/graphql/"+ TwitterAPI_data + "/UserByScreenName";
        public static bool Getgraphql()
        {
            //初始化参数
            string GetString = null;
            RequestHttpWebRequest request = new RequestHttpWebRequest();
            WebHeaderCollection webHeader = new WebHeaderCollection();
            webHeader.Add("Sec-Fetch-User", "?1");
            webHeader.Add("Upgrade-Insecure-Requests", "1");
            webHeader.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36");
            request.GetResponseAsync(new RequestInfo(TwitterAPI_Urldata) { Headers = webHeader }, x => {
                GetString = x.GetString(Encoding.UTF8);
            });
            while (GetString == null)
            { Thread.Sleep(1000); }
            string GetJs = Tools.MidStrEx_New(GetString, "crossorigin=\"anonymous\" href=\"https://abs.twimg.com/responsive-web/web/main.", ".js");
            Console.WriteLine(GetJs);
            //重置返回值
            GetString = null;
            request.GetResponseAsync(new RequestInfo("https://abs.twimg.com/responsive-web/web/main."+ GetJs + ".js") , x =>
            {
                GetString = x.GetString(Encoding.UTF8);
            });
            while (GetString == null)
            { Thread.Sleep(1000); }
            TwitterAPI_Bearer = "AAAAAAAAAAAAAAAAAAAAA" + Tools.MidStrEx_New(GetString, "AAAAAAAAAAAAAAAAAAAAA", "\"");
            Console.WriteLine(TwitterAPI_Bearer);
            string TempStr = "fDBV:function(e,t){e.exports={queryId:\"";
            TwitterAPI_data = GetString.Substring(GetString.IndexOf(TempStr) + TempStr.Length, 22);
            TwitterAPI_GRAPHQL = "https://api.twitter.com/graphql/" + TwitterAPI_data + "/UserByScreenName";
            Console.WriteLine(TwitterAPI_data);
            return true;
        }
        public static void GetUserLike(string screen_name)
        {
            //初始化参数
            var GetData = new Dictionary<string, string>() {
                { "variables" ,"{\"screen_name\":\"" +  screen_name  + "\",\"withHighlightedLabel\":true}" }
            };
            string GetDataSign = Tools.DicToPostData(GetData);
            RequestHttpWebRequest request = new RequestHttpWebRequest();
            WebHeaderCollection webHeader = new WebHeaderCollection();
            webHeader.Set("content-type", "application/json");
            webHeader.Add("referer", "https://twitter.com/" + screen_name);
            webHeader.Add("authorization", "Bearer " + TwitterAPI_Bearer);
            webHeader.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36");
            request.GetResponseAsync(new RequestInfo(TwitterAPI_GRAPHQL + "?" + GetDataSign) {  Headers = webHeader }, GetUserLike_Return);
        }
        private static void GetUserLike_Return(ResponseInfo e)
        {
            string screen_name = Tools.MidStrEx_New(e.RequestInfo.Url, "%7B%22screen_name%22%3A%22", "%22%2C%22withHighlightedLabel%22%3Atrue%7D");
            var Temp_JSON = (JObject)JsonConvert.DeserializeObject(e.GetString(Encoding.UTF8));
            //获取数据
            if (Temp_JSON["code"] != null)
            {
                Console.WriteLine("获取数据失败，正在重新获取。。。。");
                Console.WriteLine("Debug：{0}", screen_name);
                GetUserLike(screen_name);
                return;
            }
            TwitterData TempTwData;
            lock (DataLock)
            { TempTwData = dataList.m_data[screen_name]; }
            TempTwData.m_name =  Temp_JSON["data"]["user"]["legacy"]["name"].ToString();
            TempTwData.m_old24 = TempTwData.m_now24;
            TempTwData.m_now24 = int.Parse(Temp_JSON["data"]["user"]["legacy"]["followers_count"].ToString());
            TempTwData.m_Getok = true;
            lock (DataLock)
            { dataList.m_data[screen_name] = TempTwData; }
            Console.WriteLine("{0}的数据已经刷新完毕.", screen_name);
        }
    }
    public class ShowDataList
    {
        public Dictionary<string, ShowData> m_showlist { get; set; }
        public ShowDataList()
        {
            m_showlist = new Dictionary<string, ShowData>();
        }
        public void Add(string name,ShowData datalist)
        {
            m_showlist.Add(name, datalist);
        }
        public ShowData Get(string DataInOf)
        {
            return m_showlist[DataInOf];
        }
    }
    public class ShowData
    {
        public string m_name { get; set; }
        public List<string> m_screen_name { get; set; }
        public ShowData(string name)
        {
            m_name = name;
            m_screen_name = new List<string>();                
        }
        public void Add(string screen_name)
        {
            m_screen_name.Add(screen_name);
        }
        public List<string> Get()
        {
            return m_screen_name;
        }
    }
    public class TwitterDataList
    {
        public Dictionary<string, TwitterData> m_data { get; set; }
        public TwitterDataList()
        {
            m_data = new Dictionary<string, TwitterData>();
        }
        public void Add(string screen_name)
        {
            m_data.Add(screen_name, new TwitterData(screen_name));
        }
        public TwitterData Get(string DataInOf)
        {
            return m_data[DataInOf];
        }
        public bool Searh(string DataInOf)
        {
            return m_data.ContainsKey(DataInOf);
        }
    }
    public class TwitterData
    {
        public string m_name { get; set; }
        public string m_screen_name { get; set; }
        public int m_old24 { get; set; }
        public int m_now24 { get; set; }
        public bool m_Getok { get; set; }
        public TwitterData(string screen_name)
        {
            m_screen_name = screen_name;
        }
    }
}
