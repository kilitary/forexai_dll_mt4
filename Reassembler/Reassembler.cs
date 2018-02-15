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
using forexAI.Api;

namespace forexAI
{
    public class ParametersInput
    {
        string functionName;
        object[] arguments;

    }
    public static class Reassembler
    {
        public static List<ParametersInput> functionsList;
        public static Dictionary<string, FunctionsConfiguration> functionConfigurationInput;
        public static LivePrices prices = new LivePrices();
        public static int OutBegIdx;
        public static int OutNbElement;
        public static int OutIndex;


        public static void Build(string functionConfigurationString, int inputDimension)
        {
            log($"=> Reassembling input sequence ...");

            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
            functionConfigurationInput = JsonConvert.DeserializeObject<Dictionary<string, FunctionsConfiguration>>
                (functionConfigurationString, jsonSettings);

            log($"=> {functionConfigurationInput.Count} functions with {inputDimension} input dimension");

            object[] arguments;
            int fidx = 0;
            string reassembledFunctions = string.Empty;
            string paramName = String.Empty;
            string comment = string.Empty, paramComment = String.Empty;
            double paramValue = 0.0;

            foreach (var item in functionConfigurationInput)
            {
                string functionName = item.Key;
                FunctionsConfiguration conf = item.Value;

                log($" -> #{fidx++} {functionName}: {conf.parameters.parametersMap.Count} args");

                string stringOut = string.Empty;
                arguments = new object[conf.parameters.parametersMap.Count];
                int paramIndex = 0;
                string[] values = new string[4];
                int numData = inputDimension;
                int id;


                foreach (var param in conf.parameters.parametersMap)
                {
                    values = param.Split('|');
                    paramName = values[1];
                    int.TryParse(values[0], out id);

                    double.TryParse(values[2], out paramValue);
                    comment = values[3];
                    paramComment = string.Empty;
                    log($"processing [{paramName}] paramIndex {paramIndex} argLen {arguments.Length}");
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
                            log($"index={index}");
                            log($"arguments[index]={arguments[index]} ({arguments[index].GetType()}");
                            switch (index)
                            {
                                case 0:
                                    arguments[paramIndex] = prices.GetOpen(numData);
                                    break;
                                case 1:
                                    arguments[paramIndex] = prices.GetClose(numData);
                                    break;
                                case 2:
                                    arguments[paramIndex] = prices.GetHigh(numData);
                                    break;
                                case 3:
                                    arguments[paramIndex] = prices.GetLow(numData);
                                    break;
                            }

                            // debug($"real {paramName}[0]: " + ((double[])Arguments[ParamIndex])[0]);
                            break;
                        case "optInMaximum":
                            arguments[paramIndex] = 0.0;
                            break;
                        case "optInSlowD_MAType":
                        case "optInFastD_MAType":
                        case "optInSlowK_MAType":
                            arguments[paramIndex] = 0;

                            // debug($"{paramName} optMAtype=" + arguments[paramIndex]);
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
                            arguments[paramIndex] = prices.GetOpen(numData);
                            break;
                        case "inHigh":
                            arguments[paramIndex] = prices.GetHigh(numData);
                            break;
                        case "inLow":
                            arguments[paramIndex] = prices.GetLow(numData);
                            break;
                        case "inClose":
                            arguments[paramIndex] = prices.GetClose(numData);
                            break;
                        case "inVolume":
                            arguments[paramIndex] = prices.GetVolume(numData);
                            break;
                        case "outBegIdx":
                            arguments[paramIndex] = OutBegIdx;
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
                            debug($"nothing found for {paramName}");
                            break;
                    }
                    log($"arg[{paramIndex}] {arguments[paramIndex]}");


                    paramIndex++;
                }
                log($"dd={JsonConvert.SerializeObject(arguments)}");

                stringOut = $"     {paramName}={paramValue} {comment}";
                log(stringOut);
                reassembledFunctions += $"[{functionName}] ";
            }

            log($"=> Reassembling done: {reassembledFunctions}");
            return;
        }
    }
}
