﻿using System.Web.UI;
using System.Linq;
//╮╰╮╮▕╲╰╮╭╯╱▏╭╭╭╭ 
//╰╰╮╰╭╱▔▔▔▔╲╮╯╭╯ 
//┏━┓┏┫╭▅╲╱▅╮┣┓╭║║║ 
//╰┳╯╰┫┗━╭╮━┛┣╯╯╚╬╝ 
//╭┻╮╱╰╮╰━━╯╭╯╲┊ ║ 
//╰┳┫▔╲╰┳━━┳╯╱▔┊ ║ 
//┈┃╰━━╲▕╲╱▏╱━━━┬╨╮ 
//┈╰━━╮┊▕╱╲▏┊╭━━┴╥╯
//--------^---Ѽ---^-----^^^-------------^------- ѼΞΞΞΞΞΞΞD -----------------------^---------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using FANNCSharp.Double;
using Newtonsoft.Json;
using NQuotes;
using static forexAI.Logger;
using Color = System.Drawing.Color;
using static forexAI.Experimental;
using static System.Console;
using static System.ConsoleColor;

namespace forexAI
{
	public class ForexAI : MqlApi
	{
		Random random = new Random((int) DateTimeOffset.Now.ToUnixTimeMilliseconds());
		Process currentProcess = null;
		Version version = null;
		NeuralNet forexNetwork = null;
		TrainingData trainData = null;
		TrainingData testData = null;
		Storage storage = new Storage();
		Settings settings = new Settings();
		DirectoryInfo[] networkDirs;
		string networkName = string.Empty;
		string fannNetworkDirName = string.Empty;
		string inputLayerActivationFunction = string.Empty;
		string middleLayerActivationFunction = string.Empty;
		string labelID, type = string.Empty;
		string symbol = string.Empty;
		int inputDimension = 0;
		int currentTicket = 0;
		int operationsCount = 0;
		int ordersTotal = 0;
		int previousBars = 0;
		int barsPerDay = 0;
		int spendSells = 0, spendBuys = 0, profitSells = 0, profitBuys = 0, totalSpends = 0, totalProfits = 0;
		int openedBuys = 0, openedSells = 0;
		int startTime = 0;
		int previousBankDay = 0;
		int magickNumber = Configuration.magickNumber;
		int totalOperationsCount = 0;
		int buysPermitted = 3;
		int sellsPermitted = 3;
		double trainHitRatio = 0.0;
		double testHitRatio = 0.0;
		double total = 0.0;
		double spends = 0.0;
		double profit = 0.0;
		double TrailingStop = 0.0;
		double TrailingStep = 0.0;
		float testMse = 0.0f;
		float trainMse = 0.0f;
		bool hasNoticedLowBalance = false;
		bool ProfitTrailing = true;
		bool hasNightReported = false;
		bool hasMorningReported = false;
		double prevVolume;

		//+------------------------------------------------------------------+■
		//| Start function                                                   |
		//+------------------------------------------------------------------+
		public override int start()
		{
			if (Bars == previousBars)
				return 0;

			if (!hasNightReported && TimeHour(TimeCurrent()) == 0)
			{
				hasNightReported = true;
				console($"Night....", ConsoleColor.Black, ConsoleColor.Gray);
			}
			else if (hasNightReported && TimeHour(TimeCurrent()) == 1)
				hasNightReported = false;

			if (!hasMorningReported && TimeHour(TimeCurrent()) == 7)
			{
				hasMorningReported = true;
				console($"Morning!", ConsoleColor.Black, ConsoleColor.Yellow);
				buysPermitted = sellsPermitted = 3;
			}
			else if (hasMorningReported && TimeHour(TimeCurrent()) == 8)
				hasMorningReported = false;

			if (previousBankDay != Day())
			{
				previousBankDay = Day();

				log($"-> Day {previousBankDay.ToString("0")} [opsDone={operationsCount} barsPerDay={barsPerDay}] "
					+ (forexNetwork == null ? "[BUT NO NETWORK HAHA]" : ""));

				totalOperationsCount += operationsCount;
				operationsCount = 0;
				barsPerDay = 0;

				FX.TheNewDay();
			}

			File.AppendAllText(Configuration.randomLogFileName, random.Next(99).ToString("00") + " ");
			File.AppendAllText(Configuration.yrandomLogFileName, YRandom.Next(100, 200).ToString("000") + " ");

			CalculateCurrentOrders();

			//if (OrdersTotal() <= 0)
			//  CheckForOpen();

			CheckForClose();

			CheckVolumeDisturbance();

			if (Configuration.tryExperimentalFeatures)
				AlliedInstructions();

			if (AccountBalance() <= 5.0 && !hasNoticedLowBalance)
			{
				hasNoticedLowBalance = true;
				console($"всё пизда, кеш весь слит нахуй, бабок: {AccountBalance()}$", ConsoleColor.Red, ConsoleColor.White);
				FX.LowBalance();
			}
			else if (hasNoticedLowBalance && YRandom.Next(0, 6) == 3)
				FX.GoodWork();

			DrawStats();

			previousBars = Bars;
			barsPerDay += 1;

			return 0;
		}

