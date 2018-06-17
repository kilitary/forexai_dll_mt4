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

namespace forexAI
{
	public class ForexAI : MqlApi
	{
		[ExternVariable]
		public double orderLots = 0.01;

		[ExternVariable]
		public double maxNegativeSpend = -6.5;

		[ExternVariable]
		public double trailingBorder = 30;

		[ExternVariable]
		public double trailingStop = 20;

		[ExternVariable]
		public double stableBigChangeFactor = 0.3;

		[ExternVariable]
		public double EnteringTradeProbability = 0.7;

		[ExternVariable]
		public double BlockingTradeProbability = -0.2;

		[ExternVariable]
		public double MinLossForCounterTrade = -4.0;

		[ExternVariable]
		public bool useOptimizedLots = true;

		[ExternVariable]
		public int maxOrdersInParallel = 6;

		[ExternVariable]
		public int minStableTrendBarForEnter = 2;

		[ExternVariable]
		public int maxStableTrendBarForEnter = 100;

		[ExternVariable]
		public int minTradePeriodBars = 4;

		Random random = new Random((int) DateTimeOffset.Now.ToUnixTimeMilliseconds() + 33);
		Process currentProcess = null;
		Version version = null;
		NeuralNet forexNetwork = null;
		TrainingData trainData = null;
		TrainingData testData = null;
		Storage storage = new Storage();
		Settings settings = new Settings();
		DirectoryInfo[] networkDirs = null;
		List<int> orders = new List<int>();
		static public MqlApi mqlApi;
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
		double[] networkOutput = null;
		double[] prevBuyProbability = new double[3];
		double[] prevSellProbability = new double[3];
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

		int tradeBarPeriodGone => Bars - lastTradeBar;
		double buyProbability => networkOutput == null ? 0.0 : networkOutput[0];
		double sellProbability => networkOutput == null ? 0.0 : networkOutput[1];
		int ordersCount => sellsCount + buysCount;
		double ordersProfit => buysProfit + sellsProfit;

		int buysCount
		{
			get
			{
				int count = 0;
				for (int currentOrder = OrdersTotal() - 1; currentOrder >= 0; currentOrder--)
				{
					if (!(OrderSelect(currentOrder, SELECT_BY_POS, MODE_TRADES)))
						break;

					if (OrderType() == OP_BUY && OrderSymbol() == Symbol())
						count++;
				}
				return count;
			}
		}

		int sellsCount
		{
			get
			{
				int count = 0;
				for (int currentOrder = OrdersTotal() - 1; currentOrder >= 0; currentOrder--)
				{
					if (!(OrderSelect(currentOrder, SELECT_BY_POS, MODE_TRADES)))
						break;

					if (OrderType() == OP_SELL && OrderSymbol() == Symbol())
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
				double spends = activeSpend;
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

					if (OrderProfit() > 0.0)
						total += OrderProfit();
				}

				return total;
			}
		}

		double buysProfit
		{
			get
			{
				double buyIncome = 0.0;
				for (int idx = OrdersTotal() - 1; idx >= 0; idx--)
				{
					if (!(OrderSelect(idx, SELECT_BY_POS, MODE_TRADES)))
						continue;

					if (OrderType() == OP_BUY && OrderSymbol() == Symbol())
						buyIncome += OrderProfit();
				}

				return buyIncome;
			}
		}

		double sellsProfit
		{
			get
			{
				double sellIncome = 0.0;
				for (int idx = OrdersTotal() - 1; idx >= 0; idx--)
				{
					if (!(OrderSelect(idx, SELECT_BY_POS, MODE_TRADES)))
						continue;

					if (OrderType() == OP_SELL && OrderSymbol() == Symbol())
						sellIncome += OrderProfit();
				}

				return sellIncome;
			}
		}

