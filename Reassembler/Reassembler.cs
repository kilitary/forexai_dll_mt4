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

//......................../´¯)........... 
//.....................,/..../............ 
//..................../..../ ............. 
//............./´¯/' .../´¯/ ¯/\...... 
//........../'/.../... ./... /..././¯\.... 
//........('(....(.... (....(.. /'...).... 
//.........\................. ..\/..../.... 
//..........\......................./´..... 
//............\................ ..(........

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FANNCSharp.Double;
using Newtonsoft.Json;
using NQuotes;
using TicTacTec.TA.Library;
using static forexAI.Logger;
using static Newtonsoft.Json.JsonConvert;

namespace forexAI
{
  public static class Reassembler
  {
    static Dictionary<string, FunctionConfiguration> functionsConfiguration;
    static Core.RetCode ret = Core.RetCode.UnknownErr;
    static ChartPrices prices = new ChartPrices();
    static Random random = new Random();
    static int functionConfigurationHashCode = 0;
    static string functionName = string.Empty;
    static string paramName = string.Empty;
    static string comment = string.Empty;
    static string functionsNamesList;
    static double paramValue = 0.0;
    static double[] resultDataDouble = null;
    static double[] fullInputSet = null;
    static object[] functionArguments = null;
    static bool failedReassemble = false;
    static bool reassemblyStage = true;
    static int[] resultDataInt = null;
    static int currentFunctionIndex = 0;
    static int outBegIdx = 0;
    static int outNumberElement = -1;
    static int pOutNbElement = 0;
    static int outIndex = -1;
    static int outTypeDoubleOrInt = 0;
    static int nOutStartIdx = 0;
    static int startIdx = 0;
    static int arrayIndex = 0;
    static int setNextArrayIndex = 0;
    static TrainingData trainData = null;
    static TrainingData trainDataOriginal = null;