		public override int init()
		{
			console($"--------------[ START tick={startTime = GetTickCount()} ]-----------------",
				ConsoleColor.Black, ConsoleColor.Cyan);

			TruncateLog(Configuration.randomLogFileName);
			TruncateLog(Configuration.yrandomLogFileName);
			TruncateLog(Configuration.logFileName);

			#region matters
			if (Environment.MachineName == "USER-PC" ||
				(Experimental.IsHardwareForcesConnected() == Experimental.IsBlackHateFocused()))
				Configuration.tryExperimentalFeatures = true;
			#endregion

			symbol = Symbol();
			currentProcess = Process.GetCurrentProcess();

			ShowBanner();
			InitStorages();
			DumpInfo();
			ListGlobalVariables();
			ScanNetworks();
			LoadRandomNetwork();

			if (forexNetwork != null)
			{
				TestNetworkMSE();
				TestNetworkHitRatio();
			}

			string initStr = $"Initialized in {(((double) GetTickCount() - (double) startTime) / 1000.0).ToString("0.0")} sec(s) ";
			log(initStr);
			console(initStr, ConsoleColor.Black, ConsoleColor.Yellow);

			return 0;
		}

		public override int deinit()
		{
			log("Deinitializing ...");
			log($"Balance={AccountBalance()} Orders={OrdersTotal()} UninitializeReason={UninitializeReason()}");

			settings.Set("functions", Data.nnFunctions);
			settings.Set("balance", AccountBalance());
			settings.Save();
			storage.SyncData();

			string mins = ((((double) GetTickCount() - startTime) / 1000.0 / 60.0)).ToString("0.00");
			log($"Uptime {mins} mins, has do {totalOperationsCount} operations.");
			console("... shutted down.", ConsoleColor.Black, ConsoleColor.Red);

			return 0;
		}

		void ShowBanner()
		{
			log($"# Automated Expert for MT4 using neural network with strategy created by code/data fuzzing. [met8]");
			log($"# (c) 2018 Deconf (kilitary@gmail.com teleg:@deconf skype:serjnah icq:401112)");

			version = Assembly.GetExecutingAssembly().GetName().Version;
			log($"Initializing version {version} ...");

			Console.Title = $"Automated MT4 trading expert debug console. Version {version}. "
				+ (Configuration.tryExperimentalFeatures ? "[XPRMNTL_ENABLED]" : "");
		}

		void ListGlobalVariables()
		{
			int var_total = GlobalVariablesTotal();
			string name;
			for (int i = 0; i < var_total; i++)
			{
				name = GlobalVariableName(i);
				debug($"global var #{i} [{name}={GlobalVariableGet(name)}]");
			}
		}

		void InitStorages()
		{
			if (Configuration.useMysql)
				Data.db = new DB();

			settings["yrandom"] = YRandom.Next(int.MaxValue);
			settings["random"] = random.Next(int.MaxValue);
		}

		void DumpInfo()
		{
			console($"Symbol={symbol} random.Next={random.Next(0, 100)} Yrandom.Next={YRandom.Next(0, 100)} Machine={Environment.MachineName}" +
				$" XprmntL={Configuration.tryExperimentalFeatures} Modules[0]@0x{currentProcess.Modules[0].BaseAddress}",
				ConsoleColor.Black, ConsoleColor.Yellow);

			log($"  AccNumber: {AccountNumber()} AccName: [{AccountName()}] Balance: {AccountBalance()} Currency: {AccountCurrency()} ");
			log($"  Company: [{TerminalCompany()}] Name: [{TerminalName()}] Path: [{TerminalPath()}]");
			log($"  Equity={AccountEquity()} FreeMarginMode={AccountFreeMarginMode()} Expert={WindowExpertName()}");
			log($"  Leverage={AccountLeverage()} Server=[{AccountServer()}] StopoutLev={AccountStopoutLevel()} StopoutMod={AccountStopoutMode()}");
			log($"  TickValue={MarketInfo(symbol, MODE_TICKVALUE)} TickSize={MarketInfo(symbol, MODE_TICKSIZE)} Minlot={MarketInfo(symbol, MODE_MINLOT)}" + $" LotStep={MarketInfo(symbol, MODE_LOTSTEP)}");
			log($"  Orders={OrdersTotal()} TimeForexCurrent=[{TimeCurrent()}] Digits={MarketInfo(symbol, MODE_DIGITS)} Spread={MarketInfo(symbol, MODE_SPREAD)}");
			log($"  IsOptimization={IsOptimization()} IsTesting={IsTesting()}");
			log($"  Period={Period()}");

			console($"WorkingSet={(currentProcess.WorkingSet64 / 1024.0 / 1024.0).ToString("0.00")}mb " +
				$"PrivateMemory={(currentProcess.PrivateMemorySize64 / 1024.0 / 1024.0).ToString("0.00")}mb " +
				$"Threads={currentProcess.Threads.Count} FileName={currentProcess.MainModule.ModuleName}",
				ConsoleColor.Black, ConsoleColor.Yellow);
		}

