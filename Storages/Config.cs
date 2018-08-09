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
		private readonly Dictionary<string, string> _configRepository = null;

		public void Inc(string logicName)
		{
			int current = 0;

			int.TryParse(this[logicName], out current);
			current++;
			this[logicName] = current.ToString();
		}

		public void Dec(string logicName)
		{
			int current = 0;

			int.TryParse(this[logicName], out current);
			current--;
			this[logicName] = current.ToString();
		}

		public bool IsEnabled(string logicName)
		{
			if(!Has(logicName))
				return false;

			return bool.Parse(this[logicName]);
		}

		public bool Toggle(string logicName)
		{
			this[logicName] = IsEnabled(logicName) ? "false" : "true";
			Save();
			return bool.Parse(this[logicName]);
		}

		public void Enable(string logicName)
		{
			this[logicName] = "true";
			Save();
		}

		public void Disable(string logicName)
		{
			this[logicName] = "false";
			Save();
		}

		public void Remove(string logicName)
		{
			if(_configRepository != null && logicName.Length > 0)
			{
				_configRepository.Remove(logicName);
				Save();
			}
		}

		public string this[string name]
		{
			get
			{
				if(!Has(name))
					return null;

				string value = string.Empty;

				if(_configRepository.TryGetValue(name, out value))
					return value;
				else
					return null;
			}
			set
			{
				if(_configRepository != null)
					_configRepository[name] = value;
				Save();
			}
		}

		public bool Has(string name)
		{
			if(_configRepository == null || !_configRepository.Keys.Contains(name))
				return false;

			return true;
		}

		public void Set(string name, object obj)
		{
			if(_configRepository != null)
				_configRepository[name] = (string) obj;// JsonConvert.SerializeObject(obj, Formatting.Indented);
			Save();
		}

		public object Get(string name, string def = null)
		{
			if(!Has(name))
				return def;

			return _configRepository?[name];
		}

		public Config()
		{
			if(File.Exists(Configuration.configFilePath))
			{
				_configRepository = new Dictionary<string, string>();
				_configRepository = JsonConvert.DeserializeObject<Dictionary<string, string>>
					(File.ReadAllText(Configuration.configFilePath));
				log($"Config()->load {_configRepository.Count()} vars", "App.full");
			}
		}

		public int Save()
		{
			string data = string.Empty;

			if(_configRepository == null)
				return -1;

			try
			{
				data = JsonConvert.SerializeObject(_configRepository, Formatting.Indented);
				log($"Config()->saving {data.Length} bytes '{data}' to {Configuration.configFilePath}", "App.full");
				File.WriteAllText(Configuration.configFilePath, data);
			}
			catch(InvalidOperationException e)
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
			return JsonConvert.SerializeObject(_configRepository, Formatting.Indented);
		}

		public void Clear()
		{
			if(_configRepository != null)
				_configRepository.Clear();
			Save();
		}
	}
}
