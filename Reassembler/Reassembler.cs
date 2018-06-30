﻿// *    * *    *                   *    * ** *    *                   *    * *
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
		static LivePrices prices = new LivePrices();
		static Dictionary<string, FunctionsConfiguration> functionConfigurationInput;
		static Core.RetCode ret = Core.RetCode.UnknownErr;
		static int OutBegIdx = 0;
		static int OutNbElement = -1;
		static int pOutNbElement = 0;
		static int OutIndex = -1;
		static int typeOut = 0;
		static int nOutBegIdx = 0;
		static int startIdx = 0;
		static int fidx = 0;
		static int iReal = 0;
		static Random random = new Random();
		static int networkFunctionsCount = 0;
		static int[] resultDataInt = null;
		static string paramName = String.Empty;
		static string comment = string.Empty;
		static string functionName = string.Empty;
		static string functionConfigurationHash = string.Empty;
		static double paramValue = 0.0;
		static double[] resultDataDouble = null;
		static double[] fullInputSet = null;
		static object[] functionArguments = null;
		static bool failedReassemble = false;
		static bool reassemblyStage = true;
		private static string allFunctionsMark;

		public static (int, double[]) Execute(string functionConfigurationString, int inputDimension, NeuralNet neuralNetwork,
			bool reassemblingCompletedOverride, MqlApi mqlApi)
		{
			reassemblyStage = reassemblingCompletedOverride;

			allFunctionsMark = string.Empty;

			logIf(reassemblyStage, $"=> Reassembling input sequence ...");

			fullInputSet = null;

			if (failedReassemble)
				reassemblyStage = true;

			failedReassemble = false;

			if (functionConfigurationHash != Hash.md5(functionConfigurationString))
			{
				log($"hashOfFunctionConfiguration ({functionConfigurationHash}) not match content, deserializing {functionConfigurationString.Length} bytes ...");
				var jsonSettings = new JsonSerializerSettings
				{
					MetadataPropertyHandling = MetadataPropertyHandling.Ignore
				};
				functionConfigurationInput = DeserializeObject<Dictionary<string, FunctionsConfiguration>>
					(functionConfigurationString, jsonSettings);

				functionConfigurationHash = Hash.md5(functionConfigurationString);
				log($"hash of configuration: {functionConfigurationHash}");
			}

			logIf(reassemblyStage, $"=> {functionConfigurationInput.Count} functions with {inputDimension} input dimension");

			networkFunctionsCount = functionConfigurationInput.Count;

			foreach (var function in functionConfigurationInput)
			{
				functionName = function.Key;
				FunctionsConfiguration conf = function.Value;

				string stringOut = string.Empty;
				functionArguments = new object[conf.parameters.parametersMap.Count];
				int paramIndex = 0;
				string[] values = new string[4];
				int numFunctionDimension = inputDimension;
				int internalIndex;

				foreach (var param in conf.parameters.parametersMap)
				{
					values = param.Split('|');
					paramName = values[1];

					int.TryParse(values[0], out internalIndex);
					comment = values[3];
					double.TryParse(values[2], out paramValue);

					switch (paramName)
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

							if (comment.StartsWith("High"))
								iReal = 2;
							if (comment.StartsWith("Open"))
								iReal = 0;
							if (comment.StartsWith("Close"))
								iReal = 1;
							if (comment.StartsWith("Low"))
								iReal = 3;

							switch (iReal)
							{
								case 0:
									functionArguments[paramIndex] = prices.GetOpen(numFunctionDimension, mqlApi.Bars, mqlApi.Open);
									break;
								case 1:
									functionArguments[paramIndex] = prices.GetClose(numFunctionDimension, mqlApi.Bars, mqlApi.Close);
									break;
								case 2:
									functionArguments[paramIndex] = prices.GetHigh(numFunctionDimension, mqlApi.Bars, mqlApi.High);
									break;
								case 3:
									functionArguments[paramIndex] = prices.GetLow(numFunctionDimension, mqlApi.Bars, mqlApi.Low);
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
							functionArguments[paramIndex] = prices.GetOpen(numFunctionDimension, mqlApi.Bars, mqlApi.Open);
							break;
						case "inHigh":
							functionArguments[paramIndex] = prices.GetHigh(numFunctionDimension, mqlApi.Bars, mqlApi.High);
							break;
						case "inLow":
							functionArguments[paramIndex] = prices.GetLow(numFunctionDimension, mqlApi.Bars, mqlApi.Low);
							break;
						case "inClose":
							functionArguments[paramIndex] = prices.GetClose(numFunctionDimension, mqlApi.Bars, mqlApi.Close);
							break;
						case "inVolume":
							functionArguments[paramIndex] = prices.GetVolume(numFunctionDimension, mqlApi.Bars, mqlApi.Volume);
							break;
						case "outBegIdx":
							functionArguments[paramIndex] = OutBegIdx;
							nOutBegIdx = paramIndex;
							break;
						case "outNBElement":
							functionArguments[paramIndex] = OutNbElement;
							OutNbElement = pOutNbElement = paramIndex;
							break;
						case "outInteger":
							functionArguments[paramIndex] = new int[numFunctionDimension];
							OutIndex = paramIndex;
							typeOut = 0;
							break;
						case "outReal":
							functionArguments[paramIndex] = new double[numFunctionDimension];
							OutIndex = paramIndex;
							typeOut = 1;
							break;
						case "outAroonUp":
							functionArguments[paramIndex] = new double[11];
							break;
						case "outAroonDown":
							functionArguments[paramIndex] = new double[11];
							break;
						case "outSlowD":
						case "outSlowK":
						case "outFastD":
						case "outFastK":
							functionArguments[paramIndex] = new double[11];
							break;

						default:
							error($"nothing found for {paramName}");
							break;
					}
					paramIndex++;
				}

				logIf(reassemblyStage, $"=> {functionName} arguments({functionArguments.Length})={SerializeObject(functionArguments)}");

				functionArguments[OutIndex] = new double[inputDimension];

				Type[] functionTypes = new Type[conf.parameters.parametersMap.Count];
				int idx = 0;
				foreach (var arg in functionArguments)
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

				MethodInfo FunctionPointer = typeof(Core).GetMethod(functionName, functionTypes);
				if (FunctionPointer == null)
				{
					error($"fail to load method [{functionName}] from TICTAC");
					failedReassemble = true;
				}
				else
				{
					ret = (Core.RetCode) FunctionPointer.Invoke(null, functionArguments);
					int idxS = 0;

					if (typeOut == 0)
					{
						resultDataInt = (int[]) functionArguments[OutIndex];
						Array.Resize<double>(ref resultDataDouble, OutNbElement);

						Array.Copy(resultDataInt, resultDataDouble, OutNbElement);

						for (int i = 0; i < OutNbElement; i++)
						{
							if (resultDataDouble[i] == 0.0 && i == 0 && reassemblyStage)
								warning($"fucking function {functionName} starts with zero");
							if (resultDataDouble[i] == 0.0 && i == OutNbElement - 1 && reassemblyStage)
								warning($"fucking function {functionName} ends with zero");
						}
					}
					else
					{
						resultDataDouble = (double[]) functionArguments[OutIndex];
						for (int i = 0; i < OutNbElement; i++)
						{
							if (resultDataDouble[i] == 0.0 && i == 0 && reassemblyStage)
								warning($"fucking function {functionName} starts with zero");
						}
					}

					startIdx = (int) functionArguments[nOutBegIdx];
					if (reassemblyStage && startIdx != 0)
						warning($"# {functionName}: startIdx = {startIdx} (OutNbElement={OutNbElement}, begIdx={OutBegIdx})");

					logIf(reassemblyStage, $"=> {functionName}({resultDataDouble.Length}): resultDataDouble={SerializeObject(resultDataDouble)}");

					int prevLen = fullInputSet == null ? 0 : fullInputSet.Length;
					int newLen = (fullInputSet == null ? 0 : fullInputSet.Length) + resultDataDouble.Length - startIdx;

					//	dump(resultDataDouble, function.Key, "dev");
					Array.Resize<double>(ref fullInputSet, newLen);
					Array.Copy(resultDataDouble, startIdx, fullInputSet, prevLen > 0 ? prevLen - 1 : 0,
						resultDataDouble.Length - startIdx);

					allFunctionsMark += (allFunctionsMark.Length > 0 ? "+" : "") + $"{function.Key}[{numFunctionDimension}]";
				}
			}

			logIf(reassemblyStage && fullInputSet != null && fullInputSet.Length > 0, $"ret={ret} entireset={SerializeObject(fullInputSet)}");

			if (neuralNetwork.InputCount != fullInputSet.Length)
			{
				error($"=> reassembler FAILED to reassemble input sequence: diff in input count of network is " +
					$"{Math.Abs(fullInputSet.Length - neuralNetwork.InputCount)}");
				reassemblyStage = false;
				failedReassemble = true;
				return (0, null);
			}

			logIf(reassemblyStage, $"=> Reassembling [ SUCCESS ] ");

			//TrainingData trainData = new TrainingData();
			//trainData.SetTrainData((uint) 1, fullInputSet, fullInputSet);

			//JsonSerializerSettings jsonSettings2 = new JsonSerializerSettings
			//{
			//	MaxDepth = 5,
			//	Formatting = Formatting.Indented,
			//	PreserveReferencesHandling = PreserveReferencesHandling.All
			//};

			//File.WriteAllText($"{Configuration.rootDirectory}\\unscaledset.dat", $"[Functions: {allFunctionsMark}]" +
			//	"\r\n\r\n" + SerializeObject(fullInputSet, jsonSettings2));

			//neuralNetwork.ScaleInput(trainData.GetTrainInput(0));

			//File.WriteAllText($"{Configuration.rootDirectory}\\scaledset.dat", SerializeObject(trainData.GetTrainInput(0).Array, jsonSettings2));

			double[] networkOutput = neuralNetwork.Run(fullInputSet);
			neuralNetwork.DescaleOutput(networkOutput);

			reassemblyStage = false;
			return (networkFunctionsCount, networkOutput);
		}
	}
}