		double activeSpend
		{
			get
			{
				ordersTotal = OrdersTotal();
				double total = 0.0;

				for (int i = 0; i < ordersTotal; i++)
				{
					if (OrderSelect(i, SELECT_BY_POS, MODE_TRADES) == false)
						continue;

					if (OrderProfit() < 0.0)
						total += OrderProfit();
				}

				return total;
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
					for (int i = orders - 1; i >= 0; i--)
					{
						if (!OrderSelect(i, SELECT_BY_POS, MODE_HISTORY))
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
					prevBuyProbability[0] = prevBuyProbability[1];
					prevBuyProbability[1] = prevBuyProbability[2];
					prevBuyProbability[2] = buyProbability;
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
					prevSellProbability[0] = prevSellProbability[1];
					prevSellProbability[1] = prevSellProbability[2];
					prevSellProbability[2] = sellProbability;
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

		public override int init()
		{
			currentDay = (int) System.DateTime.Now.DayOfWeek;
			neuralNetworkBootstrapped = false;
			reassembleCompletedOverride = false;
			mqlApi = this;

			ClearLogs();

			console($"--------------[ START tick={startTime = GetTickCount()} day={currentDay} ]-----------------",
				ConsoleColor.Black, ConsoleColor.Cyan);

			Core.SetCompatibility(Core.Compatibility.Metastock);
			//			Core.SetUnstablePeriod(Core.FuncUnstId.FuncUnstAll, 8);

			TruncateLog(Configuration.XXrandomLogFileName);
			TruncateLog(Configuration.YYYrandomLogFileName);

			#region matters
			if ((Environment.MachineName == "USER-PC" || (Experimental.IsHardwareForcesConnected() == Experimental.IsBlackHateFocused()))
				&& (currentDay == 0) && false)
				Configuration.tryExperimentalFeatures = true;
			#endregion

			InitVariables();
			ShowBanner();
			DumpInfo();
			ListGlobalVariables();
			ScanNetworks();

			if (networkDirs.Length > 0)
			{
				LoadNetwork(networkDirs[random.Next(networkDirs.Length - 1)].Name);
				if (forexNetwork != null)
				{
					TestNetworkMSE();
					TestNetworkHitRatio();
				}
			}

			string initStr = $"Initialized in {(((double) GetTickCount() - (double) startTime) / 1000.0).ToString("0.0")} sec(s) ";
			log(initStr);
			console(initStr, ConsoleColor.Black, ConsoleColor.Yellow);

			reassembleCompletedOverride = true;
			neuralNetworkBootstrapped = true;

			return 0;
		}

		//+------------------------------------------------------------------+
		//| Start function                                                   |
		//+------------------------------------------------------------------+
		public override int start()
		{
			DrawStats();
			TrailPositions();
			CloseNegativeOrders();
			PopulateOrders();

			if (Bars == previousBars)
				return 0;

			if (neuralNetworkBootstrapped)
			{
				networkOutput = Reassembler.Execute(functionsTextContent,
					inputDimension, Open, Close, High, Low, Volume, Bars, forexNetwork, reassembleCompletedOverride,
					TimeCurrent().ToLongDateString() + TimeCurrent().ToLongTimeString(), out networkFunctionsCount);

				EnterTrade();
				EnterCounterTrade();
			}

			RenderCharizedHistory();

			if (!hasNightReported && TimeHour(TimeCurrent()) == 0)
			{
				stableTrendBar = unstableTrendBar = 0;
				hasNightReported = true;
				console($"Night....", ConsoleColor.Black, ConsoleColor.Gray);
				AddLabel($"[kNIGHT]", Color.White);
			}
			else if (hasNightReported && TimeHour(TimeCurrent()) == 1)
				hasNightReported = false;

			if (!hasMorningReported && TimeHour(TimeCurrent()) == 7)
			{
				stableTrendBar = unstableTrendBar = 0;
				hasMorningReported = true;
				console($"Morning!", ConsoleColor.Black, ConsoleColor.Yellow);
				AddLabel($"[MORNING]", Color.Yellow);
				buysPermitted = sellsPermitted = 3;
			}
			else if (hasMorningReported && TimeHour(TimeCurrent()) == 8)
				hasMorningReported = false;

			if (previousBankDay != Day())
			{
				previousBankDay = Day();
				log($"-> Day {previousBankDay.ToString("0")} [opsDone={dayOperationsCount} barsPerDay={barsPerDay}] "
					+ (forexNetwork == null ? "[BUT NO NETWORK HAHA]" : ""));
				totalOperationsCount += dayOperationsCount;
				dayOperationsCount = barsPerDay = stableTrendBar = unstableTrendBar = 0;
				FX.TheNewDay();
			}

			log($"=> Probability: Buy={buyProbability.ToString("0.0000")} Sell={sellProbability.ToString("0.0000")}", "debug");

			File.AppendAllText(Configuration.XXrandomLogFileName, random.Next(99).ToString("00") + " ");
			File.AppendAllText(Configuration.YYYrandomLogFileName, YRandom.Next(100, 200).ToString("000") + " ");

			#region matters
			if (Configuration.tryExperimentalFeatures)
				AlliedInstructions();
			#endregion

			if (AccountBalance() <= 5.0 && !hasNoticedLowBalance)
			{
				hasNoticedLowBalance = true;
				console($"всё пизда, кеш весь слит нахуй, бабок: {AccountBalance()}$", ConsoleColor.Red, ConsoleColor.White);
				FX.LowBalance();
			}
			else if (hasNoticedLowBalance && YRandom.Next(0, 6) == 3)
				FX.GoodWork();

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

		public void PopulateOrders()
		{
			var zeroTime = new DateTime(0);

			orders = orders.Where(order => OrderSelect(order, SELECT_BY_TICKET) == true && OrderCloseTime() == zeroTime).ToList();

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

		void EnterTrade()
		{
			if (!isTrendStable
				|| stableTrendBar < minStableTrendBarForEnter
				|| stableTrendBar > maxStableTrendBarForEnter)
				return;

			if (buyProbability >= EnteringTradeProbability
					&& sellProbability <= BlockingTradeProbability
					&& ordersCount < maxOrdersInParallel
					&& tradeBarPeriodGone > minTradePeriodBars)
				SendBuy();

			if (sellProbability >= EnteringTradeProbability
					&& buyProbability <= BlockingTradeProbability
					&& ordersCount < maxOrdersInParallel
					&& tradeBarPeriodGone > minTradePeriodBars)
				SendSell();
		}

		public void EnterCounterTrade()
		{
			if (buysProfit <= MinLossForCounterTrade
				&& sellsCount < buysCount
				&& ordersCount < maxOrdersInParallel)
			{
				log($"opening counter-buy [{ordersCount}]");
				SendSell();
			}

			if (sellsProfit <= MinLossForCounterTrade
				&& sellsCount > buysCount
				&& ordersCount < maxOrdersInParallel)
			{
				log($"opening counter-sell [{ordersCount}]");
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

			forexNetwork = new NeuralNet($"{Configuration.rootDirectory}\\{dirName}\\FANN.net")
			{
				ErrorLog = new FANNCSharp.FannFile($"{Configuration.rootDirectory}\\FANN.log", "a+")
			};

			log($"Network: hash={forexNetwork.GetHashCode()} inputs={forexNetwork.InputCount} layers={forexNetwork.LayerCount}" +
				$" outputs={forexNetwork.OutputCount} neurons={forexNetwork.TotalNeurons} connections={forexNetwork.TotalConnections}");

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

			networkOutput = Reassembler.Execute(functionsTextContent, inputDimension,
				Open, Close, High, Low, Volume, Bars, forexNetwork, false,
				TimeCurrent().ToLongDateString() + TimeCurrent().ToLongTimeString(), out networkFunctionsCount);
		}

		void ScanNetworks()
		{
			DirectoryInfo d = new DirectoryInfo(Configuration.rootDirectory);
			networkDirs = d.GetDirectories("NET_*");

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
			testMse = forexNetwork.TestDataParallel(testData, 4);

			log($" * MSE: train={trainMse.ToString("0.0000")} test={testMse.ToString("0.0000")} bitfail={forexNetwork.BitFail}");
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
				double[] output = forexNetwork.Run(input);
				//forexNetwork.DescaleOutput(output);

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
			for (int i = 0; i < OrdersTotal(); i++)
			{
				if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
					break;

				if (OrderType() == OP_BUY)
				{
					if (OrderProfit() + OrderSwap() + OrderCommission()
						<= maxNegativeSpend)
					{
						if (Configuration.tryExperimentalFeatures)
							console($"с{new String('y', random.Next(1, 3))}{new String('ч', random.Next(0, 2))}к{new String('a', random.Next(1, 2))} бля проёбано {OrderProfit()}$",
								ConsoleColor.Black, ConsoleColor.Red);

						spendBuys++;

						OrderClose(OrderTicket(), OrderLots(), Bid, 3, Color.White);
						log("close buy " + OrderTicket() + " bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" + OrderProfit());
						dayOperationsCount++;
					}
				}
				if (OrderType() == OP_SELL)
				{
					if (OrderProfit() + OrderSwap() + OrderCommission()
						<= maxNegativeSpend)
					{
						if (Configuration.tryExperimentalFeatures)
							console($"с{new String('y', random.Next(1, 3))}{new String('ч', random.Next(0, 2))}к{new String('a', random.Next(1, 2))} бля проёбано {OrderProfit()}$",
								ConsoleColor.Black, ConsoleColor.Red);

						spendSells++;

						OrderClose(OrderTicket(), OrderLots(), Ask, 3, Color.White);
						log("close sell " + OrderTicket() + "  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() +
							" profit=" + OrderProfit());
						dayOperationsCount++;
					}
				}
			}
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
					dayOperationsCount++;
					charizedOrdersHistory += "c";
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
					dayOperationsCount++;
					charizedOrdersHistory += "c";
				}
			}
			return (0);
		}

		void SendSell(string comment = null)
		{
			RefreshRates();
			double stopLoss = 0;// Ask - ordersStopPoints * Point;
			DateTime expirationTime = TimeCurrent();
			expirationTime = expirationTime.AddHours(3);
			OrderSend(symbol, OP_SELL, lotsOptimized, Bid, 50, stopLoss, 0, $"Probability: {comment}",
				Configuration.magickNumber, expirationTime, Color.Red);
			log($"open sell  prob:{comment} @" + Bid);
			AddLabel($"SP {sellProbability.ToString("0.0")} BP {buyProbability.ToString("0.0")}", Color.Red);
			dayOperationsCount++;
			lastTradeBar = Bars;
		}

		void SendBuy(string comment = null)
		{
			RefreshRates();
			double stopLoss = 0;//Bid - ordersStopPoints * Point;
			DateTime expirationTime = TimeCurrent();
			expirationTime = expirationTime.AddHours(3);
			OrderSend(symbol, OP_BUY, lotsOptimized, Ask, 50, stopLoss, 0, $"Probability: {comment}",
				Configuration.magickNumber, expirationTime, Color.Blue);
			log($"open buy prob:{comment} @" + Ask);
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
			ObjectSetText(on, text, 8, "lucida console", clr);
		}

		void AddVerticalLabel(string text)
		{
			string on;
			double pos = Math.Max(Bid, Ask);

			pos = Math.Max(Bid, Ask) + 0.0015;
			on = (pos.ToString());
			ObjectCreate(on, OBJ_TEXT, 0, iTime(Symbol(), 0, 0), pos);
			ObjectSet(on, OBJPROP_ANGLE, 90.0);
			ObjectSetText(on, text, 8, "lucida console", Color.White);
		}

		void DrawStats(bool commentsOnly = false)
		{
			int i;

			for (i = 0; i < 11; i++)
			{
				labelID = "order" + i;
				if (ObjectFind(labelID) == -1)
				{
					ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
					ObjectSet(labelID, OBJPROP_CORNER, 1);
					ObjectSet(labelID, OBJPROP_XDISTANCE, 1000);
					ObjectSet(labelID, OBJPROP_YDISTANCE, i * 18);
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
						OrderProfit().ToString(), 10, "lucida console", OrderProfit() > 0.0 ? Color.LightGreen : Color.Red);
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

			total = activeProfit;
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
				ObjectSet(labelID, OBJPROP_XDISTANCE, 627);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 608);
			}

			ObjectSetText(labelID, "ActiveSpend: " + DoubleToStr(activeSpend, 2), 8, "lucida console", Color.Red);

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
						  "lucida console",
						  Color.LightGreen);

			spends = activeSpend;
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
				ObjectSet(labelID, OBJPROP_YDISTANCE, 655);
			}
			ObjectSetText(labelID,
						  $"{charizedOrdersHistory}",
						  10,
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

			ObjectSetText(labelID, "Total operations: " + (totalOperationsCount + dayOperationsCount), 8, "lucida console", Color.Yellow);

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
				ObjectSet(labelID, OBJPROP_XDISTANCE, 15);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 191);
			}
			ObjectSetText(labelID, "Buy Prob. " + buyProbability.ToString("0.0000"), 14, "lucida console",
				buyProbability > 0.0 ? Color.LightCyan : Color.Gray);

			labelID = gs_80 + "12";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 15);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 442);
			}
			ObjectSetText(labelID, "Sell Prob. " + sellProbability.ToString("0.0000"), 14, "lucida console",
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
				$" {(isTrendStable ? stableTrendBar : unstableTrendBar)}" + "]", 15, "lucida console",
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
			   networkFunctionsCount +
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
			   "%\r\n" +
			   "Network Output: " +
			   ((networkOutput != null && networkOutput[0] != 0.0 && networkOutput[1] != 0.0) ?
			   ($"{ networkOutput[0].ToString("0.0000") ?? "F.FFFF"}:{ networkOutput[1].ToString("0.0000") ?? "F.FFFF"}") : ""));
			}
		}

		void ShowBanner()
		{
			log($"> Automated Expert for MT4 using neural network with strategy created by code/data fuzzing. [met8]");
			log($"> (c) 2018 Deconf (kilitary@gmail.com teleg:@deconf skype:serjnah icq:401112)");

			version = Assembly.GetExecutingAssembly().GetName().Version;
			log($"Initializing version {version} ...");

			Console.Title = $"Automated MT4 trading expert debug console. Version {version}. "
				+ (Configuration.tryExperimentalFeatures ? "[XPRMNTL_ENABLED]" : ";)");
		}

		void RenderCharizedHistory()
		{
			profitBuys = profitSells = spendSells = spendBuys = 0;
			charizedOrdersHistory = "";
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
						charizedOrdersHistory += ".";
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
		}

	}
}
