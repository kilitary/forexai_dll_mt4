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
	public class Storage
	{
		MemcachedClient memcachedServer = null;
		Dictionary<string, object> properties = new Dictionary<string, object>();

		public Storage()
		{
			debug($"storage INIT (Configuration.useMemcached={Configuration.memcahedEnabled}, useMysql={Configuration.mysqlEnabled})");

			if (Configuration.memcahedEnabled)
				InitMemcached();
		}

		~Storage()
		{
			log("Storage DESTROY CALLED! but i am died already....");
			SyncData();
		}

		public object this[string name]
		{
			get
			{
				if (properties.ContainsKey(name))
					return properties[name];

				if (!Configuration.mysqlEnabled)
					return string.Empty;

				string retrievedValue = string.Empty;
				retrievedValue = (string) Data.mysqlDatabase.Get(name);
				properties[name] = retrievedValue;

				return (retrievedValue != null && retrievedValue.Length > 0)
					? retrievedValue
					: string.Empty;
			}
			set
			{
				properties[name] = value;
				if (Configuration.memcahedEnabled)
					memcachedServer.Store(StoreMode.Set, name, value);
			}
		}

		public void SyncData()
		{
			if (properties.Count <= 0)
				return;

			debug($"storage: storing {properties.Count} key-value pairs.");
			foreach (KeyValuePair<string, object> o in properties)
			{
				if (Configuration.mysqlEnabled)
					Data.mysqlDatabase.Set(o.Key, o.Value);

				if (Configuration.memcahedEnabled)
					memcachedServer.Store(StoreMode.Set, o.Key, o.Value);
			}
		}

		public void InitMemcached()
		{
			MemcachedClientConfiguration config = new MemcachedClientConfiguration();
			IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Parse(Configuration.memcachedIP),
												   Configuration.memcachedPort);
			config.Servers.Add(ipEndpoint);
			config.Protocol = MemcachedProtocol.Text;
			memcachedServer = new MemcachedClient(config);

			if (memcachedServer != null)
				debug($"memcached up [0x{memcachedServer.GetHashCode()}]");
			else
				debug($"fail to init memcached: {memcachedServer.ToString()}");
		}
	}
}
