/*......................../´¯)........... 
.....................,/..../............ 
..................../..../ ............. 
............./´¯/' .../´¯/ ¯/\...... 
........../'/.../... ./... /..././¯\.... 
........('(....(.... (....(.. /'...).... 
.........\................. ..\/..../.... 
..........\......................./´..... 
............\................ ..(........*/
using System;
using System.Drawing;
using System.Reflection;
using System.Text;
using static forexAI.Logger;
using forexAI;

namespace forexAI
{
    class FunctionParameters
    {
        public int ParamIndex;
        public int NumData;
        public int OutBegIdx = 0;
        public int Offset;
        public StringBuilder parametersMap;
        public object[] Arguments;
        public int OutIndex;
        public int OutNbElement;
        public Prices prices;

        public FunctionParameters(MethodInfo methodInfo, int numdata, int offset)
        {
            
            NumData = numdata;
            Offset = offset;
            parametersMap = new StringBuilder();

            // DumpParams(methodInfo);
            Arguments = new object[methodInfo.GetParameters().Length];

            // debug($"function method {methodInfo.Name} offset {offset} numdata {NumData} randomSeed {randomSeed}");
            foreach (ParameterInfo param in methodInfo.GetParameters())
            {
                string paramComment = string.Empty;
                switch (param.Name)
                {
                    case "optInVFactor":
                        Arguments[ParamIndex] = 0;
                        break;
                    case "outMACDSignal":
                        Arguments[ParamIndex] = 0;
                        break;
                    case "outMACDHist":
                        Arguments[ParamIndex] = 0;
                        break;

                    case "outMin":
                        Arguments[ParamIndex] = 0;
                        break;
                    case "optInNbDevUp":
                        Arguments[ParamIndex] = 0.0;
                        break;
                    case "outMACD":
                        Arguments[ParamIndex] = 0.0;
                        break;
                    case "outLeadSine":
                        Arguments[ParamIndex] = 0.0;
                        break;
                    case "outSine":
                        Arguments[ParamIndex] = 0.0;
                        break;
                    case "optInMinPeriod":
                    case "optInMaxPeriod":
                    case "optInSignalPeriod":
                        Arguments[ParamIndex] = -999;
                        paramComment = $"MaTypeGen";

                        // debug($"{param.Name} optInSignalPeriod=" + arguments[paramIndex]);
                        break;
                    case "optInMAType":
                        Arguments[ParamIndex] = -999;
                        paramComment = $"MaTypeGen";

                        // debug($"{param.Name} optInMAType=" + arguments[paramIndex]);
                        break;
                    case "optInNbDev":
                        Arguments[ParamIndex] = 0.0;
                        break;
                    case "inReal0":
                    case "inReal1":
                    case "inReal":
                        var index = -999;//XRandom.next(3);
                        switch (index)
                        {
                            case 0:
                                Arguments[ParamIndex] = prices.GetOpen(NumData);
                                paramComment = $"%Open% {NumData}";
                                break;
                            case 1:
                                Arguments[ParamIndex] = prices.GetClose(numdata);
                                paramComment = $"%Close% {NumData}";
                                break;
                            case 2:
                                Arguments[ParamIndex] = prices.GetHigh(NumData);
                                paramComment = $"%High% {NumData}";
                                break;
                            case 3:
                                Arguments[ParamIndex] = prices.GetLow(NumData);
                                paramComment = $"%Low% {NumData}";
                                break;
                        }

                        // debug($"real {param.Name}[0]: " + ((double[])Arguments[ParamIndex])[0]);
                        break;
                    case "optInMaximum":
                        Arguments[ParamIndex] = 0.0;
                        break;
                    case "optInSlowD_MAType":
                    case "optInFastD_MAType":
                    case "optInSlowK_MAType":
                        Arguments[ParamIndex] = -999;
                        paramComment = $"MaTypeGen";

                        // debug($"{param.Name} optMAtype=" + arguments[paramIndex]);
                        break;
                    case "optInAccelerationShort":
                    case "optInAccelerationMaxShort":
                    case "optInAccelerationInitShort":
                    case "optInAccelerationMaxLong":
                    case "optInAccelerationLong":
                    case "optInAccelerationInitLong":
                    case "optInAcceleration":
                        Arguments[ParamIndex] = 0.0;
                        break;

                    case "optInOffsetOnReverse":
                        Arguments[ParamIndex] = 0;
                        break;
                    case "optInSlowK_Period":
                    case "optInFastK_Period":
                    case "optInSlowD_Period":
                    case "optInFastD_Period":
                        Arguments[ParamIndex] = -999;
                        paramComment = $"MaTypeGen";
                        break;
                    case "optInSlowPeriod":
                        Arguments[ParamIndex] = -999;
                        break;
                    case "optInFastPeriod":
                        Arguments[ParamIndex] = -999;
                        break;

                    case "optInTimePeriod":
                        Arguments[ParamIndex] = 2;
                        break;

                    case "optInTimePeriod1":
                    case "optInTimePeriod3":
                    case "optInTimePeriod2":
                        Arguments[ParamIndex] = -999;
                        break;
                    case "optInPenetration":
                        Arguments[ParamIndex] = 0;
                        break;
                    case "optInStartValue":
                        Arguments[ParamIndex] = 0;
                        break;
                    case "startIdx":
                        Arguments[ParamIndex] = 0;
                        break;
                    case "endIdx":
                        Arguments[ParamIndex] = NumData - 1;
                        break;
                    case "inOpen":
                        Arguments[ParamIndex] = prices.GetOpen(NumData);
                        paramComment = $"Open {NumData}";
                        break;
                    case "inHigh":
                        Arguments[ParamIndex] = prices.GetHigh(NumData);
                        paramComment = $"High {NumData}";
                        break;
                    case "inLow":
                        Arguments[ParamIndex] = prices.GetLow(NumData);
                        paramComment = $"Low {NumData}";
                        break;
                    case "inClose":
                        Arguments[ParamIndex] = prices.GetClose(NumData);
                        paramComment = $"Close {NumData}";
                        break;
                    case "inVolume":
                        Arguments[ParamIndex] = prices.GetVolume(NumData);
                        paramComment = $"Volume {NumData}";
                        break;
                    case "outBegIdx":
                        Arguments[ParamIndex] = this.OutBegIdx;
                        break;
                    case "outNBElement":
                        Arguments[ParamIndex] = OutNbElement;
                        OutNbElement = ParamIndex;
                        break;
                    case "outInteger":
                        Arguments[ParamIndex] = new int[NumData];
                        OutIndex = ParamIndex;
                        break;
                    case "outReal":
                        Arguments[ParamIndex] = new double[NumData];
                        OutIndex = ParamIndex;
                        break;
                    case "outAroonUp":
                        Arguments[ParamIndex] = new double[1000];
                        break;
                    case "outAroonDown":
                        Arguments[ParamIndex] = new double[1000];
                        break;
                    case "outSlowD":
                    case "outSlowK":
                    case "outFastD":
                    case "outFastK":
                        Arguments[ParamIndex] = new double[1000];
                        break;

                    default:
                        debug($"nothing found for {param.Name}");
                        break;
                }

                parametersMap.Append($"  arg{ParamIndex,2:0} {param.Name}: {Arguments[ParamIndex]} {paramComment}\r\n");
                ParamIndex++;
            }

            //DumpArguments();
        }
    }
}