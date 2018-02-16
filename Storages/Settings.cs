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
    class Settings
    {
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

        public void Set(string name, object obj)
        {
            settings[name] = JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public object Get(string name)
        {
            return JsonConvert.DeserializeObject<object>(settings[name].ToString());
        }

        public Settings()
        {
            settings = JsonConvert.DeserializeObject<Dictionary<string, object>>
                (File.ReadAllText(Configuration.settingsPath));
        }

        public void Save()
        {
            File.WriteAllText(Configuration.settingsPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        ~Settings()
        {
            Save();
        }

    }
}
