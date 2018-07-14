using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forexAI
{
	public struct ExtendedMethodInfo
	{
		public string Name;
		public string AssemblyName;
		public string ClassName;
		public string Signature;
		public string Signature2;
		public object GenericArguments;
		public int MemberType;
	}

	public struct FunctionParameters
	{
		public List<string> parametersMap;
		public object Arguments;
		public int OutIndex;
		public int OutNbElement;
		public int ParamIndex;
		public int NumData;
		public int OutBegIdx;
		public int Offset;
	}

	public struct FunctionConfiguration
	{
		public FunctionParameters parameters;
		public ExtendedMethodInfo methodInfo;
	}
}
