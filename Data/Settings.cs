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
            settings = JsonConvert.DeserializeObject<Dictionary<string, object>>
                (File.ReadAllText(settingsPath));
        }

        public void Save()
        {
            File.WriteAllText(settingsPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        ~Settings()
        {
            Save();
        }

    }
}
