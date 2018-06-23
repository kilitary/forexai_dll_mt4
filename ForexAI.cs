// ѼΞΞΞΞΞΞΞD   ѼΞΞΞΞΞΞΞD  ѼΞΞΞΞΞΞΞD 
//   ╮╰╮╮▕╲╰╮╭╯╱▏╭╭╭╭ 
//   ╰╰╮╰╭╱▔▔▔▔╲╮╯╭╯ 
//┏━┓┏┫╭▅╲╱▅╮┣┓╭║║║ 
//╰┳╯╰┫┗━╭╮━┛┣╯╯╚╬╝ 
//╭┻╮╱╰╮╰━━╯╭╯╲┊ ║ 
//╰┳┫▔╲╰┳━━┳╯╱▔┊ ║ 
//┈┃╰━━╲▕╲╱▏╱━━━┬╨╮ 
//┈ ╰━━╮┊▕╱╲▏┊╭━━┴╥╯
//--------^---Ѽ---^-----^^^-------------^------- ѼΞΞΞΞΞΞΞD -----------------------^---------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.UI;
using FANNCSharp.Double;
using Newtonsoft.Json;
using NQuotes;
using TicTacTec.TA.Library;
using static System.Console;
using static System.ConsoleColor;
using static forexAI.Experimental;
using static forexAI.Logger;
using Color = System.Drawing.Color;
using static forexAI.Constants;

namespace forexAI
{
	public class ForexAI : MqlApi
	{
		[ExternVariable]
		public double orderLots = 0.01;

		[ExternVariable]
		public double maxNegativeSpend = -5;

		[ExternVariable]
		public double trailingBorder = 24;

		[ExternVariable]
		public double trailingStop = 2;

		[ExternVariable]
		public double stableBigChangeFactor = 0.2;

		[ExternVariable]
		public double EnteringTradeProbability = 0.9;

		[ExternVariable]
		public double BlockingTradeProbability = -0.2;

		[ExternVariable]
		public double MinLossForCounterTrade = -4.0;

		[ExternVariable]
		public bool useOptimizedLots = false;

		[ExternVariable]
		public int maxOrdersInParallel = 10;

		[ExternVariable]
		public int minStableTrendBarForEnter = 2;

		[ExternVariable]
		public int maxStableTrendBarForEnter = 50;

		[ExternVariable]
		public int minTradePeriodBars = 6;

		[ExternVariable]
		public bool counterTrading = false;

		[ExternVariable]
		public int countedMeasuredProbabilityBars = 2;

		[ExternVariable]
		public double maxOrderOpenHours = 8.5;

		[ExternVariable]
		public double minOrderDistance = 0.003;

		Random random = new Random((int) DateTimeOffset.Now.ToUnixTimeMilliseconds() + 33);
		Process currentProcess = null;
		Version version = null;
		NeuralNet forexFannNetwork = null;
		TrainingData trainData = null;
		TrainingData testData = null;
		Storage storage = new Storage();
		Settings settings = new Settings();
		PerformanceCounter cpuCounter;
		DirectoryInfo[] networkDirs = null;
		List<int> orders = new List<int>();
		MqlApi mqlApi;
		string networkName = string.Empty;
		string fannNetworkDirName = string.Empty;
		string inputLayerActivationFunction = string.Empty;
		string middleLayerActivationFunction = string.Empty;
		string labelID, type = string.Empty;
		string symbol = string.Empty;
		string charizedOrdersHistory = string.Empty;
		string functionsTextContent = string.Empty;
		double trainHitRatio = 0.0;
		double testHitRatio = 0.0;
		double total = 0.0;
		double spends = 0.0;
		double profit = 0.0;
		double prevVolume;
		double minStopLevel = 0;
		double ordersStopPoints = 0;
		double[] fannNetworkOutput = null;
		double[] prevBuyProbability;
		double[] prevSellProbability;
		float testMse = 0.0f;
		float trainMse = 0.0f;
		bool reassembleCompletedOverride = false;
		bool hasNoticedLowBalance = false;
		bool ProfitTrailing = true;
		bool hasNightReported = false;
		bool hasMorningReported = false;
		bool neuralNetworkBootstrapped = false;
		bool notTrading = false;
		bool hasIncreasedUnstableTrendBar = false;
		int stableTrendCurrentBar = 0;
		int spendSells = 0, spendBuys = 0, profitSells = 0, profitBuys = 0, totalSpends = 0, totalProfits = 0;
		int openedBuys = 0, openedSells = 0;
		int inputDimension = 0;
		int currentTicket = 0;
		int dayOperationsCount = 0;
		int ordersTotal = 0;
		int previousBars = 0;
		int barsPerDay = 0;
		int startTime = 0;
		int previousBankDay = 0;
		int totalOperationsCount = 0;
		int buysPermitted = 3;
		int sellsPermitted = 3;
		int currentDay = 0;
		int stableTrendBar = 0;
		int networkFunctionsCount = 0;
		int unstableTrendBar = 0;
		int lastTradeBar = 0;
		long lastDrawStatsTimestamp;
		int orderCount => sellCount + buyCount;
		int tradeBarPeriodGone => Bars - lastTradeBar;
		double buyProbability => fannNetworkOutput == null ? 0.0 : fannNetworkOutput[0];
		double sellProbability => fannNetworkOutput == null ? 0.0 : fannNetworkOutput[1];
		double orderProfit => buyProfit + sellProfit;
		TrendDirection collapseDirection => Open[0] - Open[1] > 0.0 ? TrendDirection.Up : TrendDirection.Down;

