// *    * *    *                   *    * ** *    *                   *    * *
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃
//┓┏┓┏.  
//┛┗┛┗┛┃ ЕБАНЫЙ РОООООТ!
//┓┏┓┏┓┃＼○／ 
//┛┗┛┗┛┃ / /
//┓┏┓┏┓┃ノ
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃＼○／ 
//┛┗┛┗┛┃ / /
//┓┏┓┏┓┃ノ
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
        static Dictionary<string, FunctionsConfiguration> functionConfiguration;
        public static void Build(string functionConfigurationString, int inputDimension)
        {
            log($"=> Reassembling input sequence...");

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
            functionConfiguration = JsonConvert.DeserializeObject<Dictionary<string, FunctionsConfiguration>>
                (functionConfigurationString, jsonSettings);

            log($"=> {functionConfiguration.Count} functions with {inputDimension} input dimension");

            int fidx = 0;
            foreach (var item in functionConfiguration)
            {
                string functionName = item.Key;
                FunctionsConfiguration conf = item.Value;

                log($" -> #{fidx++} {functionName}()");

                foreach (var param in conf.parameters.parametersMap)
                {
                    log($"     {param}");
                }
            }

            log($"=> Reassembling done.");
            return;
        }
    }
}
