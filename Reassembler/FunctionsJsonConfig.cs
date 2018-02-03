using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        int ParamIndex;
        public int NumData;
        int OutBegIdx;
        int Offset;
        public List<string> parametersMap;
        object Arguments;
        int OutIndex;
        int OutNbElement;
    }

    public class FunctionsConfiguration
    {
        public FunctionParams parameters;
        ExMethodInfo methodInfo;
    }
}