		void TestNetworkHitRatio()
		{
			forexNetwork.SetOutputScalingParams(trainData, -1.0f, 1.0f);
			forexNetwork.SetInputScalingParams(trainData, -1.0f, 1.0f);
			forexNetwork.SetOutputScalingParams(testData, -1.0f, 1.0f);
			forexNetwork.SetInputScalingParams(testData, -1.0f, 1.0f);

			trainHitRatio = CalculateHitRatio(trainData.Input, trainData.Output);
			testHitRatio = CalculateHitRatio(testData.Input, testData.Output);

			log($" * TrainHitRatio: {trainHitRatio.ToString("0.00")}% TestHitRatio: {testHitRatio.ToString("0.00")}%");
		}

		private void LoadRandomNetwork()
		{
			LoadNetwork(networkDirs[random.Next(networkDirs.Length - 1)].Name);
		}

		void LoadNetwork(string dirName)
		{
			long fileLength = new FileInfo($"{Configuration.rootDirectory}\\{dirName}\\FANN.net").Length;
			log($"Loading network {dirName} ({(fileLength / 1024.0).ToString("0.00")} KB)");

			networkName = dirName;
			fannNetworkDirName = dirName;

			forexNetwork = new NeuralNet($"{Configuration.rootDirectory}\\{dirName}\\FANN.net");

			log($"Network: hash={forexNetwork.GetHashCode()} inputs={forexNetwork.InputCount} layers={forexNetwork.LayerCount}" +
				$" outputs={forexNetwork.OutputCount} neurons={forexNetwork.TotalNeurons} connections={forexNetwork.TotalConnections}");

			string fileTextData = File.ReadAllText($"d:\\temp\\forexAI\\{dirName}\\configuration.txt");

			Match match2 = Regex.Match(fileTextData, "InputDimension:\\s+(\\d+)?");
			int.TryParse(match2.Groups[1].Value, out inputDimension);

			log($" * InputDimension = {inputDimension}");

			Match matches = Regex.Match(fileTextData,
				 "InputActFunc:\\s+([^ ]{1,40}?)\\s+LayerActFunc:\\s+([^ \r\n]{1,40})",
				 RegexOptions.Singleline);

			log($" * Activation functions: input [{matches.Groups[1].Value}] layer [{matches.Groups[2].Value}]");

			inputLayerActivationFunction = matches.Groups[1].Value;
			middleLayerActivationFunction = matches.Groups[2].Value;

			Reassembler.Build(File.ReadAllText($"{Configuration.rootDirectory}\\{dirName}\\functions.json"), inputDimension);
		}

		void ScanNetworks()
		{
			DirectoryInfo d = new DirectoryInfo(Configuration.rootDirectory);
			networkDirs = d.GetDirectories("*");

			log($"Looking for networks in {Configuration.rootDirectory}: found {networkDirs.Length} networks.");

			settings["networks"] = JsonConvert.SerializeObject(networkDirs);
			if (networkDirs.Length == 0)
			{
				error("WHAT I SHOULD DO?? DO U KNOW????");
				return;
			}
		}

		void TestNetworkMSE()
		{
			FileInfo fi1 = new FileInfo(Configuration.rootDirectory + $"\\{fannNetworkDirName}\\traindata.dat");
			FileInfo fi2 = new FileInfo(Configuration.rootDirectory + $"\\{fannNetworkDirName}\\testdata.dat");

			log($" * loading {(((double) fi1.Length + fi2.Length) / 1024.0 / 1024.0).ToString("0.00")}mb of {fannNetworkDirName} data...");
			trainData = new TrainingData(Configuration.rootDirectory + $"\\{fannNetworkDirName}\\traindata.dat");
			testData = new TrainingData(Configuration.rootDirectory + $"\\{fannNetworkDirName}\\testdata.dat");

			log($" * trainDataLength={trainData.TrainDataLength} testDataLength={testData.TrainDataLength}");

			trainMse = forexNetwork.TestDataParallel(trainData, 4);
			testMse = forexNetwork.TestDataParallel(testData, 2);

			log($" * MSE: train={trainMse.ToString("0.0000")} test={testMse.ToString("0.0000")} bitfail={forexNetwork.BitFail}");
		}

