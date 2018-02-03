// *    * *    *                   *    * ** *    *                   *    * *
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ ЕБАНЫЙ РОООООТ!
//┓┏┓┏┓┃＼○／ 
//┛┗┛┗┛┃ / /
//┓┏┓┏┓┃ノ
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┓┏┓┏┓┃＼○／ 
//┛┗┛┗┛┃ / /
//┓┏┓┏┓┃ノ
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃
using static forexAI.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace forexAI
{
    public static class Reassembler
    {
        public static void Compose(string functionsConfig, int inputDimension)
        {
            Dictionary<string, FunctionsConfiguration> functionsConfiguration;

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
            functionsConfiguration = JsonConvert.DeserializeObject<Dictionary<string, FunctionsConfiguration>>(functionsConfig, jsonSettings);

            int fidx = 0;
            foreach (var item in functionsConfiguration)
            {
                string functionName = item.Key;

                log($" -> func {fidx++,2:00} [{functionName}]");
                dump(item.Value, functionName);
            }

            return;
        }
    }
}
