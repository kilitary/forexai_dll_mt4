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
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;
using NQuotes;
using TicTacTec.TA.Library;
using FANNCSharp.Double;

namespace forexAI
{
    public class ParametersInput
    {
        string functionName;
        object[] arguments;

    }
    public static class Reassembler
    {
        static List<ParametersInput> functionsList;
        static LivePrices prices = new LivePrices();
        static Dictionary<string, FunctionsConfiguration> functionConfigurationInput;
        static int OutBegIdx;
        static int OutNbElement = -1;
        private static int pOutNbElement;
        static int OutIndex = -1;
        private static int typeOut;
        static string reassembledFunctions = string.Empty;
        static string paramName = String.Empty;
        static string comment = string.Empty, paramComment = String.Empty;
        static double paramValue = 0.0;
        static string functionName = string.Empty;
        static int nOutBegIdx;
        static double[] resultDataDouble = { };
        static int[] resultDataInt = { };
        static double[] entireSet = { };
        static Core.RetCode ret;
        private static bool failedReassemble;
        static int sourceInputDimension = 0;

        public static double[] Build(string functionConfigurationString, int inputDimension,
            IMqlArray<double> Open, IMqlArray<double> Close, IMqlArray<double> High,
            IMqlArray<double> Low, IMqlArray<double> Volume, int Bars, NeuralNet forexNetwork)
        {
            log($"=> Reassembling input sequence ...");

            entireSet = new double[] { };
            failedReassemble = false;
            sourceInputDimension = inputDimension;

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
            functionConfigurationInput = DeserializeObject<Dictionary<string, FunctionsConfiguration>>
                (functionConfigurationString, jsonSettings);

            log($"=> {functionConfigurationInput.Count} functions with {inputDimension} input dimension");

            object[] arguments = new object[20];
            int fidx = 0;

            foreach (var item in functionConfigurationInput)
            {
                functionName = item.Key;
                FunctionsConfiguration conf = item.Value;

                //  log($" ==> #{fidx++} {functionName}: {conf.parameters.parametersMap.Count} args");

                string stringOut = string.Empty;
                arguments = new object[conf.parameters.parametersMap.Count];
                int paramIndex = 0;
                string[] values = new string[4];
                int numData = inputDimension;
                int id;
                //  log($"    [params={SerializeObject(conf.parameters.parametersMap, Formatting.Indented)}]");

                foreach (var param in conf.parameters.parametersMap)
                {
                    values = param.Split('|');
                    paramName = values[1];

                    int.TryParse(values[0], out id);
                    double.TryParse(values[2], out paramValue);

                    if (paramName == "endIdx" && id == 1)
                    {
                        //inputDimension = numData = (int) paramValue + 1;
                        //    log($"=> calculated {(int) paramValue + 1} input dimension");
                    }

                    comment = values[3];
                    paramComment = string.Empty;
                    //  log($"processing [{paramName}] paramIndex {paramIndex} argLen {arguments.Length}");
                    switch (paramName)
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
                            arguments[paramIndex] = 0;
                            break;
                        case "optInMAType":
                            arguments[paramIndex] = 0;
                            break;
                        case "optInNbDev":
                            arguments[paramIndex] = 0.0;
                            break;
                        case "inReal0":
                        case "inReal1":
                        case "inReal":
                            var index = YRandom.Next(3);
                            switch (index)
                            {
                                case 0:
                                    arguments[paramIndex] = prices.GetOpen(numData, Bars, Open);
                                    break;
                                case 1:
                                    arguments[paramIndex] = prices.GetClose(numData, Bars, Close);
                                    break;
                                case 2:
                                    arguments[paramIndex] = prices.GetHigh(numData, Bars, High);
                                    break;
                                case 3:
                                    arguments[paramIndex] = prices.GetLow(numData, Bars, Low);
                                    break;
                            }

                            break;
                        case "optInMaximum":
                            arguments[paramIndex] = 0.0;
                            break;
                        case "optInSlowD_MAType":
                        case "optInFastD_MAType":
                        case "optInSlowK_MAType":
                            arguments[paramIndex] = 0;
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
                            arguments[paramIndex] = 0;
                            break;
                        case "optInSlowPeriod":
                            arguments[paramIndex] = 0;
                            break;
                        case "optInFastPeriod":
                            arguments[paramIndex] = 0;
                            break;

                        case "optInTimePeriod":
                        case "optInTimePeriod1":
                        case "optInTimePeriod3":
                        case "optInTimePeriod2":
                            arguments[paramIndex] = 2;
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
                            arguments[paramIndex] = prices.GetOpen(numData, Bars, Open);
                            break;
                        case "inHigh":
                            arguments[paramIndex] = prices.GetHigh(numData, Bars, High);
                            break;
                        case "inLow":
                            arguments[paramIndex] = prices.GetLow(numData, Bars, Low);
                            break;
                        case "inClose":
                            arguments[paramIndex] = prices.GetClose(numData, Bars, Close);
                            break;
                        case "inVolume":
                            arguments[paramIndex] = prices.GetVolume(numData, Bars, Volume);
                            break;
                        case "outBegIdx":
                            arguments[paramIndex] = OutBegIdx;
                            nOutBegIdx = paramIndex;
                            break;
                        case "outNBElement":
                            arguments[paramIndex] = OutNbElement;
                            OutNbElement = paramIndex;
                            pOutNbElement = paramIndex;
                            break;
                        case "outInteger":
                            arguments[paramIndex] = new int[numData];
                            OutIndex = paramIndex;
                            typeOut = 0;
                            //    log($"outIndexI={OutIndex}");
                            break;
                        case "outReal":
                            arguments[paramIndex] = new double[numData];
                            OutIndex = paramIndex;
                            typeOut = 1;
                            //   log($"outIndexR={OutIndex}");
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
                            error($"nothing found for {paramName}");
                            break;
                    }
                    paramIndex++;
                }

                //  log($"  ==> arguments={SerializeObject(arguments)}");

                reassembledFunctions += $"[{functionName}] ";

                arguments[OutIndex] = new double[inputDimension];

                //                log($"    => getting method [{functionName}]:args={conf.parameters.parametersMap.Count} from TICTAC ...");
                Type[] functionTypes = new Type[conf.parameters.parametersMap.Count];
                int idx = 0;
                foreach (var arg in arguments)
                {
                    if (arg.GetType().IsByRef || arg.GetType().IsMarshalByRef)
                        functionTypes[idx] = arg.GetType().MakeByRefType();
                    else
                        functionTypes[idx] = arg.GetType();

                    if (OutNbElement == idx)
                        functionTypes[idx] = arg.GetType().MakeByRefType();

                    if (nOutBegIdx == idx)
                        functionTypes[idx] = arg.GetType().MakeByRefType();
                    idx++;
                }

                MethodInfo mi = typeof(Core).GetMethod(functionName, functionTypes);
                if (mi == null)
                {
                    error($"fail to load method [{functionName}] from TICTAC");
                    failedReassemble = true;
                }
                else
                {
                    ret = (Core.RetCode) mi.Invoke(null, arguments);

                    if (typeOut == 0)
                    {
                        resultDataInt = (int[]) arguments[OutIndex];
                        //     log($"    => resultDataInt={SerializeObject(resultDataInt)}");
                        int idxS = 0;
                        Array.Resize<double>(ref resultDataDouble, resultDataInt.Length);
                        foreach (int i in resultDataInt)
                            resultDataDouble[idxS++] = i;
                    }
                    else
                    {
                        resultDataDouble = (double[]) arguments[OutIndex];
                        //       log($"    => resultDataDouble={SerializeObject(resultDataDouble)}");
                    }
                    //  log($"    => mi={mi.Name} ret={ret} resultDataDouble={resultDataDouble.Length} resultDataInt={resultDataInt.Length}");

                    int prevLen = entireSet.Length;
                    Array.Resize<double>(ref entireSet, entireSet.Length + resultDataDouble.Length);
                    Array.Copy(resultDataDouble, 0, entireSet, prevLen, resultDataDouble.Length);
                }
            }

            log($"    =>  ret={ret} entireset={SerializeObject(entireSet)}");

            if (failedReassemble || forexNetwork.InputCount != entireSet.Length)
            {
                error($"failed to reassemble: diff in input count of network is {Math.Abs(entireSet.Length - forexNetwork.InputCount)}");
                return new double[] { };
            }

           
            log($"=> Reassembling done: {reassembledFunctions} OutputLength={entireSet.Length} inputDimension={inputDimension}" +
                $" sourceInputDimension={sourceInputDimension}");

            return entireSet;
        }
    }
}