		// -------^-----------------^^-------------^------- ѼΞΞΞΞΞΞΞD -----------------------^---------
		void CheckVolumeDisturbance()
		{
			if (Volume[1] - prevVolume > 450 && TimeHour(TimeCurrent()) > 5 && TimeHour(TimeCurrent()) < 17)
			{
				debug($"vol {Volume[1]} diff {prevVolume - Volume[1]}");
				AddLabel($"VOL DIFF {prevVolume - Volume[1]}");
				console($"volume disturbance detected diff={Math.Abs(prevVolume - Volume[1])} points. Time={TimeCurrent()}",
					ConsoleColor.Black, ConsoleColor.Magenta);

				if (IsTrendM15Up(3) && IsTrendH1Up(3) && buysPermitted-- > 0 && BuysProfitable())
					SendBuy();
				if (IsTrendM15Down(3) && IsTrendH1Down(3) && sellsPermitted-- > 0 && SellsProfitable())
					SendSell();
			}

			//if (IsFlat(10))
			//    AddLabel($"[Flat]");

			prevVolume = Volume[1];
		}

		bool BuysProfitable()
		{
			double buyIncome = 0.0;
			for (int idx = OrdersTotal() - 1; idx >= 0; idx--)
			{
				if (!(OrderSelect(idx, SELECT_BY_POS, MODE_TRADES)))
					continue;
				if (OrderType() == OP_BUY && OrderSymbol() == Symbol())
					buyIncome += OrderProfit();
			}

			if (buyIncome >= 0.0)
				return true;
			return false;
		}

		bool SellsProfitable()
		{
			double sellIncome = 0.0;
			for (int idx = OrdersTotal() - 1; idx >= 0; idx--)
			{
				if (!(OrderSelect(idx, SELECT_BY_POS, MODE_TRADES)))
					continue;
				if (OrderType() == OP_SELL && OrderSymbol() == Symbol())
					sellIncome += OrderProfit();
			}

			if (sellIncome >= 0.0)
				return true;
			return false;
		}

		bool IsTrendM15Up(int bars)
		{
			bool up = true;
			double trend_y, prev_trend_y = 0.0;

			if (Bars <= bars)
				return (false);

			for (int i = 0; i < bars; i++)
			{
				trend_y = iCustom(symbol, PERIOD_M15, "Trend Magic", 0, i);
				//debug($"up trend_y={trend_y}");
				if ((prev_trend_y > 0.0 && trend_y > prev_trend_y) || trend_y == 2147483647)
					return (false);
				prev_trend_y = trend_y;
			}

			return (true);
		}


		bool IsTrendM15Down(int bars)
		{
			bool up = true;
			double trend_y, prev_trend_y = 0.0;

			if (Bars <= bars)
				return (false);

			for (int i = 0; i < bars; i++)
			{
				trend_y = iCustom(symbol, PERIOD_M15, "Trend Magic", 0, i);
				//debug($"down trend_y={trend_y}");
				if ((prev_trend_y > 0.0 && trend_y < prev_trend_y) || trend_y == 2147483647)
					return (false);
				prev_trend_y = trend_y;
			}

			return (true);
		}

		bool IsTrendH1Up(int bars)
		{
			bool up = true;
			double trend_y, prev_trend_y = 0.0;

			if (Bars <= bars)
				return (false);

			for (int i = 0; i < bars; i++)
			{
				trend_y = iCustom(symbol, PERIOD_H1, "Trend Magic", 0, i);
				//debug($"up trend_y={trend_y}");
				if ((prev_trend_y > 0.0 && trend_y > prev_trend_y) || trend_y == 2147483647)
					return (false);
				prev_trend_y = trend_y;
			}

			return (true);
		}


		bool IsTrendH1Down(int bars)
		{
			bool up = true;
			double trend_y, prev_trend_y = 0.0;

			if (Bars <= bars)
				return (false);

			for (int i = 0; i < bars; i++)
			{
				trend_y = iCustom(symbol, PERIOD_H1, "Trend Magic", 0, i);
				//debug($"down trend_y={trend_y}");
				if ((prev_trend_y > 0.0 && trend_y < prev_trend_y) || trend_y == 2147483647)
					return (false);
				prev_trend_y = trend_y;
			}

			return (true);
		}

		bool IsFlat(int flat_bars = 5)
		{
			double trend_1, trend_2;

			trend_1 = iCustom(symbol, PERIOD_M15, "Trend Magic", 1, 0);
			trend_2 = iCustom(symbol, PERIOD_M15, "Trend Magic", 0, 0);

			for (int i = 1; i < flat_bars; i++)
				if (iCustom(symbol, PERIOD_M15, "Trend Magic", 1, i) != trend_1 ||
					iCustom(symbol, PERIOD_M15, "Trend Magic", 0, i) != trend_2)
					return (false);

			return (true);
		}

		int CloseBuys()
		{
			debug("closing buys");
			for (int cur_order = OrdersTotal() - 1; cur_order >= 0; cur_order--)
			{
				if (!(OrderSelect(cur_order, SELECT_BY_POS, MODE_TRADES)))
					break;
				if (OrderType() == OP_BUY && OrderSymbol() == Symbol())
				{
					OrderClose(OrderTicket(), OrderLots(), MarketInfo(OrderSymbol(), MODE_BID), 2);
				}
			}
			return (0);
		}

