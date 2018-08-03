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

		public string this[string name]
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

		public void Set(string name, object obj)
		{
			if (_config != null)
				_config[name] = (string) obj;// JsonConvert.SerializeObject(obj, Formatting.Indented);
		}

		public object Get(string name)
		{
			return _config == null ? "<nullconfig>" : _config[name];
		}

		public Config()
		{
			if (File.Exists(Configuration.configFilePath))
			{
				_config = new Dictionary<string, dynamic>();
				_config = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>
					(File.ReadAllText(Configuration.configFilePath));
				log($"Config()->load {_config.Count()} vars", "dev");
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
				log($"saving {data}", "dev");
				File.WriteAllText(Configuration.configFilePath, data);
			}
			catch (InvalidOperationException e)
			{
				log($"exception in save config: {e.Message}", "error");
			}
			return data == string.Empty ? -1 : data.Length;
		}

		~Config()
		{
			Save();
		}

		public object getDump()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(_config, Formatting.Indented);
		}

		internal void Clear()
		{
			_config.Clear();
		}
	}
}