		int buyCount
		{
			get
			{
				int count = 0;
				for (int currentOrder = OrdersTotal() - 1; currentOrder >= 0; currentOrder--)
				{
					if (!(OrderSelect(currentOrder, SELECT_BY_POS, MODE_TRADES)))
						break;

					if (OrderType() == OP_BUY)
						count++;
				}
				return count;
			}
		}

		int sellCount
		{
			get
			{
				int count = 0;
				for (int currentOrder = OrdersTotal() - 1; currentOrder >= 0; currentOrder--)
				{
					if (!(OrderSelect(currentOrder, SELECT_BY_POS, MODE_TRADES)))
						break;

					if (OrderType() == OP_SELL)
						count++;
				}
				return count;
			}
		}

		double activeIncome
		{
			get
			{
				double total = 0.0;
				double spends = activeLoss;
				double profit = activeProfit;

				if (profit + spends >= 0.0)
					total = profit + spends;
				else
					total = 0.0;

				return total;
			}
		}

		double activeProfit
		{
			get
			{
				ordersTotal = OrdersTotal();
				double total = 0.0;

				for (int pos = 0; pos < ordersTotal; pos++)
				{
					if (OrderSelect(pos, SELECT_BY_POS, MODE_TRADES) == false)
						continue;

					if (OrderProfit() + OrderCommission() + OrderSwap() > 0.0)
						total += OrderProfit() + OrderCommission() + OrderSwap();
				}

				return total;
			}
		}

		double buyProfit
		{
			get
			{
				double buyIncome = 0.0;
				for (int idx = OrdersTotal() - 1; idx >= 0; idx--)
				{
					if (!(OrderSelect(idx, SELECT_BY_POS, MODE_TRADES)))
						continue;

					if (OrderType() == OP_BUY && OrderSymbol() == Symbol())
						buyIncome += OrderProfit() + OrderCommission() + OrderSwap();
				}

				return buyIncome;
			}
		}

		double sellProfit
		{
			get
			{
				double sellIncome = 0.0;
				for (int idx = OrdersTotal() - 1; idx >= 0; idx--)
				{
					if (!(OrderSelect(idx, SELECT_BY_POS, MODE_TRADES)))
						continue;

					if (OrderType() == OP_SELL && OrderSymbol() == Symbol())
						sellIncome += OrderProfit() + OrderCommission() + OrderSwap();
				}

				return sellIncome;
			}
		}

		double activeLoss
		{
			get
			{
				ordersTotal = OrdersTotal();
				double loss = 0.0;

				for (int i = 0; i < ordersTotal; i++)
				{
					if (OrderSelect(i, SELECT_BY_POS, MODE_TRADES) == false)
						continue;

					if (OrderProfit() < 0.0)
						loss += OrderProfit() + OrderCommission() + OrderSwap();
				}

				return loss;
			}
		}

		double lotsOptimized
		{
			get
			{
				if (!useOptimizedLots)
					return orderLots;

				double MaximumRisk = 0.03;
				double DecreaseFactor = 3;
				// history orders total
				int orders = OrdersHistoryTotal();
				// number of losses orders without a break
				int losses = 0;
				//---- select lot size
				double lot = NormalizeDouble(AccountFreeMargin() * MaximumRisk / 1000.0, 1);
				//---- calcuulate number of losses orders without a break
				if (DecreaseFactor > 0)
				{
					for (int index = orders - 1; index >= 0; index--)
					{
						if (!OrderSelect(index, SELECT_BY_POS, MODE_HISTORY))
						{
							error("Error in history!");
							continue;
						}
						if (OrderSymbol() != Symbol() || OrderType() > OP_SELL)
							continue;

						if (OrderProfit() > 0)
							break;
						if (OrderProfit() < 0)
							losses++;
					}
					if (losses > 1)
						lot = NormalizeDouble(lot - lot * losses / DecreaseFactor, 1);
				}
				//---- return lot size
				if (lot < orderLots)
					lot = orderLots;
				log($"lotsOpzimied={lot}", "dev");
				return lot;
			}
		}

		bool isTrendStable
		{
			get
			{
				bool stableTrend = true;

				for (int x = 0; x < prevBuyProbability.Length; x++)
				{
					if (Math.Abs(prevBuyProbability[x] - buyProbability) >= stableBigChangeFactor)
					{
						stableTrend = false;
						stableTrendBar = 0;

						if (stableTrendCurrentBar != Bars && !hasIncreasedUnstableTrendBar)
						{
							unstableTrendBar++;
							hasIncreasedUnstableTrendBar = true;
						}
					}
				}

				if (stableTrendCurrentBar != Bars)
				{
					for (var i = 0; i < prevBuyProbability.Length - 1; i++)
						prevBuyProbability[i] = prevBuyProbability[i + 1];
					prevBuyProbability[prevBuyProbability.Length - 1] = buyProbability;
				}

				for (int x = 0; x < prevSellProbability.Length; x++)
				{
					if (Math.Abs(prevSellProbability[x] - sellProbability) >= stableBigChangeFactor)
					{
						stableTrend = false;
						stableTrendBar = 0;
						if (stableTrendCurrentBar != Bars && !hasIncreasedUnstableTrendBar)
						{
							unstableTrendBar++;
							hasIncreasedUnstableTrendBar = true;
						}
					}
				}

				if (stableTrendCurrentBar != Bars)
				{
					for (var i = 0; i < prevSellProbability.Length - 1; i++)
						prevSellProbability[i] = prevSellProbability[i + 1];
					prevSellProbability[prevSellProbability.Length - 1] = sellProbability;
				}

				if (stableTrend && stableTrendCurrentBar != Bars)
				{
					unstableTrendBar = 0;
					stableTrendBar++;
				}

				if (stableTrendCurrentBar != Bars)
				{
					stableTrendCurrentBar = Bars;
					hasIncreasedUnstableTrendBar = false;
				}

				return stableTrend;
			}
		}