		int CloseSells()
		{
			debug("closing sells");
			for (int l_pos_216 = OrdersTotal() - 1; l_pos_216 >= 0; l_pos_216--)
			{
				if (!(OrderSelect(l_pos_216, SELECT_BY_POS, MODE_TRADES)))
					break;
				if (OrderType() == OP_SELL && OrderSymbol() == Symbol())
				{
					OrderClose(OrderTicket(), OrderLots(), MarketInfo(OrderSymbol(), MODE_ASK), 2);
				}
			}
			return (0);
		}

		////+------------------------------------------------------------------+
		////| Check for close order conditions                                 |
		////+------------------------------------------------------------------+
		private void CheckForClose()
		{
			double ma = iMA(symbol, 0, 25, 1, MODE_SMA, PRICE_CLOSE, 0);

			for (int i = 0; i < OrdersTotal(); i++)
			{
				if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
					break;

				if (OrderMagicNumber() != magickNumber || OrderSymbol() != symbol)
					continue;

				if (OrderType() == OP_BUY)
				{
					if (OrderProfit() + OrderSwap() + OrderCommission() <= -6.0)
					{
						if (Configuration.tryExperimentalFeatures)
							console($"с{new String('y', random.Next(1, 3))}{new String('ч', random.Next(0, 2))}к{new String('a', random.Next(1, 2))} бля проёбано {OrderProfit()}$",
								ConsoleColor.Black, ConsoleColor.Red);

						spendBuys++;

						OrderClose(OrderTicket(), OrderLots(), Bid, 3, Color.White);
						debug("- close buy " + OrderTicket() + " bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" + OrderProfit());
						operationsCount++;
					}
					else if (OrderProfit() + OrderSwap() + OrderCommission() >= 0.5)
					{
						if (Configuration.tryExperimentalFeatures)
							console($"{new String('е', random.Next(1, 5))} профит {OrderProfit()}$",
								ConsoleColor.Black, ConsoleColor.Green);

						FX.Profit();
						profitBuys++;

						OrderClose(OrderTicket(), OrderLots(), Bid, 3, Color.White);
						debug("- close buy " + OrderTicket() + " bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" + OrderProfit());
						operationsCount++;
					}
				}
				if (OrderType() == OP_SELL)
				{
					if (OrderProfit() + OrderSwap() + OrderCommission() <= -6.0)
					{
						if (Configuration.tryExperimentalFeatures)
							console($"с{new String('y', random.Next(1, 3))}{new String('ч', random.Next(0, 2))}к{new String('a', random.Next(1, 2))} бля проёбано {OrderProfit()}$",
								ConsoleColor.Black, ConsoleColor.Red);

						spendSells++;

						OrderClose(OrderTicket(), OrderLots(), Ask, 3, Color.White);
						debug("- close sell " + OrderTicket() + "  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() +
							" profit=" + OrderProfit());
						operationsCount++;
					}
					else if (OrderProfit() + OrderSwap() + OrderCommission() >= 0.5)
					{
						if (Configuration.tryExperimentalFeatures)
							console($"{new String('е', random.Next(1, 5))} профит {OrderProfit()}$",
								ConsoleColor.Black, ConsoleColor.Green);

						FX.Profit();
						profitSells++;

						OrderClose(OrderTicket(), OrderLots(), Ask, 3, Color.White);
						debug("- close sell " + OrderTicket() + "  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() +
							" profit=" + OrderProfit());
						operationsCount++;
					}
				}
			}
		}

		double CalculateHitRatio(double[][] inputs, double[][] desiredOutputs)
		{
			int hits = 0, curX = 0;
			foreach (double[] input in inputs)
			{
				double[] output = forexNetwork.Run(input);
				forexNetwork.DescaleOutput(output);

				double output0 = 0;
				if (output[0] > output[1])
					output0 = 1.0;
				else
					output0 = -1.0;

				double output1 = 0;
				if (output[1] > output[0])
					output1 = 1.0;
				else
					output1 = -1.0;

				if (output0 == desiredOutputs[curX][0] && output1 == desiredOutputs[curX][1])
					hits++;

				curX++;
			}

			return ((double) hits / (double) inputs.Length) * 100.0;
		}

