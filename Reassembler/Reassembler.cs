// *    * *    *                   *    * ** *    *                   *    * *
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃
//┓┏┓┏.  
//┛┗┛┗┛┃
//┓┏┓┏┓┃＼○／ 
//┛┗┛┗┛┃ / /
//┓┏┓┏┓┃ノ
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃
//┛┗┛┗┛┃ 
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃  ЕБАНЫЙ РОООООТ!
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
            log($"=> Reassembling input sequence ...");

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

                string stringOut = string.Empty;

                foreach (var param in conf.parameters.parametersMap)
                {
                    string[] values = new string[4];

                    values = param.Split('|');
                    int id;
                    int.TryParse(values[0], out id);
                    string paramName = values[1];
                    double paramValue;
                    double.TryParse(values[2], out paramValue);
                    string comment = values[3];
                    stringOut += $" [{paramName}={paramValue} {comment}]";
                }
                log(stringOut);
            }

            log($"=> Reassembling done.");
            return;
        }
    }
}