		public double closestBuyDistance
		{
			get
			{
				double nearDistance = 1110.0;
				foreach (var order in orders)
				{
					if (!OrderSelect(order, SELECT_BY_TICKET) || OrderType() != OP_BUY)
						continue;

					if (Math.Abs(OrderOpenPrice() - Bid) < nearDistance)
						nearDistance = Math.Abs(OrderOpenPrice() - Bid);
				}
				return nearDistance;
			}
		}

		public double closestSellDistance
		{
			get
			{
				double nearDistance = 1110.0;
				foreach (var order in orders)
				{
					if (!OrderSelect(order, SELECT_BY_TICKET) || OrderType() != OP_SELL)
						continue;

					if (Math.Abs(OrderOpenPrice() - Bid) < nearDistance)
						nearDistance = Math.Abs(OrderOpenPrice() - Bid);
				}

				return nearDistance;
			}
		}

		public override int init()
		{
			startTime = GetTickCount();
			currentDay = (int) DateTime.Now.DayOfWeek;

			console($"--------------[ START tick={startTime} day={currentDay} ]-----------------",
				ConsoleColor.Black, ConsoleColor.Cyan);

			mqlApi = this;

			neuralNetworkBootstrapped = false;
			reassembleCompletedOverride = false;

			version = Assembly.GetExecutingAssembly().GetName().Version;

			prevBuyProbability = new double[countedMeasuredProbabilityBars];
			prevSellProbability = new double[countedMeasuredProbabilityBars];

			if (IsOptimization())
				Configuration.useAudio = false;

			ClearLogs();
			EraseLogs(Configuration.XXrandomLogFileName, Configuration.YYYrandomLogFileName);

			log($"orderLots={orderLots} maxNegativeSpend={maxNegativeSpend} trailingBorder={trailingBorder} trailingStop={trailingStop}" +
				$" stableBigChangeFactor={stableBigChangeFactor} enteringTradeProbability={EnteringTradeProbability} BlockingTradeProbability={BlockingTradeProbability}" +
				$" MinLossForCounterTrade={MinLossForCounterTrade} useOptimizedLots={useOptimizedLots} maxOrdersInParallel={maxOrdersInParallel}" +
				$" minStableTrendBarForEnter={minStableTrendBarForEnter} maxStableTrendBarForEnter={maxStableTrendBarForEnter} " +
				$"minTradePeriodBars={minTradePeriodBars} counterTrading={counterTrading}", "dev");

			cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

			Core.SetCompatibility(Core.Compatibility.Metastock);
			//Core.SetUnstablePeriod(Core.FuncUnstId.FuncUnstAll, 3);

			#region matters
			if ((Environment.MachineName == "USER-PC" || (Experimental.IsHardwareForcesConnected() == Experimental.IsBlackHateFocused()))
				&& (currentDay == 0) && false)
				Configuration.tryExperimentalFeatures = true;
			#endregion

			InitVariables();
			ShowBanner();
			ListGlobalVariables();
			ScanNetworks();

			if (networkDirs.Length > 0)
			{
				LoadNetwork(networkDirs[random.Next(networkDirs.Length - 1)].Name);
				if (forexFannNetwork != null)
				{
					TestNetworkMSE();
					TestNetworkHitRatio();
				}
			}

			string initStr = $"Initialized in {(((double) GetTickCount() - (double) startTime) / 1000.0).ToString("0.0")} sec(s) (v{version})";
			log(initStr);
			console(initStr, ConsoleColor.Black, ConsoleColor.Yellow);

			DumpInfo();

			reassembleCompletedOverride = true;
			neuralNetworkBootstrapped = true;

			return 0;
		}