    public static (int, double[]) Execute(string functionConfigurationString, int inputDimension, NeuralNet neuralNetwork, bool reassemblingCompletedOverride)
    {
      reassemblyStage = reassemblingCompletedOverride;

      functionsNamesList = string.Empty;

      if(reassemblyStage)
        log($"=> Reassembling input sequence ...");

      setNextArrayIndex = 0;
      currentFunctionIndex = 0;

      if(failedReassemble)
        reassemblyStage = true;

      failedReassemble = false;

      if(functionConfigurationHashCode != functionConfigurationString.GetHashCode())
      {
        log($"hashOfFunctionConfiguration ({functionConfigurationHashCode}) not match content, deserializing {functionConfigurationString.Length} bytes ...");


        functionsConfiguration = DeserializeObject<Dictionary<string, FunctionConfiguration>>
           (functionConfigurationString, new JsonSerializerSettings
           {
             MetadataPropertyHandling = MetadataPropertyHandling.Ignore
           });
        functionConfigurationHashCode = functionConfigurationString.GetHashCode();
        consolelog($"hash of configuration: {functionConfigurationHashCode.ToString("x")}");
      }

      if(reassemblyStage)
        log($"=> {functionsConfiguration.Count} functions with {inputDimension} input dimension");

      if(fullInputSet == null || fullInputSet.Length != neuralNetwork.InputCount)
      {
        Array.Resize<double>(ref fullInputSet, (int) neuralNetwork.InputCount);
        Helpers.ZeroArray(fullInputSet);
      }

      if(reassemblyStage)
        log($"fullInputSet.Length = {fullInputSet.Length}");

      foreach(var function in functionsConfiguration)
      {
        string stringOut = string.Empty;
        string[] values = new string[4];
        int paramIndex = 0;
        int numFunctionDimension = inputDimension;
        int internalIndex;

        functionName = function.Key;
        FunctionConfiguration functionConfiguration = function.Value;

        functionArguments = new object[functionConfiguration.parameters.parametersMap.Count];

        foreach(var param in functionConfiguration.parameters.parametersMap)
        {
          values = param.Split('|');
          paramName = values[1];

          int.TryParse(values[0], out internalIndex);
          comment = values[3];
          double.TryParse(values[2], out paramValue);

          switch(paramName)
          {
            case "optInVFactor":
              functionArguments[paramIndex] = 0;
              break;
            case "outMACDSignal":
              functionArguments[paramIndex] = 0;
              break;
            case "outMACDHist":
              functionArguments[paramIndex] = 0;
              break;

            case "outMin":
              functionArguments[paramIndex] = 0;
              break;
            case "optInNbDevUp":
              functionArguments[paramIndex] = 0.0;
              break;
            case "outMACD":
              functionArguments[paramIndex] = 0.0;
              break;
            case "outLeadSine":
              functionArguments[paramIndex] = 0.0;
              break;
            case "outSine":
              functionArguments[paramIndex] = 0.0;
              break;
            case "optInMinPeriod":
            case "optInMaxPeriod":
            case "optInSignalPeriod":
              functionArguments[paramIndex] = 0;
              break;
            case "optInMAType":
              functionArguments[paramIndex] = 0;
              break;
            case "optInNbDev":
              functionArguments[paramIndex] = 0.0;
              break;
            case "inReal0":
            case "inReal1":
            case "inReal":

              if(comment.StartsWith("High"))
              {
                arrayIndex = 2;
              }

              if(comment.StartsWith("Open"))
              {
                arrayIndex = 0;
              }

              if(comment.StartsWith("Close"))
              {
                arrayIndex = 1;
              }

              if(comment.StartsWith("Low"))
              {
                arrayIndex = 3;
              }

              switch(arrayIndex)
              {
                case 0:
                  functionArguments[paramIndex] = prices.GetOpen(numFunctionDimension);
                  break;
                case 1:
                  functionArguments[paramIndex] = prices.GetClose(numFunctionDimension);
                  break;
                case 2:
                  functionArguments[paramIndex] = prices.GetHigh(numFunctionDimension);
                  break;
                case 3:
                  functionArguments[paramIndex] = prices.GetLow(numFunctionDimension);
                  break;
              }

              break;
            case "optInMaximum":
              functionArguments[paramIndex] = 0.0;
              break;
            case "optInSlowD_MAType":
            case "optInFastD_MAType":
            case "optInSlowK_MAType":
              functionArguments[paramIndex] = 0;
              break;
            case "optInAccelerationShort":
            case "optInAccelerationMaxShort":
            case "optInAccelerationInitShort":
            case "optInAccelerationMaxLong":
            case "optInAccelerationLong":
            case "optInAccelerationInitLong":
            case "optInAcceleration":
              functionArguments[paramIndex] = 0.0;
              break;

            case "optInOffsetOnReverse":
              functionArguments[paramIndex] = 0;
              break;
            case "optInSlowK_Period":
            case "optInFastK_Period":
            case "optInSlowD_Period":
            case "optInFastD_Period":
              functionArguments[paramIndex] = 0;
              break;
            case "optInSlowPeriod":
              functionArguments[paramIndex] = 0;
              break;
            case "optInFastPeriod":
              functionArguments[paramIndex] = 0;
              break;

            case "optInTimePeriod":
            case "optInTimePeriod1":
            case "optInTimePeriod3":
            case "optInTimePeriod2":
              functionArguments[paramIndex] = 2;
              break;
            case "optInPenetration":
              functionArguments[paramIndex] = 0;
              break;
            case "optInStartValue":
              functionArguments[paramIndex] = 0;
              break;
            case "startIdx":
              functionArguments[paramIndex] = 0;
              break;
            case "endIdx":
              functionArguments[paramIndex] = numFunctionDimension - 1;
              break;
            case "inOpen":
              functionArguments[paramIndex] = prices.GetOpen(numFunctionDimension);
              break;
            case "inHigh":
              functionArguments[paramIndex] = prices.GetHigh(numFunctionDimension);
              break;
            case "inLow":
              functionArguments[paramIndex] = prices.GetLow(numFunctionDimension);
              break;
            case "inClose":
              functionArguments[paramIndex] = prices.GetClose(numFunctionDimension);
              break;
            case "inVolume":
              functionArguments[paramIndex] = prices.GetVolume(numFunctionDimension);
              break;
            case "outBegIdx":
              functionArguments[paramIndex] = outBegIdx;
              nOutStartIdx = paramIndex;
              break;
            case "outNBElement":
              functionArguments[paramIndex] = outNumberElement;
              outNumberElement = pOutNbElement = paramIndex;
              break;
            case "outInteger":
              functionArguments[paramIndex] = new int[numFunctionDimension];
              outIndex = paramIndex;
              outTypeDoubleOrInt = 0;
              break;
            case "outReal":
              functionArguments[paramIndex] = new double[numFunctionDimension];
              outIndex = paramIndex;
              outTypeDoubleOrInt = 1;
              break;
            case "outAroonUp":
              functionArguments[paramIndex] = new double[111];
              break;
            case "outAroonDown":
              functionArguments[paramIndex] = new double[111];
              break;
            case "outSlowD":
            case "outSlowK":
            case "outFastD":
            case "outFastK":
              functionArguments[paramIndex] = new double[111];
              break;

            default:
              error($"nothing found for {paramName}");
              break;
          }
          paramIndex++;
        }

        if(reassemblyStage)
        {
          log($"=> {functionName} arguments({functionArguments.Length})={SerializeObject(functionArguments)}");
        }

        functionArguments[outIndex] = new double[inputDimension];

        Type[] functionTypes = new Type[functionConfiguration.parameters.parametersMap.Count];
        int idx = 0;
        foreach(object arg in functionArguments)
        {
          if(arg.GetType().IsByRef || arg.GetType().IsMarshalByRef)
          {
            functionTypes[idx] = arg.GetType().MakeByRefType();
          }
          else
          {
            functionTypes[idx] = arg.GetType();
          }

          if(outNumberElement == idx)
          {
            functionTypes[idx] = arg.GetType().MakeByRefType();
          }

          if(nOutStartIdx == idx)
          {
            functionTypes[idx] = arg.GetType().MakeByRefType();
          }

          idx++;
        }

        MethodInfo FunctionPointer = typeof(TicTacTec.TA.Library.Core).GetMethod(functionName, functionTypes);
        if(FunctionPointer == null)
        {
          error($"fail to load method [{functionName}] from TICTAC");
          failedReassemble = true;
        }
        else
        {
          ret = (Core.RetCode) FunctionPointer.Invoke(null, functionArguments);
          if(outTypeDoubleOrInt == 0)
          {
            resultDataInt = (int[]) functionArguments[outIndex];
            Array.Resize<double>(ref resultDataDouble, (int) functionArguments[outNumberElement]);
            Array.Copy(resultDataInt, resultDataDouble, (int) functionArguments[outNumberElement]);

            for(int i = 0; i < (int) functionArguments[outNumberElement]; i++)
            {
              if(resultDataDouble[i] == 0.0 && i == 0 && reassemblyStage)
                warning($"fucking function {functionName} starts with zero");

              if(resultDataDouble[i] == 0.0 && i == outNumberElement - 1 && reassemblyStage)
                warning($"fucking function {functionName} ends with zero");
            }
          }
          else
          {
            resultDataDouble = (double[]) functionArguments[outIndex];
            for(int i = 0; i < (int) functionArguments[outNumberElement]; i++)
            {
              if(resultDataDouble[i] == 0.0 && i == 0 && reassemblyStage)
              {
                warning($"fucking function {functionName} starts with zero");
              }
            }
          }

          startIdx = (int) functionArguments[nOutStartIdx];
          if(reassemblyStage && startIdx != 0)
          {
            warning($"# {functionName}: startIdx = {startIdx} (outNumberElement={functionArguments[outNumberElement]}, outBegIdx={outBegIdx})");
            dump(resultDataDouble, functionName, "warning");
          }

          if(reassemblyStage)
          {
            log($"=> {functionName}({resultDataDouble.Length}): resultDataDouble={SerializeObject(resultDataDouble)}");
          }

          Array.Copy(resultDataDouble, startIdx, fullInputSet, setNextArrayIndex, resultDataDouble.Length - startIdx);

          functionsNamesList += (functionsNamesList.Length > 0 ? "+" : "") +
             $"[{function.Key}[{resultDataDouble.Length - startIdx}/{numFunctionDimension}]";
        }
        currentFunctionIndex++;
        setNextArrayIndex += resultDataDouble.Length - startIdx;

        //File.WriteAllText($"{Configuration.rootDirectory}\\{function.Key}.dat", SerializeObject(resultDataDouble));
      }

      if(reassemblyStage && fullInputSet != null && fullInputSet.Length > 0)
      {
        log($"ret={ret} entireset={SerializeObject(fullInputSet)}");
      }

      if(neuralNetwork.InputCount != fullInputSet.Length)
      {
        error($"=> reassembler FAILED to reassemble input sequence: diff in input count of network is " +
           $"{Math.Abs(fullInputSet.Length - neuralNetwork.InputCount)}");
        reassemblyStage = false;
        failedReassemble = true;
        return (0, null);
      }

      if(reassemblyStage)
      {
        log($"=> Reassembling [ SUCCESS ] Functions: {functionsNamesList}");
      }

      if(reassemblyStage)
      {
        JsonSerializerSettings jsonSettings2 = new JsonSerializerSettings
        {
          MaxDepth = 5,
          //Formatting = Formatting.Indented,
          PreserveReferencesHandling = PreserveReferencesHandling.All
        };

        File.WriteAllText($"{Configuration.rootDirectory}\\unscaledset.dat", $"[Functions: {functionsNamesList}]" +
           "\r\n\r\n" + SerializeObject(fullInputSet, jsonSettings2));
      }

      //dump(ptr, "ptr", "dev");
      /*if (trainDataOriginal == null)
   {
     string trainPath = $"{Configuration.rootDirectory}\\{App.currentNetworkId}\\traindata.dat";
     log($"loading pretrain '{trainPath}' ...");
     trainData = new TrainingData();
     trainData.ReadTrainFromFile(trainPath);
     trainDataOriginal = trainData;
   }
   else
   {
     trainData = trainDataOriginal;
   }

   TrainingData trainDataCurrent = new TrainingData();
   double[][] ptr = new double[1][];
   double[][] outPtr = new double[1][];
   ptr[0] = fullInputSet;
   outPtr[0] = new double[2] { 0, 0 };
   trainDataCurrent.SetTrainData(ptr, outPtr);
   trainData.MergeTrainData(trainDataCurrent);
   neuralNetwork.SetScalingParams(trainData, -1.0f, 1.0f, -1.0f, 1.0f);
   //neuralNetwork.ScaleInput(trainData.GetTrainInput(0));
   //neuralNetwork.ScaleTrain(trainData);
   trainData.ScaleInputTrainData(-1.0, 1.0);

   double[] outD;
   outD = trainData.GetTrainInput(0).Array;
   //outD = fullInputSet;
   File.WriteAllText($"{Configuration.rootDirectory}\\scaledset.dat", SerializeObject(outD, jsonSettings2));*/

      double[] networkOutput = neuralNetwork.Run(fullInputSet);
      //neuralNetwork.DescaleOutput(networkOutput);

      reassemblyStage = false;
      return (functionsConfiguration.Count, networkOutput);
    }
  }
}
