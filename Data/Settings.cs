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
    class Settings
    {
        string settingsPath = @"d:\temp\forexAI\mt4forexai.cfg";
        Dictionary<string, object> settings = new Dictionary<string, object>();
        public object this[string name]
        {
            get
            {
                return (object) settings[name];
            }
            set
            {
                settings[name] = value.ToString();
            }
        }

        public Settings()
        {
            string data = File.ReadAllText(settingsPath);
            settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
        }

        public void Save()
        {
            string data = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(settingsPath, data);
        }

        ~Settings()
        {
            Save();
        }

    }
}