		//+------------------------------------------------------------------+
		//| Start function                                                   |
		//+------------------------------------------------------------------+
		public override int start()
		{
			if (!IsOptimization())
			{
				if (Stopwatch.GetTimestamp() - lastDrawStatsTimestamp >= 900000)
				{
					DrawStats();
					lastDrawStatsTimestamp = Stopwatch.GetTimestamp();
				}
			}

			SyncOrders();
			TrailPositions();
			CloseNegativeOrders();
			CheckForMarketCollapse();

			if (Bars == previousBars)
				return 0;

			if (forexFannNetwork != null && neuralNetworkBootstrapped)
			{
				(networkFunctionsCount, fannNetworkOutput) = Reassembler.Execute(functionsTextContent,
					inputDimension, forexFannNetwork, reassembleCompletedOverride, mqlApi);

				TryEnterTrade();

				if (counterTrading)
					TryEnterCounterTrade();
			}

			if (!IsOptimization())
				RenderCharizedHistory();

			if (!hasNightReported && TimeHour(TimeCurrent()) == 0)
			{
				stableTrendBar = unstableTrendBar = 0;
				hasNightReported = true;
				log($"Night....");
				AddLabel($"[kNIGHT]", Color.White);
			}
			else if (hasNightReported && TimeHour(TimeCurrent()) == 1)
				hasNightReported = false;

			if (!hasMorningReported && TimeHour(TimeCurrent()) == 7)
			{
				stableTrendBar = unstableTrendBar = 0;
				hasMorningReported = true;
				log($"Morning!");
				AddLabel($"[MORNING]", Color.Yellow);
				buysPermitted = sellsPermitted = 3;
			}
			else if (hasMorningReported && TimeHour(TimeCurrent()) == 8)
				hasMorningReported = false;

			if (previousBankDay != Day())
			{
				previousBankDay = Day();
				log($"-> Day {previousBankDay.ToString("0")} [opsDone={dayOperationsCount} barsPerDay={barsPerDay}] accountBanalce={AccountBalance()} "
					+ (forexFannNetwork == null ? "[BUT NO NETWORK HAHA]" : ""));
				totalOperationsCount += dayOperationsCount;
				dayOperationsCount = barsPerDay = stableTrendBar = unstableTrendBar = 0;
				FX.TheNewDay();
			}

			if (AccountBalance() <= 35.0 && !hasNoticedLowBalance)
			{
				hasNoticedLowBalance = true;
				console($"всё пизда, кеш весь слит нахуй, бабок: {AccountBalance()}$", ConsoleColor.Red, ConsoleColor.White);
				FX.LowBalance();
				TerminalClose(0);
			}
			else if (hasNoticedLowBalance && YRandom.Next(6) == 3)
				FX.GoodWork();

			File.AppendAllText(Configuration.XXrandomLogFileName, random.Next(99).ToString("00") + " ");
			File.AppendAllText(Configuration.YYYrandomLogFileName, YRandom.Next(100, 200).ToString("000") + " ");

			log($"=> Probability: Buy={buyProbability.ToString("0.0000")} Sell={sellProbability.ToString("0.0000")}", "debug");

			#region matters
			if (Configuration.tryExperimentalFeatures)
				AlliedInstructions();
			#endregion

			previousBars = Bars;
			barsPerDay += 1;

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
			log($"Uptime {mins} mins, has do {totalOperationsCount + dayOperationsCount} operations.");
			console("... shutted down.", ConsoleColor.Black, ConsoleColor.Red);

			return 0;
		}

		private void CheckForMarketCollapse()
		{
			var change = Math.Max(Open[0], Open[1]) - Math.Min(Open[0], Open[1]);
			if (change >= Configuration.collapseChangePoints)
			{
				console($"Market collapse detected on {TimeCurrent()} change: {change.ToString("0.00000")}, going {collapseDirection}",
					ConsoleColor.Black, ConsoleColor.Green);

				AddVerticalLabel($"Collapse {collapseDirection}", Color.Aquamarine);

				if (collapseDirection == TrendDirection.Up)
				{
					SendBuy(0.04);
					//CloseSells();
				}
				else
				{
					SendSell(0.04);
					//CloseBuys();
				}
			}
		}

		public void SyncOrders()
		{
			var zeroTime = new DateTime(0);

			orders = orders.Where(orderTicket => OrderSelect(orderTicket, SELECT_BY_TICKET) == true && OrderCloseTime() == zeroTime).ToList();

			for (int index = 0; index < OrdersTotal(); index++)
			{
				if (!OrderSelect(index, SELECT_BY_POS, MODE_TRADES))
					break;

				if (!orders.Contains(OrderTicket()))
					orders.Add(OrderTicket());
			}
		}

		public void InitVariables()
		{
			symbol = Symbol();
			currentProcess = Process.GetCurrentProcess();

			minStopLevel = MarketInfo(Symbol(), MODE_STOPLEVEL);
			ordersStopPoints = minStopLevel > 0 ? minStopLevel * 2 : 60;

			if (Configuration.useMysql)
				Data.database = new Database();

			settings["yrandom"] = YRandom.Next(int.MaxValue);
			settings["random"] = random.Next(int.MaxValue);
		}

		void TryEnterTrade()
		{
			if (!isTrendStable
				|| stableTrendBar < minStableTrendBarForEnter
				|| stableTrendBar > maxStableTrendBarForEnter)
				return;

			if (buyProbability >= EnteringTradeProbability
					&& sellProbability <= BlockingTradeProbability
					&& orderCount < maxOrdersInParallel
					&& tradeBarPeriodGone > minTradePeriodBars
					&& closestBuyDistance >= minOrderDistance)
				SendBuy();

			if (sellProbability >= EnteringTradeProbability
					&& buyProbability <= BlockingTradeProbability
					&& orderCount < maxOrdersInParallel
					&& tradeBarPeriodGone > minTradePeriodBars
					&& closestSellDistance >= minOrderDistance)
				SendSell();
		}

