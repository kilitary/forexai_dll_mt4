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

        public object this[string name]
        {
            get
            {
                if (properties.ContainsKey(name))
                    return properties[name];

                string retrievedValue = "";
                retrievedValue = (string) Data.db.GetSetting(name);
                properties[name] = retrievedValue;
                return (retrievedValue != null && retrievedValue.Length > 0) ? retrievedValue : "";
            }
            set
            {
                properties[name] = value;
               // mc.Store(StoreMode.Set, name, value);
            }
        }

        public void UpMemcache()
        {
            MemcachedClientConfiguration config = new MemcachedClientConfiguration();
            IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Parse("192.168.10.10"), 11211);
            config.Servers.Add(ipEndpoint);
            config.Protocol = MemcachedProtocol.Text;
            mc = new MemcachedClient(config);
            if (mc != null)
                log($"memcached up [0x{mc.GetHashCode()}]");
        }

        public Storage()
        {
        }

        public void SyncData()
        {
            debug($"storage: storing {properties.Count} key-value pairs.");
            foreach (KeyValuePair<string, object> o in properties)
            {
                Data.db.StoreSetting(o.Key, o.Value);
            //    mc.Store(StoreMode.Set, o.Key, o.Value);
            }
        }

        ~Storage()
        {
            log("DESTROY CALLED!");
            SyncData();
        }
    }
}