using System;
using Color = System.Drawing.Color;
using NQuotes;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Logger;
using FANNCSharp.Double;
using System.Text.RegularExpressions;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using System.Net;

namespace forexAI
{
    internal class Storage
    {
        private MemcachedClient mc = null;
        private Dictionary<string, object> properties = new Dictionary<string, object>();

        public Storage()
        {
        }

        ~Storage()
        {
            log("Storage DESTROY CALLED!");
            SyncData();
        }

        public object this[string name]
        {
            get
            {
                if (properties.ContainsKey(name))
                    return properties[name];

                if (!Configuration.useMysql)
                    return "";

                string retrievedValue = "";
                retrievedValue = (string) Data.db.GetSetting(name);
                properties[name] = retrievedValue;
                return (retrievedValue != null && retrievedValue.Length > 0) ? retrievedValue : "";
            }
            set
            {
                properties[name] = value;
                if (Configuration.useMemcached)
                    mc.Store(StoreMode.Set, name, value);
            }
        }

        public void SyncData()
        {
            debug($"storage: storing {properties.Count} key-value pairs.");
            foreach (KeyValuePair<string, object> o in properties)
            {
                if (Configuration.useMysql)
                    Data.db.SetSetting(o.Key, o.Value);
                if (Configuration.useMemcached)
                    mc.Store(StoreMode.Set, o.Key, o.Value);
            }
        }

        public void InitMemcached()
        {
            MemcachedClientConfiguration config = new MemcachedClientConfiguration();
            IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Parse("192.168.10.10"), 11211);
            config.Servers.Add(ipEndpoint);
            config.Protocol = MemcachedProtocol.Text;
            mc = new MemcachedClient(config);
            if (mc != null)
                log($"memcached up [0x{mc.GetHashCode()}]");
        }
    }
}