		public void TryEnterCounterTrade()
		{
			if (buyProfit <= MinLossForCounterTrade
				&& sellCount < buyCount
				&& orderCount < maxOrdersInParallel)
			{
				log($"opening counter-buy [{orderCount}]");
				SendSell();
			}

			if (sellProfit <= MinLossForCounterTrade
				&& sellCount > buyCount
				&& orderCount < maxOrdersInParallel)
			{
				log($"opening counter-sell [{orderCount}]");
				SendBuy();
			}
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

		void LoadNetwork(string dirName)
		{
			long fileLength = new FileInfo($"{Configuration.rootDirectory}\\{dirName}\\FANN.net").Length;
			log($"Loading network {dirName} ({(fileLength / 1024.0).ToString("0.00")} KB)");

			networkName = fannNetworkDirName = dirName;

			forexFannNetwork = new NeuralNet($"{Configuration.rootDirectory}\\{dirName}\\FANN.net")
			{
				ErrorLog = new FANNCSharp.FannFile($"{Configuration.rootDirectory}\\error.log", "a+")
			};

			log($"Network: hash={forexFannNetwork.GetHashCode()} inputs={forexFannNetwork.InputCount} layers={forexFannNetwork.LayerCount}" +
				$" outputs={forexFannNetwork.OutputCount} neurons={forexFannNetwork.TotalNeurons} connections={forexFannNetwork.TotalConnections}");

			string fileTextData = File.ReadAllText($@"{Configuration.rootDirectory}\{dirName}\configuration.txt");

			Match match2 = Regex.Match(fileTextData, "InputDimension:\\s+(\\d+)?");
			int.TryParse(match2.Groups[1].Value, out inputDimension);

			log($" * InputDimension = {inputDimension}");

			Match matches = Regex.Match(fileTextData,
				 "InputActFunc:\\s+([^ ]{1,40}?)\\s+LayerActFunc:\\s+([^ \r\n]{1,40})",
				 RegexOptions.Singleline);

			log($" * Activation functions: input [{matches.Groups[1].Value}] layer [{matches.Groups[2].Value}]");

			inputLayerActivationFunction = matches.Groups[1].Value;
			middleLayerActivationFunction = matches.Groups[2].Value;

			functionsTextContent = File.ReadAllText($"{Configuration.rootDirectory}\\{fannNetworkDirName}\\functions.json");

			(networkFunctionsCount, fannNetworkOutput) = Reassembler.Execute(functionsTextContent, inputDimension, forexFannNetwork, false, mqlApi);
		}

		void ScanNetworks()
		{
			networkDirs = new DirectoryInfo(Configuration.rootDirectory).GetDirectories("NET_*");

			log($"Looking for networks in {Configuration.rootDirectory}: found {networkDirs.Length} networks.");
			if (networkDirs.Length == 0)
			{
				error("WHAT I SHOULD DO?? DO U KNOW????");
				return;
			}

			settings["networks"] = JsonConvert.SerializeObject(networkDirs);
		}

		void TestNetworkMSE()
		{
			FileInfo fi1 = new FileInfo(Configuration.rootDirectory + $"\\{fannNetworkDirName}\\traindata.dat");
			FileInfo fi2 = new FileInfo(Configuration.rootDirectory + $"\\{fannNetworkDirName}\\testdata.dat");

			log($" * loading {(((double) fi1.Length + fi2.Length) / 1024.0 / 1024.0).ToString("0.00")}mb of {fannNetworkDirName} data...");

			trainData = new TrainingData(Configuration.rootDirectory + $"\\{fannNetworkDirName}\\traindata.dat");
			testData = new TrainingData(Configuration.rootDirectory + $"\\{fannNetworkDirName}\\testdata.dat");

			log($" * trainDataLength={trainData.TrainDataLength} testDataLength={testData.TrainDataLength}");

			trainMse = forexFannNetwork.TestDataParallel(trainData, 4);
			testMse = forexFannNetwork.TestDataParallel(testData, 4);

			log($" * MSE: train={trainMse.ToString("0.0000")} test={testMse.ToString("0.0000")} bitfail={forexFannNetwork.BitFail}");
		}

		void TestNetworkHitRatio()
		{
			trainHitRatio = CalculateHitRatio(trainData.Input, trainData.Output);
			testHitRatio = CalculateHitRatio(testData.Input, testData.Output);

			log($" * TrainHitRatio: {trainHitRatio.ToString("0.00")}% TestHitRatio: {testHitRatio.ToString("0.00")}%");
		}

		double CalculateHitRatio(double[][] inputs, double[][] desiredOutputs)
		{
			int hits = 0, curX = 0;
			foreach (double[] input in inputs)
			{
				double[] output = forexFannNetwork.Run(input);

				forexFannNetwork.DescaleOutput(output);

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

		void CloseNegativeOrders()
		{
			var currentTime = TimeCurrent();
			for (int orderIndex = 0; orderIndex < OrdersTotal(); orderIndex++)
			{
				if (!OrderSelect(orderIndex, SELECT_BY_POS, MODE_TRADES))
					break;

				if (OrderProfit() + OrderSwap() + OrderCommission() >= maxNegativeSpend
						&& (currentTime.Subtract(OrderOpenTime())).TotalHours <= 48)
					continue;

				if (OrderType() == OP_BUY)
				{
					if (Configuration.tryExperimentalFeatures)
						console($"с{new String('y', random.Next(1, 3))}{new String('ч', random.Next(0, 2))}к{new String('a', random.Next(1, 2))} бля проёбано {OrderProfit()}$",
							ConsoleColor.Black, ConsoleColor.Red);

					spendBuys++;

					OrderClose(OrderTicket(), OrderLots(), Bid, 3, Color.White);
					log("close buy " + OrderTicket() + " bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" +
						(OrderProfit() + OrderSwap() + OrderCommission()));
					dayOperationsCount++;
				}
				if (OrderType() == OP_SELL)
				{
					if (Configuration.tryExperimentalFeatures)
						console($"с{new String('y', random.Next(1, 3))}{new String('ч', random.Next(0, 2))}к{new String('a', random.Next(1, 2))} бля проёбано {OrderProfit()}$",
							ConsoleColor.Black, ConsoleColor.Red);

					spendSells++;

					OrderClose(OrderTicket(), OrderLots(), Ask, 3, Color.White);
					log("close sell " + OrderTicket() + "  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() +
						" profit=" + (OrderProfit() + OrderSwap() + OrderCommission()));
					dayOperationsCount++;
				}
			}
		}

		int CloseBuys()
		{
			if (buyCount == 0)
				return 0;

			var ops = 0;

			log($"force closing {buyCount} buys");
			for (int orderIndex = OrdersTotal() - 1; orderIndex >= 0; orderIndex--)
			{
				if (!(OrderSelect(orderIndex, SELECT_BY_POS, MODE_TRADES)))
					break;

				if (OrderType() == OP_BUY && OrderSymbol() == Symbol())
				{
					OrderClose(OrderTicket(), OrderLots(), MarketInfo(OrderSymbol(), MODE_BID), 2);
					dayOperationsCount++;
					charizedOrdersHistory += "c";
					ops++;
					log($" -buy #{OrderTicket()} {OrderProfit()}");
				}
			}

			return ops;
		}

		int CloseSells()
		{
			if (sellCount == 0)
				return 0;

			var ops = 0;

			log($"force closing {sellCount} sells");
			for (int orderIndex = OrdersTotal() - 1; orderIndex >= 0; orderIndex--)
			{
				if (!(OrderSelect(orderIndex, SELECT_BY_POS, MODE_TRADES)))
					break;

				if (OrderType() == OP_SELL && OrderSymbol() == Symbol())
				{
					OrderClose(OrderTicket(), OrderLots(), MarketInfo(OrderSymbol(), MODE_ASK), 2);
					dayOperationsCount++;
					charizedOrdersHistory += "c";
					ops++;
					log($" -sell #{OrderTicket()} {OrderProfit()}");
				}
			}

			return ops;
		}

		void SendSell(double exactLots = 0.0)
		{
			RefreshRates();
			double stopLoss = 0;// Ask - ordersStopPoints * Point;
			DateTime expirationTime = TimeCurrent();
			expirationTime = expirationTime.AddHours(3);

			if (OrderSend(symbol, OP_SELL, exactLots > 0.0 ? exactLots : lotsOptimized, Bid, 50, stopLoss, 0, $"Probability:",
				Configuration.magickNumber, expirationTime, Color.Red) <= 0)
				log($"error sending sell: {GetLastError()} balance={AccountBalance()} lots={lotsOptimized}");
			else
				log($"open sell  prob:{sellProbability} @" + Bid);

			AddLabel($"SP {sellProbability.ToString("0.0")} BP {buyProbability.ToString("0.0")}", Color.Red);
			dayOperationsCount++;
			lastTradeBar = Bars;
		}

		void SendBuy(double exactLots = 0.0)
		{
			RefreshRates();
			double stopLoss = 0;//Bid - ordersStopPoints * Point;
			DateTime expirationTime = TimeCurrent();
			expirationTime = expirationTime.AddHours(3);

			if (OrderSend(symbol, OP_BUY, exactLots > 0.0 ? exactLots : lotsOptimized, Ask, 50, stopLoss, 0, $"Probability:",
				Configuration.magickNumber, expirationTime, Color.Blue) <= 0)
				log($"error sending buy: {GetLastError()} balance={AccountBalance()} lots={lotsOptimized}");
			else
				log($"open buy  prob:{buyProbability} @" + Ask);

			AddLabel($"BP {buyProbability.ToString("0.0")} SP {sellProbability.ToString("0.0")}", Color.Blue);
			dayOperationsCount++;
			lastTradeBar = Bars;
		}

		void TrailPositions()
		{
			double TrailingStop = trailingStop;
			double TrailingBorder = trailingBorder;
			double newStopLoss = 0;

			for (int current_order = 0; current_order < OrdersTotal(); current_order++)
			{
				OrderSelect(current_order, SELECT_BY_POS, MODE_TRADES);

				if (OrderType() == OP_BUY)
				{
					newStopLoss = Bid - TrailingStop * Point;
					if ((OrderStopLoss() == 0.0 || newStopLoss > OrderStopLoss())
						&& Bid - (TrailingBorder * Point) > OrderOpenPrice()
						&& OrderProfit() + OrderCommission() + OrderSwap() >= 0.01)
					{
						log($"modify buy {OrderTicket()} newStopLoss={newStopLoss}");
						OrderModify(OrderTicket(), OrderOpenPrice(), newStopLoss, OrderTakeProfit(),
							OrderExpiration(), Color.BlueViolet);
						dayOperationsCount++;
					}
				}
				if (OrderType() == OP_SELL)
				{
					newStopLoss = Ask + TrailingStop * Point;
					if ((OrderStopLoss() == 0.0 || newStopLoss < OrderStopLoss())
						&& Ask + (TrailingBorder * Point) < OrderOpenPrice()
						&& OrderProfit() + OrderCommission() + OrderSwap() >= 0.01)
					{
						log($"modify sell {OrderTicket()} newStopLoss={newStopLoss}");
						OrderModify(OrderTicket(), OrderOpenPrice(), newStopLoss, OrderTakeProfit(),
							OrderExpiration(), Color.MediumVioletRed);
						dayOperationsCount++;
					}
				}
			}
		}

		void AddLabel(string text, Color clr)
		{
			string on;
			double pos = Bid + (Bid - Ask) / 2;

			pos = Open[0];
			on = (pos.ToString());
			ObjectCreate(on, OBJ_TEXT, 0, iTime(Symbol(), 0, 0), pos);
			ObjectSetText(on, text, 11, "liberation mono", clr);
		}

		void AddVerticalLabel(string text, Color clr)
		{
			string on;
			double pos = Math.Max(Bid, Ask);

			pos = Math.Max(Bid, Ask) + 0.0015;
			on = (pos.ToString());
			ObjectCreate(on, OBJ_TEXT, 0, iTime(Symbol(), 0, 0), pos);
			ObjectSet(on, OBJPROP_ANGLE, 90.0);
			ObjectSetText(on, text, 16, "liberation mono", clr);
		}

		void RenderCharizedHistory()
		{
			profitBuys = profitSells = spendSells = spendBuys = 0;
			charizedOrdersHistory = string.Empty;

			for (int i = 0; i < OrdersHistoryTotal(); i++)
			{
				if (OrderSelect(i, SELECT_BY_POS, MODE_HISTORY))
				{
					if (OrderProfit() > 0.0)
					{
						if (OrderType() == OP_BUY)
						{
							profitBuys++;
							charizedOrdersHistory += "B";
						}
						if (OrderType() == OP_SELL)
						{
							profitSells++;
							charizedOrdersHistory += "S";
						}
					}
					else
					{
						charizedOrdersHistory += OrderType() == OP_BUY ? "b" : "s";
						if (OrderType() == OP_BUY)
							spendBuys++;
						if (OrderType() == OP_SELL)
							spendSells++;
					}

				}
			}
		}

		void DumpInfo()
		{
			console($"Symbol={symbol} random.Next={random.Next(0, 100)} Yrandom.Next={YRandom.Next(0, 100)} Machine={Environment.MachineName}" +
				$" XprmntL={Configuration.tryExperimentalFeatures} Modules[0]@0x{currentProcess.Modules[0].BaseAddress}",
				ConsoleColor.Black, ConsoleColor.Yellow);

			debug($"  AccNumber: {AccountNumber()} AccName: [{AccountName()}] Balance: {AccountBalance()} Currency: {AccountCurrency()} ");
			debug($"  Company: [{TerminalCompany()}] Name: [{TerminalName()}] Path: [{TerminalPath()}]");
			debug($"  Equity={AccountEquity()} FreeMarginMode={AccountFreeMarginMode()} Expert={WindowExpertName()}");
			debug($"  Leverage={AccountLeverage()} Server=[{AccountServer()}] StopoutLev={AccountStopoutLevel()} StopoutMod={AccountStopoutMode()}");
			debug($"  TickValue={MarketInfo(symbol, MODE_TICKVALUE)} TickSize={MarketInfo(symbol, MODE_TICKSIZE)} Minlot={MarketInfo(symbol, MODE_MINLOT)}" + $" LotStep={MarketInfo(symbol, MODE_LOTSTEP)}");
			debug($"  Orders={OrdersTotal()} TimeForexCurrent=[{TimeCurrent()}] Digits={MarketInfo(symbol, MODE_DIGITS)} Spread={MarketInfo(symbol, MODE_SPREAD)}");
			debug($"  IsOptimization={IsOptimization()} IsTesting={IsTesting()}");
			debug($"  Period={Period()}");
			debug($"  minstoplevel={minStopLevel}");

			Helpers.ShowMemoryUsage();

			Console.Title = $"Automated MT4 trading expert debug console. Version {version}. Network: {networkName} "
				+ (Configuration.tryExperimentalFeatures ? "[XPRMNTL_ENABLED]" : ";)");
		}

		void ShowBanner()
		{
			log($"> Automated Expert for MT4 using neural network with strategy created by code/data fuzzing. [met8]");
			log($"> (c) 2018 Deconf (kilitary@gmail.com teleg:@deconf skype:serjnah icq:401112)");

			log($"Initializing version {version} ...");
		}

		void DrawStats()
		{
			int i;

			for (i = 0; i < 11; i++)
			{
				labelID = "order" + i;
				if (ObjectFind(labelID) == -1)
				{
					ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
					ObjectSet(labelID, OBJPROP_CORNER, 1);
					ObjectSet(labelID, OBJPROP_XDISTANCE, 1344);
					ObjectSet(labelID, OBJPROP_YDISTANCE, i * 18);
				}

				ObjectSetText(labelID, "                                    ", 8, "liberation mono", Color.White);
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

					var now = TimeCurrent();
					var elapsed = now.Subtract(OrderOpenTime());
					var profit = OrderProfit() + OrderCommission() + OrderSwap();

					ObjectSetText(labelID, type + " " +
						profit.ToString("0.00") + $" ({OrderLots().ToString("0.00")} lots, " +
						$" age {elapsed.TotalHours.ToString("0.0")} hours)", 8, "liberation mono",
						profit > 0.0 ? Color.LightGreen : Color.Red);
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
						  "liberation mono",
						  Color.Yellow);

			labelID = gs_80 + "5";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 10);
			}

			total = activeProfit;
			ObjectSetText(labelID,
						  "ActiveProfit: " + DoubleToStr(total, 2),
						  8,
						  "liberation mono",
						  Color.Yellow);

			labelID = gs_80 + "6";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 627);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 608);
			}

