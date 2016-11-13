using Shadowsocks.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks.Model.ThirdData
{
    public abstract class AstractIshadowsocks
    {
        public ShadowsocksController controller { get; set; }

        public Configuration config { get; set; }

        public List<Server> newServerList{ get; set; }

        public Boolean SaveServers()
        {
            return this.SaveServers(this.newServerList);
        }

        public Boolean SaveServers(List<Server> list)
        {
            try
            {
                if (list == null || list.Count == 0) return false;
                NetSailServerCompare netSailServerCompare = new NetSailServerCompare();
                var servers = config.configs.Union(list).Distinct(netSailServerCompare).ToList(); //config去掉重复的服务名称
                controller.SaveServers(servers, config.localPort);
            }
            catch (Exception e)
            {
                Logging.Error(string.Format("Refresh config file error:{0}", e));
            }
            return true;
        }
        public static Boolean OpenConfigFile()
        {
            string configFile = "gui-config.json";
            if (!System.IO.File.Exists(configFile)) return false;
            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo(configFile);
            System.Diagnostics.Process Pro = System.Diagnostics.Process.Start(Info);
            return true;
        }

        public void MarkConfigNewTime(Server item)
        {
            if (config.configs.Count > 1 && item != null)
            {
                Server oldServer = config.configs.Where(m => m.server == item.server).First();
                item.remarks = ((oldServer != null && oldServer.password != item.password)) ? item.remarks : oldServer.remarks;
            }
        }

    }
}
