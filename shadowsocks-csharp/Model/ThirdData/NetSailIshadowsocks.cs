using Shadowsocks.Controller;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Globalization;
using System.Linq;
using Shadowsocks.Model.ThirdData;

/// <summary>
/// http get iss config
/// auth netsail
/// time 2016-10-29 00:00
/// </summary>
namespace Shadowsocks.Model
{
    class NetSailIshadowsocks: AstractIshadowsocks
    {
        const string ISS_DEFAULT_URL = "http://www.ishadowsocks.net/";
        const int ISS_SERVER_NUM = 3;
        const string ISS_USER_AGENT = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.112 Safari/537.36";
        
        public NetSailIshadowsocks(ShadowsocksController controller)
        {
            this.controller = controller;
            config = this.controller.GetConfigurationCopy();
        }
        private string GetMatchData(string regex, string content, int index)
        {
            // Regex regex2 = new Regex(regex, RegexOptions.Singleline);
            //return regex2.Match(content).Groups[index].ToString();
            Match match = Regex.Match(content, regex);
            int localIndex = 1;
            while (match.Success)
            {
                //Console.WriteLine("Duplicate '{0}' found at position {1}.",
                //                  match.Groups[1].Value, match.Groups[2].Index);
                if (localIndex == index) break;
                match = match.NextMatch();
                localIndex++;
            }
            return match.Groups[1].Value;
        }

        public  List<Server> LoadServerList(string html)
        {
            if (html.IsNullOrEmpty()) return null;
           this.newServerList= new List<Server>();
            DateTimeFormatInfo provider = new DateTimeFormatInfo
            {
                LongDatePattern = "yyyy-MM-dd hh:mm:ss"
            };
            if (!html.IsNullOrEmpty())
            {
                DateTime maxUpdateTime = DateTime.Now;
                for (int i = 0; i < ISS_SERVER_NUM; i++)
                {
                    
                    Server item = new Server
                    {
                        server = GetMatchData(@"<h4>[\s\S]服务器地址:([\s\S]*?)<\/h4>", html, i + 1).ToUpper(),
                        password = GetMatchData(@"<h4>[\s\S]密码:([\s\S]*?)<\/h4>", html, i + 1),
                        server_port = int.Parse(GetMatchData(@"<h4>端口:([\s\S]*?)<\/h4>", html, i + 1)),
                        method = GetMatchData(@"<h4>加密方式:([\s\S]*?)<\/h4>", html, i + 1).ToUpper(),
                        remarks= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    this.MarkConfigNewTime(item);
                    if (config.configs.Count > i)
                    {
                        //设置更新时间最大的为默认配置
                        if (DateTime.Compare(Convert.ToDateTime(item.remarks, provider), maxUpdateTime) <= 0)
                        {
                            config.index = i;
                            maxUpdateTime = Convert.ToDateTime(item.remarks, provider);
                        }
                    }

                    newServerList.Add(item);
                }
            }
            return newServerList;
        }

        public async Task<string> AccessTheWebAsync()
        {
            string responseContents = null;
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("UserAgent", ISS_USER_AGENT);
                // GetStringAsync 返回 Task<string>. 这意味着当你等待这个任务的时候，你将获得一个字符串（urlContents）。
                Task<string> getStringTask = client.GetStringAsync(ISS_DEFAULT_URL);

                //这里你可以处理任务，它不依赖来自GetStringAsync的字符串
                //DoIndependentWork();

                // await 操作符延缓了AccessTheWebAsync.
                //  - AccessTheWebAsync 直到 getStringTask完成才继续执行。
                //  - 同时, 控制返回到 AccessTheWebAsync的调用者.
                //  - 当getStringTask完成时，控制恢复. 
                //  - await 操作符然后检索来自 getStringTask的字符串.
                responseContents = await getStringTask;
            }
            catch (Exception e)
            {
                Logging.Error(string.Format(@"请求地址:{0}异常:{1}", ISS_DEFAULT_URL, e));
            }
            //  return 语句表明返回一个整数.
            return responseContents;
        }

        //public Boolean checkChangedConfig()
        //{
        //    Server server=controller.GetCurrentServer();
        //    return true;
        //}

    }
}