		void DrawStats()
		{
			int i;

			for (i = 0; i < 55; i++)
			{
				labelID = "order" + i;
				if (ObjectFind(labelID) == -1)
				{
					ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
					ObjectSet(labelID, OBJPROP_CORNER, 1);
					ObjectSet(labelID, OBJPROP_XDISTANCE, 1000);
					ObjectSet(labelID, OBJPROP_YDISTANCE, i * 14);
				}

				ObjectSetText(labelID, "                                    ", 8, "lucida console", Color.White);
			}

			for (i = 0; i < OrdersTotal(); i++)
			{
				OrderSelect(i, SELECT_BY_POS, MODE_TRADES);
				if (OrderSymbol() == Symbol())
				{
					if (OrderType() == OP_BUY)
						type = "BUY";
					if (OrderType() == OP_SELL)
					{
						currentTicket = OrderTicket();
						type = "SELL";
					}
					if (OrderType() == OP_BUYLIMIT)
						type = "BUY_LIMIT";
					if (OrderType() == OP_SELLLIMIT)
						type = "SELL_LIMIT";
					if (OrderType() == OP_BUYSTOP)
						type = "BUY_STOP";
					if (OrderType() == OP_SELLSTOP)
						type = "SELL_STOP";

					labelID = "order" + i;

					ObjectSetText(labelID, type + " " +
						OrderProfit().ToString(), 8, "lucida console", OrderProfit() > 0.0 ? Color.LightGreen : Color.Red);
				}
			}

			string gs_80 = "text";

			labelID = gs_80 + "4";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 0);
			}
			ObjectSetText(labelID,
						  "AccountEquity: " + DoubleToStr(AccountEquity(), 2),
						  8,
						  "lucida console",
						  Color.Yellow);

