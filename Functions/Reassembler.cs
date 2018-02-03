//┓┏┓┏┓┃ 
//┛┗┛┗┛┃
//┓┏┓┏┓┃ 
//┛┗┛┗┛┃ ЕБАНЫЙ РОООООТ!
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
    class ExMethodInfo
    {
        string Name;
        string AssemblyName;
        string ClassName;
        string Signature;
        string Signature2;
        int MemberType;
        object GenericArguments;
    }

    class FunctionParams
    {
        int ParamIndex;
        int NumData;
        int OutBegIdx;
        int Offset;
        object Arguments;
        int OutIndex;
        int OutNbElement;
    }

    class FunctionsConfiguration
    {
        Dictionary<string, FunctionParams> parameters;
        ExMethodInfo methodInfo;
    }

    class CombinedData
    {
        Dictionary<string, FunctionsConfiguration> functions;
    }

    class Reassembler
    {
        public Reassembler(string functionsConfig, int inputDimension)
        {
            CombinedData functionConfiguration;

            log($"functionsConfig:{functionsConfig}");

            functionConfiguration = JsonConvert.DeserializeObject<CombinedData>(functionsConfig);
            dump(functionConfiguration, "fc");

            return;
        }
    }
}
