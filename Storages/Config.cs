//......................../´¯)........... 
//.....................,/..../............ 
//..................../..../ ............. 
//............./´¯/' .../´¯/ ¯/\...... 
//........../'/.../... ./... /..././¯\.... 
//........('(....(.... (....(.. /'...).... 
//.........\................. ..\/..../.... 
//..........\......................./´..... 
//............\................ ..(........

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static forexAI.Logger;

namespace forexAI
{
	class Config
	{
		private readonly Dictionary<string, object> config = new Dictionary<string, object>();

		public object this[string name]
		{
			get
			{
				return (object) config[name];
			}
			set
			{
				config[name] = value;
			}
		}

		public void Set(string name, object obj)
		{
			config[name] = obj;// JsonConvert.SerializeObject(obj, Formatting.Indented);
		}

		public object Get(string name)
		{
			return config[name];// JsonConvert.DeserializeObject<object>(settings[name].ToString());
		}

		public Config()
		{
			if (File.Exists(Configuration.configFilePath))
				config = JsonConvert.DeserializeObject<Dictionary<string, object>>
					(File.ReadAllText(Configuration.configFilePath));
		}

		public void Save()
		{
			log($"saving {JsonConvert.SerializeObject(config, Formatting.Indented)}", "dev");
			try
			{
				File.WriteAllText(Configuration.configFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));
			}
			catch(InvalidOperationException e)
			{
				error($"exception in save config: {e.Message}");
			}
		}

		~Config()
		{
			Save();
		}

	}
}