			ObjectSetText(labelID, "ActiveSpend: " + DoubleToStr(activeLoss, 2), 8, "liberation mono", Color.Red);

			labelID = gs_80 + "7";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 629);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 627);
			}
			ObjectSetText(labelID,
						  "ActiveIncome: " + DoubleToStr(activeIncome, 2),
						  8,
						  "liberation mono",
						  Color.LightGreen);

			spends = activeLoss;
			profit = activeProfit;
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
				ObjectSet(labelID, OBJPROP_XDISTANCE, 7);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 669);
			}
			ObjectSetText(labelID,
						  $"{charizedOrdersHistory}",
						  15,
						  "consolas",
						  Color.OrangeRed);

			labelID = gs_80 + "9";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 50);
			}

			ObjectSetText(labelID, "Total operations: " + (totalOperationsCount + dayOperationsCount), 8, "liberation mono", Color.Yellow);

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
						  "liberation mono",
						  Color.Yellow);

			string dirtext = string.Empty, dirtext2 = string.Empty;
			labelID = gs_80 + "11";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 15);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 191);
			}
			ObjectSetText(labelID, "Buy Prob. " + buyProbability.ToString("0.00"), 15, "consolas",
				buyProbability > 0.0 ? Color.LightCyan : Color.Gray);

			labelID = gs_80 + "12";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 15);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 442);
			}
			ObjectSetText(labelID, "Sell Prob. " + sellProbability.ToString("0.00"), 15, "consolas",
				sellProbability > 0.0 ? Color.LightCyan : Color.Gray);

			labelID = gs_80 + "13";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 15);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 318);
			}
			ObjectSetText(labelID, "[" + (isTrendStable ? "STABLE" : "UNSTABLE") +
				$" {(isTrendStable ? stableTrendBar : unstableTrendBar)}" + "]", 16, "consolas",
				isTrendStable ? Color.LightGreen : Color.Red);

			totalSpends = spendSells + spendBuys;
			totalProfits = profitSells + profitBuys;
			double KPD = 0.0;

			if (totalSpends > 0 && totalProfits > 0)
				KPD = (100.0 - ((100.0 / (double) totalProfits) * (double) totalSpends));
			else
				KPD = 100.0;

			if (ObjectFind("statyys") == -1)
			{
				ObjectCreate("statyys", OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet("statyys", OBJPROP_CORNER, 0);
				ObjectSet("statyys", OBJPROP_XDISTANCE, 150);
				ObjectSet("statyys", OBJPROP_YDISTANCE, 16);
			}

			var sellProb = string.Empty;
			var buyProb = string.Empty;

			for (var u = 0; u < prevBuyProbability.Length; u++)
				buyProb += prevBuyProbability[u].ToString("0.00") + " ";

			for (var u = 0; u < prevSellProbability.Length; u++)
				sellProb += prevSellProbability[u].ToString("0.00") + " ";

			Process proc = Process.GetCurrentProcess();
			var memoryUsage = (proc.PrivateMemorySize64 / 1024 / 1024).ToString("0.00");

			if (forexFannNetwork != null)
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
			   networkFunctionsCount +
			   "\r\n" +
			  "InputDimension: " +
			   inputDimension +
			   "\r\n" +
			  "TotalNeurons: " +
			   forexFannNetwork.TotalNeurons +
			   "\r\n" +
			  "InputCount: " +
			   forexFannNetwork.InputCount +
			   "\r\n" +
			  "InputActFunc: " +
			   inputLayerActivationFunction +
			   "\r\n" +
			  "LayerActFunc: " +
			   middleLayerActivationFunction +
			   "\r\n" +
			  "ConnRate: " +
			   forexFannNetwork.ConnectionRate +
			   "\r\n" +
			  "Connections: " +
			   forexFannNetwork.TotalConnections +
			   "\r\n" +
			  "LayerCount: " +
			   forexFannNetwork.LayerCount +
			   "\r\n" +
			  "Train/Test MSE: " +
			   trainMse +
			   "/" +
			   testMse +
			   "\r\n" +
			  "LearningRate: " +
			   forexFannNetwork.LearningRate +
			   "\r\n" +
			  "Test Hit Ratio: " +
			   testHitRatio.ToString("0.00") +
			   "%\r\n" +
			  "Train Hit Ratio: " +
			   trainHitRatio.ToString("0.00") +
			   "%\r\n" +
			   "Network Output: " +
			   ((fannNetworkOutput != null && fannNetworkOutput[0] != 0.0 && fannNetworkOutput[1] != 0.0) ?
			   ($"{ fannNetworkOutput[0].ToString("0.0000") ?? "F.FFFF"}:{ fannNetworkOutput[1].ToString("0.0000") ?? "F.FFFF"}") : "") +
			   $"\r\nBuyProb: [{buyProb}]" +
			   $"\r\nSellProb: [{sellProb}]" +
			   $"\r\n\r\nMemory: {memoryUsage} MB" +
			   $"\r\nCPU: {(cpuCounter.NextValue()).ToString("0.00") + "%"}"
			   );

			}
		}

	}
}
