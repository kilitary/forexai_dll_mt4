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
	public class Config
	{
		private readonly Dictionary<string, dynamic> _config = null;

		public dynamic this[string name]
		{
			get
			{
				if (_config == null || !_config.Keys.Contains(name))
					return null;

				dynamic value;
				_config.TryGetValue(name, out value);

				return value;
			}
			set
			{
				if (_config != null)
					_config[name] = value;
			}
		}

		public bool Has(string name)
		{
			if (_config == null || !_config.Keys.Contains(name))
				return false;

			return true;
		}

		public void Set(string name, object obj)
		{
			if (_config != null)
				_config[name] = (string) obj;// JsonConvert.SerializeObject(obj, Formatting.Indented);
		}

		public object Get(string name, dynamic def = null)
		{
			if (!Has(name))
				return def;

			return _config?[name];
		}

		public Config()
		{
			if (File.Exists(Configuration.configFilePath))
			{
				_config = new Dictionary<string, dynamic>();
				_config = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>
					(File.ReadAllText(Configuration.configFilePath));
				log($"Config()->load {_config.Count()} vars", "App.full");
			}
		}

		public int Save()
		{
			string data = string.Empty;

			if (_config == null)
				return -1;

			try
			{
				data = JsonConvert.SerializeObject(_config, Formatting.Indented);
				log($"Config()->saving {data.Length} bytes '{data}' to {Configuration.configFilePath}", "App.full");
				File.WriteAllText(Configuration.configFilePath, data);
			}
			catch (InvalidOperationException e)
			{
				log($"exception in save config: {e.Message}", "error");
			}

			return data.Length;
		}

		~Config()
		{
			Save();
		}

		public string DumpString()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(_config, Formatting.Indented);
		}

		public void Clear()
		{
			if (_config != null)
				_config.Clear();
		}
	}
}
