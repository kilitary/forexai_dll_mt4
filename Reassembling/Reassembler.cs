// *    * *    *                   *    * *
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Logger;
using Newtonsoft.Json;

namespace forexAI
{
    public class ExMethodInfo
    {
        public string Name;
        public string AssemblyName;
        public string ClassName;
        public string Signature;
        public string Signature2;
        public int MemberType;
        public object GenericArguments;
    }

    public class FunctionParams
    {
        public int ParamIndex;
        public int NumData;
        public int OutBegIdx;
        public int Offset;
        public List<string> parametersMap;
        // public object Arguments;
        public int OutIndex;
        public int OutNbElement;
    }

    public class FunctionsConfiguration
    {
        public FunctionParams parameters;
        public ExMethodInfo methodInfo;
    }

    class Reassembler
    {
        public Reassembler(string functionsConfig, int inputDimension)
        {
            Dictionary<string, FunctionsConfiguration> functionsConfiguration;

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
            functionsConfiguration = JsonConvert.DeserializeObject<Dictionary<string, FunctionsConfiguration>>(functionsConfig, jsonSettings);

            int fidx = 0;
            foreach (var item in functionsConfiguration)
            {
                string functionName = item.Key;

                log($"#{fidx++} [{functionName}]");
                dump(item.Value);
            }

            return;
        }
    }
}