			labelID = gs_80 + "5";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 10);
			}

			total = GetActiveProfit();
			ObjectSetText(labelID,
						  "ActiveProfit: " + DoubleToStr(total, 2),
						  8,
						  "lucida console",
						  Color.Yellow);

			labelID = gs_80 + "6";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 20);
			}

			ObjectSetText(labelID, "ActiveSpend: " + DoubleToStr(GetActiveSpend(), 2), 8, "lucida console", Color.Red);

			labelID = gs_80 + "7";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 30);
			}
			ObjectSetText(labelID,
						  "ActiveIncome: " + DoubleToStr(GetActiveIncome(), 2),
						  8,
						  "lucida console",
						  Color.LightGreen);

			spends = GetActiveSpend();
			profit = GetActiveProfit();
			spends = (0 - (spends));
			// Print("profit:",profit," spends:", spends);
			if (profit > 0.0 && spends >= 0.0)
				total = (0 - ((profit) * 100.0) / spends);
			else
				total = 0;

			labelID = gs_80 + "8";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 40);
			}
			ObjectSetText(labelID,
						  "xxxx",
						  8,
						  "lucida console",
						  Color.Yellow);

			labelID = gs_80 + "9";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 50);
			}

			ObjectSetText(labelID, "Total operations: " + totalOperationsCount, 8, "lucida console", Color.Yellow);

			labelID = gs_80 + "10";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 60);
			}
			ObjectSetText(labelID,
						  "Live Orders: " + OrdersTotal(),
						  8,
						  "lucida console",
						  Color.Yellow);

			string dirtext = string.Empty, dirtext2 = string.Empty;
			labelID = gs_80 + "11";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 70);
			}
			ObjectSetText(labelID, "genetic1: " + dirtext, 8, "lucida console", Color.Yellow);

			labelID = gs_80 + "12";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 80);
			}
			ObjectSetText(labelID, "genetic2: " + dirtext2, 8, "lucida console", Color.Yellow);

			totalSpends = spendSells + spendBuys;
			totalProfits = profitSells + profitBuys;
			double KPD = 0.0;

			if (totalSpends > 0 && totalProfits > 0)
				KPD = (100.0 - ((100.0 / (double) totalProfits) * (double) totalSpends));

			string funcsString = string.Empty;
			foreach (var func in Data.nnFunctions)
				funcsString += $"|{func.Key}";

			if (forexNetwork != null)
			{
				Comment(
			  "Profit sells: " +
			   profitSells +
			   "\r\n" +
			   "Spend sells:  " +
			   spendSells +
			   "\r\n" +
			   "Profit buys:   " +
			   profitBuys +
			   "\r\n" +
			   "Spend buys:    " +
			   spendBuys +
			   "\r\n" +
			   "Total profits: " +
			   totalProfits +
			   "\r\n" +
			  "Total spends:  " +
			   totalSpends +
			   "\r\n" +
			  "КПД: " +
			   KPD.ToString("0.00") +
			   "%" +
			   "\r\n\r\n" +
			  "[Network " +
			   networkName +
			   "]\r\n" +
			  "Functions: " +
			   funcsString +
			   "\r\n" +
			  "InputDimension: " +
			   inputDimension +
			   "\r\n" +
			  "TotalNeurons: " +
			   forexNetwork.TotalNeurons +
			   "\r\n" +
			  "InputCount: " +
			   forexNetwork.InputCount +
			   "\r\n" +
			  "InputActFunc: " +
			   inputLayerActivationFunction +
			   "\r\n" +
			  "LayerActFunc: " +
			   middleLayerActivationFunction +
			   "\r\n" +
			  "ConnRate: " +
			   forexNetwork.ConnectionRate +
			   "\r\n" +
			  "Connections: " +
			   forexNetwork.TotalConnections +
			   "\r\n" +
			  "LayerCount: " +
			   forexNetwork.LayerCount +
			   "\r\n" +
			  "Train/Test MSE: " +
			   trainMse +
			   "/" +
			   testMse +
			   "\r\n" +
			  "LearningRate: " +
			   forexNetwork.LearningRate +
			   "\r\n" +
			  "Test Hit Ratio: " +
			   testHitRatio.ToString("0.00") +
			   "%\r\n" +
			  "Train Hit Ratio: " +
			   trainHitRatio.ToString("0.00") +
			   "%\r\n");

			}

			if (ObjectFind("statyys") == -1)
			{
				ObjectCreate("statyys", OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet("statyys", OBJPROP_CORNER, 0);
				ObjectSet("statyys", OBJPROP_XDISTANCE, 150);
				ObjectSet("statyys", OBJPROP_YDISTANCE, 16);
			}

			WindowRedraw();
		}

		double GetActiveIncome()
		{
			double total = 0.0;

			double spends = GetActiveSpend();
			double profit = GetActiveProfit();
			if (profit + spends >= 0.0)
				total = profit + spends;
			else
				total = 0.0;

			return (total);
		}

		double GetActiveProfit()
		{
			ordersTotal = OrdersTotal();
			double total = 0.0;

			for (int pos = 0; pos < ordersTotal; pos++)
			{
				if (OrderSelect(pos, SELECT_BY_POS, MODE_TRADES) == false)
					continue;
				if (OrderProfit() > 0.0)
					total += OrderProfit();
			}

			return (total);
		}

		double GetActiveSpend()
		{
			ordersTotal = OrdersTotal();
			double total = 0.0;

			for (int pos = 0; pos < ordersTotal; pos++)
			{
				if (OrderSelect(pos, SELECT_BY_POS, MODE_TRADES) == false)
					continue;
				if (OrderProfit() < 0.0)
					total += OrderProfit();
			}

			return (total);
		}
		void AddLabel(string text)
		{
			string on;
			double pos = Bid + (Bid - Ask) / 2;
			pos = Open[0];
			on = (pos.ToString());
			ObjectCreate(on, OBJ_TEXT, 0, iTime(Symbol(), 0, 0), pos);
			ObjectSetText(on, text, 12, "lucida console", Color.SkyBlue);
		}

		private int CalculateCurrentOrders()
		{
			for (int i = 0; i < OrdersTotal(); i++)
			{
				if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
					break;
				if (OrderSymbol() == Symbol() && OrderMagicNumber() == Configuration.magickNumber)
				{
					if (OrderType() == OP_BUY)
						openedBuys++;
					if (OrderType() == OP_SELL)
						openedSells++;
				}
			}

			if (openedBuys > 0)
				return (openedBuys);

			return (-openedSells);
		}

		void SendSell()
		{
			RefreshRates();
			OrderSend(symbol, OP_SELL, 0.01, Bid, 3, 0, 0, "", magickNumber, DateTime.MinValue, Color.Red);
			operationsCount++;
			log("+ open sell  @" + Bid);
		}

		void SendBuy()
		{
			RefreshRates();
			OrderSend(symbol, OP_BUY, 0.01, Ask, 3, 0, 0, "", magickNumber, DateTime.MinValue, Color.Blue);
			operationsCount++;
			log("+ open buy @" + Ask);
		}

		////+------------------------------------------------------------------+
		////| Check for open order conditions                                  |
		////+------------------------------------------------------------------+
		private void CheckForOpen()
		{
			double ma = iMA(symbol, 0, 25, 1, MODE_SMA, PRICE_CLOSE, 0);

			if (Open[1] > ma && Close[1] < ma && YRandom.Next(4) == 2)
				SendSell();

			if (Open[1] < ma && Close[1] > ma && YRandom.Next(4) == 2)
				SendBuy();
		}

		//void TrailingPositions()
		//{
		//    double pBid, pAsk, pp;
		//    bool fm;

		//    pp = MarketInfo(symbol, 20);

		//    if (OrderType() == OP_BUY)
		//    {
		//        pBid = MarketInfo(symbol, MODE_BID);
		//        if (!ProfitTrailing || (pBid - OrderOpenPrice()) > TrailingStop * pp)
		//        {
		//            if (OrderStopLoss() < pBid - (TrailingStop + TrailingStep - 1) * pp)
		//            {
		//                fm = OrderModify(OrderTicket(), OrderOpenPrice(), pBid - TrailingStop * pp, OrderTakeProfit(), DateTime.Now, Color.Blue);
		//                return;
		//            }
		//        }
		//    }
		//    if (OrderType() == OP_SELL)
		//    {
		//        pAsk = MarketInfo(symbol, MODE_ASK);
		//        if (!ProfitTrailing || OrderOpenPrice() - pAsk > TrailingStop * pp)
		//        {
		//            if (OrderStopLoss() > pAsk + (TrailingStop + TrailingStep - 1) * pp || OrderStopLoss() == 0)
		//            {


		//                fm = OrderModify(OrderTicket(), OrderOpenPrice(), pAsk + TrailingStop * pp, OrderTakeProfit(), DateTime.Now, Color.Red);
		//                return;
		//            }
		//        }
		//    }
		//}

		//public void TrailOrders()
		//{
		//    int current_order;
		//    for (current_order = 0; current_order < OrdersTotal(); current_order++)
		//    {
		//        OrderSelect(current_order, SELECT_BY_POS, MODE_TRADES);

		//        if (OrderSymbol() == symbol && OrderType() == OP_BUY && OrderMagicNumber() == magickNumber)
		//        {
		//            double new_take_profit;
		//            double stop_loss = 0;
		//            double new_stop_loss = stop_loss;
		//            double eStopLoss = 0;
		//            if (eStopLoss <= 0.0)
		//                new_stop_loss = 0;
		//            new_take_profit = OrderTakeProfit();
		//            if (new_take_profit != OrderTakeProfit() || new_stop_loss != OrderStopLoss())
		//            {
		//                OrderModify(OrderTicket(), OrderOpenPrice(), new_stop_loss, new_take_profit, DateTime.Now, Color.Pink);

		//                double ticket = 0;
		//                ObjectCreate(DoubleToStr(ticket, 0), OBJ_TEXT, 0, iTime(symbol, PERIOD_M1, 0), OrderOpenPrice());
		//                ObjectSetText(DoubleToStr(ticket, 0), "MDF " + DoubleToStr(OrderTicket(), 0), 8, "tahoma", Color.Pink);
		//            }

		//            operationsCount++;
		//        }

		//        if (OrderSymbol() == symbol && OrderType() == OP_SELL && OrderMagicNumber() == magickNumber)
		//        {
		//            double l_price_24;
		//            double ld_184 = 0;
		//            double stop_loss_price = ld_184;
		//            double eStopLoss = 0;
		//            if (eStopLoss <= 0.0)
		//                stop_loss_price = 0;

		//            l_price_24 = OrderTakeProfit();
		//            if (l_price_24 != OrderTakeProfit() || stop_loss_price != OrderStopLoss())
		//            {
		//                OrderModify(OrderTicket(), OrderOpenPrice(), stop_loss_price, l_price_24, DateTime.Now, Color.Red);

		//                double ticket = 0;
		//                ObjectCreate(DoubleToStr(ticket, 0), OBJ_TEXT, 0, iTime(symbol, PERIOD_M1, 0), OrderOpenPrice());
		//                ObjectSetText(DoubleToStr(ticket, 0), "MDF " + DoubleToStr(OrderTicket(), 0), 8, "tahoma", Color.Pink);
		//            }
		//            operationsCount++;
		//        }
		//    }
		//}

		//public void TrailingAlls(int trail = 10)
		//{
		//    double TakeProfit = 20;
		//    double Point = 0.00020;
		//    double stopcrnt;
		//    double stopcal;
		//    int trade;
		//    int trades = OrdersTotal();
		//    double profitcalc;

		//    if (trail == 0)
		//        return;

		//    for (trade = 0; trade < trades; trade++)
		//    {
		//        OrderSelect(trade, SELECT_BY_POS, MODE_TRADES);
		//        if (OrderSymbol() == symbol)
		//        {
		//            if (OrderType() == OP_BUY)
		//            {
		//                stopcrnt = OrderStopLoss();
		//                stopcal = Bid - (trail * Point);

		//                profitcalc = OrderTakeProfit() + (TakeProfit * Point);
		//                if (stopcrnt == 0)
		//                {
		//                    OrderModify(OrderTicket(), OrderOpenPrice(), stopcal, profitcalc, DateTime.Now, Color.Blue);
		//                }
		//                else if (stopcal > stopcrnt)
		//                {
		//                    OrderModify(OrderTicket(), OrderOpenPrice(), stopcal, profitcalc, DateTime.Now, Color.Blue);
		//                }
		//            }

		//            if (OrderType() == OP_SELL)
		//            {
		//                stopcrnt = OrderStopLoss();
		//                stopcal = Ask + (trail * Point);
		//                profitcalc = OrderTakeProfit() - (TakeProfit * Point);
		//                if (stopcrnt == 0)
		//                {
		//                    OrderModify(OrderTicket(), OrderOpenPrice(), stopcal, profitcalc, DateTime.Now, Color.Red);
		//                }
		//                else if (stopcal < stopcrnt)
		//                {
		//                    OrderModify(OrderTicket(), OrderOpenPrice(), stopcal, profitcalc, DateTime.Now, Color.Red);
		//                }
		//            }
		//        }
		//    }
		//}
		//} 


	}
}
