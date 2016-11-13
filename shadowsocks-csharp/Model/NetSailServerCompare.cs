using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks.Model
{
    class NetSailServerCompare : IEqualityComparer<Server>
    {
        public bool Equals(Server x, Server y)
        {
            return x.server == y.server;
        }

        public int GetHashCode(Server obj)
        {
            return obj.ToString().ToLower().GetHashCode();
        }
    }
}
