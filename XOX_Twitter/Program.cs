using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace XOX_Twitter
{
    class Program
    {
        static bool NowBoolOk = true;
        static bool NowBoolOk2 = true;
        static void Main(string[] args)
        {
            Console.WriteLine("Read Config：Loging......");
            if (!File.Exists("TwitterConfig.conf"))
            {
                //写入配置文件
                File.WriteAllText("TwitterConfig.conf",null);
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
                    Temp_team = GetName.Replace("[","").Replace("]","");
                    if(Temp_team != "End")
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
            while (true)
            {
                string Time = DateTime.UtcNow.ToString("%H");
                //首次运行执行
                if (NowBoolOk2)
                    Time = "15";
                switch (Time) {
                    case "14":
                        if (!NowBoolOk)
                        {
                            lock (Twitter_API.DataLock)
                            {
                                foreach (var Temp in Twitter_API.dataList.m_data)
                                {
                                    //预刷新数据
                                    NowBoolOk = true;
                                    Temp.Value.m_Getok = false;
                                }
                            }
                        }
                        Console.WriteLine("UTC时间:{0}", DateTime.UtcNow.ToString());
                        Thread.Sleep(60000);
                        break;
                    case "15":
                        if (NowBoolOk)
                        {
                            //初始构造
                            Console.WriteLine("正在构造基础函数....");
                            Twitter_API.Getgraphql();
                            //读取后再操作, 方便处理
                            Console.WriteLine("UTC时间:{0}", DateTime.UtcNow.ToString());
                            Console.WriteLine("正在获取数据并输出....");
                            TwitterDataList ReadData;
                            lock (Twitter_API.DataLock)
                            { ReadData = Twitter_API.dataList; }
                            foreach (var Temp in ReadData.m_data)
                            {
                                //异步获取数据
                                Twitter_API.GetUserLike(Temp.Key);
                            }
                            while (true)
                            {
                                int ConutOK = 0;
                                Thread.Sleep(1000);
                                lock (Twitter_API.DataLock)
                                { ReadData = Twitter_API.dataList; }
                                foreach (var Temp in ReadData.m_data)
                                {
                                    if (Temp.Value.m_Getok)
                                        ConutOK++;
                                }
                                if (ConutOK == ReadData.m_data.Count)
                                    break;
                            }
                            var ReadShow = Twitter_API.showList;
                            //导出保存文件
                            Tools.OutDataimage(ReadShow, ReadData);
                            //重置数据
                            NowBoolOk = false;
                            NowBoolOk2 = false;
                        }
                        Console.WriteLine("UTC时间:{0}", DateTime.UtcNow.ToString());
                        Thread.Sleep(60000);
                        break;
                    default:
                        Console.WriteLine("UTC时间:{0}", DateTime.UtcNow.ToString());
                        Thread.Sleep(60000);
                        break;
                }
            }
        }
    }
}
