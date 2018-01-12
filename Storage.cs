using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using FANNCSharp.Double;
using NQuotes;
using static forexAI.Logger;
using Color = System.Drawing.Color;

namespace forexAI
{
    internal class Storage
    {
        MemcachedClient mc = null;
        Dictionary<string, object> properties = new Dictionary<string, object>();

        public Storage()
        {
            if (Configuration.useMemcached)
                InitMemcached();
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
                    return string.Empty;

                string retrievedValue = string.Empty;
                retrievedValue = (string) Data.db.GetSetting(name);
                properties[name] = retrievedValue;
                return (retrievedValue != null && retrievedValue.Length > 0)
                    ? retrievedValue
                    : string.Empty;
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
            if (properties.Count <= 0)
                return;

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
            IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Parse(Configuration.MemcachedIP),
                                                   Configuration.MemcachedPort);
            config.Servers.Add(ipEndpoint);
            config.Protocol = MemcachedProtocol.Text;
            mc = new MemcachedClient(config);
            if (mc != null)
                log($"memcached up [0x{mc.GetHashCode()}]");
        }
    }
}
