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
        public int paramIndex;
        public int numData;
        public int OutBegIdx = 0;
        public int Offset;
        public StringBuilder parametersMap;
        public object[] arguments;
        public int OutIndex;
        public int OutNbElement;
        public Prices prices;

        public FunctionParameters(MethodInfo methodInfo, int numdata, int offset)
        {
            
            numData = numdata;
            Offset = offset;
            parametersMap = new StringBuilder();

            // DumpParams(methodInfo);
            arguments = new object[methodInfo.GetParameters().Length];

            // debug($"function method {methodInfo.Name} offset {offset} numdata {NumData} randomSeed {randomSeed}");
            // TODO: использовать все opt* парамсы
            foreach (ParameterInfo param in methodInfo.GetParameters())
            {
                string paramComment = string.Empty;
                switch (param.Name)
                {
                    case "optInVFactor":
                        arguments[paramIndex] = 0;
                        break;
                    case "outMACDSignal":
                        arguments[paramIndex] = 0;
                        break;
                    case "outMACDHist":
                        arguments[paramIndex] = 0;
                        break;

                    case "outMin":
                        arguments[paramIndex] = 0;
                        break;
                    case "optInNbDevUp":
                        arguments[paramIndex] = 0.0;
                        break;
                    case "outMACD":
                        arguments[paramIndex] = 0.0;
                        break;
                    case "outLeadSine":
                        arguments[paramIndex] = 0.0;
                        break;
                    case "outSine":
                        arguments[paramIndex] = 0.0;
                        break;
                    case "optInMinPeriod":
                    case "optInMaxPeriod":
                    case "optInSignalPeriod":
                        arguments[paramIndex] = -999;
                        paramComment = $"MaTypeGen";

                        // debug($"{param.Name} optInSignalPeriod=" + arguments[paramIndex]);
                        break;
                    case "optInMAType":
                        arguments[paramIndex] = -999;
                        paramComment = $"MaTypeGen";

                        // debug($"{param.Name} optInMAType=" + arguments[paramIndex]);
                        break;
                    case "optInNbDev":
                        arguments[paramIndex] = 0.0;
                        break;
                    case "inReal0":
                    case "inReal1":
                    case "inReal":
                        var index = -999;//XRandom.next(3);
                        switch (index)
                        {
                            case 0:
                                arguments[paramIndex] = prices.GetOpen(numData);
                                paramComment = $"%Open% {numData}";
                                break;
                            case 1:
                                arguments[paramIndex] = prices.GetClose(numdata);
                                paramComment = $"%Close% {numData}";
                                break;
                            case 2:
                                arguments[paramIndex] = prices.GetHigh(numData);
                                paramComment = $"%High% {numData}";
                                break;
                            case 3:
                                arguments[paramIndex] = prices.GetLow(numData);
                                paramComment = $"%Low% {numData}";
                                break;
                        }

                        // debug($"real {param.Name}[0]: " + ((double[])Arguments[ParamIndex])[0]);
                        break;
                    case "optInMaximum":
                        arguments[paramIndex] = 0.0;
                        break;
                    case "optInSlowD_MAType":
                    case "optInFastD_MAType":
                    case "optInSlowK_MAType":
                        arguments[paramIndex] = -999;
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
                        arguments[paramIndex] = 0.0;
                        break;

                    case "optInOffsetOnReverse":
                        arguments[paramIndex] = 0;
                        break;
                    case "optInSlowK_Period":
                    case "optInFastK_Period":
                    case "optInSlowD_Period":
                    case "optInFastD_Period":
                        arguments[paramIndex] = -999;
                        paramComment = $"MaTypeGen";
                        break;
                    case "optInSlowPeriod":
                        arguments[paramIndex] = -999;
                        break;
                    case "optInFastPeriod":
                        arguments[paramIndex] = -999;
                        break;

                    case "optInTimePeriod":
                        arguments[paramIndex] = 2;
                        break;

                    case "optInTimePeriod1":
                    case "optInTimePeriod3":
                    case "optInTimePeriod2":
                        arguments[paramIndex] = -999;
                        break;
                    case "optInPenetration":
                        arguments[paramIndex] = 0;
                        break;
                    case "optInStartValue":
                        arguments[paramIndex] = 0;
                        break;
                    case "startIdx":
                        arguments[paramIndex] = 0;
                        break;
                    case "endIdx":
                        arguments[paramIndex] = numData - 1;
                        break;
                    case "inOpen":
                        arguments[paramIndex] = prices.GetOpen(numData);
                        paramComment = $"Open {numData}";
                        break;
                    case "inHigh":
                        arguments[paramIndex] = prices.GetHigh(numData);
                        paramComment = $"High {numData}";
                        break;
                    case "inLow":
                        arguments[paramIndex] = prices.GetLow(numData);
                        paramComment = $"Low {numData}";
                        break;
                    case "inClose":
                        arguments[paramIndex] = prices.GetClose(numData);
                        paramComment = $"Close {numData}";
                        break;
                    case "inVolume":
                        arguments[paramIndex] = prices.GetVolume(numData);
                        paramComment = $"Volume {numData}";
                        break;
                    case "outBegIdx":
                        arguments[paramIndex] = this.OutBegIdx;
                        break;
                    case "outNBElement":
                        arguments[paramIndex] = OutNbElement;
                        OutNbElement = paramIndex;
                        break;
                    case "outInteger":
                        arguments[paramIndex] = new int[numData];
                        OutIndex = paramIndex;
                        break;
                    case "outReal":
                        arguments[paramIndex] = new double[numData];
                        OutIndex = paramIndex;
                        break;
                    case "outAroonUp":
                        arguments[paramIndex] = new double[1000];
                        break;
                    case "outAroonDown":
                        arguments[paramIndex] = new double[1000];
                        break;
                    case "outSlowD":
                    case "outSlowK":
                    case "outFastD":
                    case "outFastK":
                        arguments[paramIndex] = new double[1000];
                        break;

                    default:
                        debug($"nothing found for {param.Name}");
                        break;
                }

                parametersMap.Append($"  arg{paramIndex,2:0} {param.Name}: {arguments[paramIndex]} {paramComment}\r\n");
                paramIndex++;
            }

            //DumpArguments();
        }
    }
}