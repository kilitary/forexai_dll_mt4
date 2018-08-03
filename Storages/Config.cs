﻿//......................../´¯)........... 
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
		private readonly Dictionary<string, string> _config = null;

		public bool IsEnabled(string logic)
		{
			if (!Has(logic))
				return false;

			return bool.Parse(this[logic]);
		}

		public void Toggle(string logic)
		{
			if(IsEnabled(logic))
				this[logic] = "false";
			else
				this[logic] = "true";
			Save();
		}

		public void Enable(string logic)
		{
			this[logic] = "true";
			Save();
		}

		public void Disable(string logic)
		{
			this[logic] = "false";
			Save();
		}

		public string this[string name]
		{
			get
			{
				if (_config == null || !_config.Keys.Contains(name))
					return null;

				string value;
				_config.TryGetValue(name, out value);

				return value;
			}
			set
			{
				if (_config != null)
					_config[name] = value;
				Save();
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
			Save();
		}

		public object Get(string name, string def = null)
		{
			if (!Has(name))
				return def;

			return _config?[name];
		}

		public Config()
		{
			if (File.Exists(Configuration.configFilePath))
			{
				_config = new Dictionary<string, string>();
				_config = JsonConvert.DeserializeObject<Dictionary<string, string>>
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
