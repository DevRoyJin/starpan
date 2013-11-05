using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiskAPIBase
{
    public class WebUtiltiy
    {
        public static bool IsProxyEnable
        {
            get { return IEProxyInfo.GetIEProxyInfo().Enabled; }
        }
    }
}
