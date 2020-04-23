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
        static void backspace(int n)
        {
            for (var i = 0; i < n; ++i)
                Console.Write((char)0x8);
        }
        static void Main(string[] args)
        {
            //读取配置文件
            Tools.ReadConfig();
            new Thread(() => {
                while (true)
                {
                    string TimeOut = "UTC时间:" + DateTime.UtcNow.ToString();
                    Console.Write(TimeOut);
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
                            break;
                        case "15":
                            if (NowBoolOk)
                            {
                                //导出图片
                                Tools.OutImage();
                                Console.WriteLine("");
                                //重置数据
                                NowBoolOk = false;
                                NowBoolOk2 = false;
                            }
                            break;
                        default:
                            break;
                    }
                    backspace(TimeOut.Length + 2);
                    Thread.Sleep(1000);
                } })
            { IsBackground = true }.Start();
            while (true)
            {
                Console.WriteLine("需要更新配置文件请输入 reconfig");
                Console.WriteLine("需要刷新请输入 reload");
                string Read = Console.ReadLine();
                if (Read.ToLower() == "reconfig")
                {
                    //读取配置文件
                    Tools.ReadConfig();
                }
                if (Read.ToLower() == "reload")
                {
                    //立刻刷新数据
                    lock (Twitter_API.DataLock)
                    {
                        foreach (var Temp in Twitter_API.dataList.m_data)
                        {
                            //预刷新数据
                            Temp.Value.m_Getok = false;
                        }
                    }
                    Tools.OutImage();
                }
            }
        }
    }
}
