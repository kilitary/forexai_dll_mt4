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
using Color = System.Drawing.Color;
using DataType = System.Double;
using System.Threading.Tasks;
using static System.Console;
using static System.ConsoleColor;
using static forexAI.Experimental;
using static forexAI.Logger;
using static forexAI.Constants;
using System.Runtime.InteropServices;
using static Newtonsoft.Json.JsonConvert;
using System.Threading;

namespace forexAI
{
	public class ForexAI : MqlApi
	{
		[ExternVariable]
		public double orderLots = 0.01;

		[ExternVariable]
		public double maxNegativeSpend = -2;

		[ExternVariable]
		public double trailingBorderFactor = 1.1;

		[ExternVariable]
		public double trailingStopFactor = 0.3;

		[ExternVariable]
		public double stableBigChangeFactor = 0.2;

		[ExternVariable]
		public double tradeEnterProbabilityMin = 0.7;

		[ExternVariable]
		public double rejectTradeProbability = -0.2;

		[ExternVariable]
		public double minLossForCounterTrade = -5.0;

		[ExternVariable]
		public bool useOptimizedLots = true;

		[ExternVariable]
		public bool muteSound = false;

		[ExternVariable]
		public int maxOrdersParallel = 10;

		[ExternVariable]
		public int minStableTrendBarForEnter = 2;

		[ExternVariable]
		public int maxStableTrendBarForEnter = 50;

		[ExternVariable]
		public int minTradePeriodBars = 3;

		[ExternVariable]
		public bool counterTrading = false;

		[ExternVariable]
		public int countedMeasuredProbabilityBars = 4;

		[ExternVariable]
		public double maxOrderOpenHours = 8.5;

		[ExternVariable]
		public double minOrderDistance = 0.003;

		[ExternVariable]
		public double orderAliveHours = 4;

		[ExternVariable]
		public double collapseEnterLots = 0.01;

		[ExternVariable]
		public double counterTradeLots = 0.02;

		[ExternVariable]
		public double collapseChangePoints = 0.0048;

		[DllImport("kernel32.dll")]
		public static extern bool AttachConsole(int dwProcessId);

		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);
		public delegate bool HandlerRoutine(CtrlTypes CtrlType);
		public enum CtrlTypes
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}


		private const int ATTACH_PARENT_PROCESS = -1;

		//  props
		public Random random = new Random((int) DateTimeOffset.Now.ToUnixTimeMilliseconds() + 1314);
		public Storage storage = new Storage();
		public NeuralNet fannNetwork = null;
		public TrainingData trainingData = null;
		public Process currentProcess = null;
		public TrainingData testData = null;
		public Stopwatch runWatch = null;
		public string networkId = string.Empty;
		public string fannNetworkDirName = string.Empty;
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
		double minStopLevel = 0;
		double ordersStopPoints = 0;
		double risky2_Risk = 2;
		double risky2_Lots = 0.04;
		double risky2_LotDigits = 2;
		double minlot = 0.01;
		double maxlot = 0.08;
		double leverage = 0.0;
		double lotsize = 0.01;
		double spread;
		double previousSpread;
		double stoplevel = 0.0;
		double trailingBorder = 30;
		double trailingStop = 25;
		static double cpuCounterValue;
		double[] prevNetworkOutputBuy = new double[11];
		double[] prevNetworkOutputSell = new double[11];
		double[] fannNetworkOutput = null;
		double[] prevBuyProbability = null;
		double[] prevSellProbability = null;
		float testMse = 0.0f;
		float trainMse = 0.0f;
		long lastDrawStatsTimestamp = 0;
		long lastMemoryStatsDumpTimesamp = 0;
		bool reassembleStageOverride = true;
		bool hasNoticedLowBalance = false;
		bool profitTrailing = true;
		bool hasNightReported = false;
		bool hasMorningReported = false;
		bool neuralNetworkBootstrapped = false;
		bool hasIncreasedUnstableTrendBar = false;
		bool networkIsGood = false;
		int stableTrendCurrentBar = 0;
		static int spendSells = 0;
		static int spendBuys = 0;
		static int profitSells = 0;
		static int profitBuys = 0;
		int totalSpends = 0;
		int totalProfits = 0;
		int openedBuys = 0;
		int openedSells = 0;
		int inputDimension = 0;
		int currentTicket = 0;
		int currentDayOperationsCount = 0;
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
		int tradeBar = 0;
		int marketCollapsedBar = 0;
		int slipPage = 30;

		// computed properties
		int ordersCount => Data.ordersActive.Count();
		int tradeBarPeriodPass => Bars - tradeBar;
		int buysCount => Data.ordersActive.Where(o => o.type == Constants.OrderType.Buy).Count();
		int sellsCount => Data.ordersActive.Where(o => o.type == Constants.OrderType.Sell).Count();
		double buyProbability => fannNetworkOutput == null ? 0.0 : (App.config.IsEnabled("versaOut") ? fannNetworkOutput[1] : fannNetworkOutput[0]);
		double sellProbability => fannNetworkOutput == null ? 0.0 : (App.config.IsEnabled("versaOut") ? fannNetworkOutput[0] : fannNetworkOutput[1]);
		double ordersProfit => buysProfit + sellsProfit;
		double diffProbability => buyProbability + sellProbability;
		TrendDirection collapseDirection => Low[0] - Low[1] > 0.0 ? TrendDirection.Up : TrendDirection.Down;

		double activeIncome
		{
			get
			{
				double total = 0.0;
				double spends = activeLoss;
				double profit = activeProfit;

				if(profit + spends >= 0.0)
					total = profit + spends;

				return total > 0 ? total : 0;
			}
		}

		double activeProfit
		{
			get
			{
				double total = 0.0;

				foreach(var order in Data.ordersActive)
				{
					var orderTotal = order.profit + order.commission + order.swap;

					if(orderTotal > 0.0)
						total += orderTotal;
				}

				return total;
			}
		}

		double buysProfit
		{
			get
			{
				double buyIncome = 0.0;

				Helpers.Each(Data.ordersActive.Where(o => o.type == Constants.OrderType.Buy), delegate (Order order)
				{
					buyIncome += order.calculatedProfit;
				});

				return buyIncome;
			}
		}

		double sellsProfit
		{
			get
			{
				double sellIncome = 0.0;

				Helpers.Each(Data.ordersActive.Where(o => o.type == Constants.OrderType.Sell), delegate (Order order)
				{
					sellIncome += order.calculatedProfit;
				});

				return sellIncome;
			}
		}

		double activeLoss
		{
			get
			{
				ordersTotal = OrdersTotal();
				double loss = 0.0;

				foreach(var order in Data.ordersActive)
				{
					var orderTotal = order.profit + order.commission + order.swap;
					if(orderTotal < 0.0)
						loss += orderTotal;
				}

				return loss;
			}
		}

		public double lotsOptimizedV3
		{
			get
			{
				double lots = MathMin(AccountBalance(), AccountFreeMargin()) * 0.01 / 1000 / (MarketInfo(symbol, MODE_TICKVALUE));
				if(lots < 0.01)
					lots = 0.01;
				return lots;
			}
		}

		public double lotsOptimizedV2
		{
			get
			{
				leverage = AccountLeverage();
				double MinLots = lotsize;
				double MaximalLots = 0.08;
				double lots = risky2_Lots;

				lots = NormalizeDouble(AccountFreeMargin() * risky2_Risk / 100 / 1000.0, 5);

				if(lots < minlot) lots = minlot;

				if(lots > MaximalLots) lots = MaximalLots;

				if(AccountFreeMargin() < Ask * lots * lotsize / leverage)
					consolelog("fail to calc lots, too les money");
				else
					lots = NormalizeDouble(risky2_Lots, Digits);

				return lots;
			}
		}

		public double lotsOptimizedV1
		{
			get
			{
				if(!useOptimizedLots)
					return orderLots;

				var MaximumRisk = 0.03;
				double DecreaseFactor = 3;
				// history orders total
				int orders = OrdersHistoryTotal();
				// number of losses orders without a break
				int losses = 0;
				//---- select lot size
				double lot = NormalizeDouble(AccountFreeMargin() * MaximumRisk / 1000.0, 1);
				//---- calcuulate number of losses orders without a break
				if(DecreaseFactor > 0)
				{
					for(int index = orders - 1; index >= 0; index--)
					{
						if(!OrderSelect(index, SELECT_BY_POS, MODE_HISTORY))
						{
							error("Error in history!");
							continue;
						}
						if(OrderSymbol() != symbol || OrderType() > OP_SELL)
							continue;

						if(OrderProfit() > 0)
							break;
						if(OrderProfit() < 0)
							losses++;
					}
					if(losses > 1)
						lot = NormalizeDouble(lot - lot * losses / DecreaseFactor, 1);
				}
				//---- return lot size
				if(lot < orderLots)
					lot = orderLots;
				return lot;
			}
		}

		bool isTrendStable
		{
			get
			{
				bool bStableTrend = true;

				for(var x = 0; x < prevBuyProbability.Length; x++)
				{
					if(Math.Abs(prevBuyProbability[x] - buyProbability) >= stableBigChangeFactor)
					{
						bStableTrend = false;
						stableTrendBar = 0;

						if(stableTrendCurrentBar != Bars && !hasIncreasedUnstableTrendBar)
						{
							unstableTrendBar++;
							hasIncreasedUnstableTrendBar = true;
						}
					}
				}

				if(stableTrendCurrentBar != Bars)
				{
					for(var i = 0; i < prevBuyProbability.Length - 1; i++)
						prevBuyProbability[i] = prevBuyProbability[i + 1];

					prevBuyProbability[prevBuyProbability.Length - 1] = buyProbability;
				}

				for(var x = 0; x < prevSellProbability.Length; x++)
				{
					if(Math.Abs(prevSellProbability[x] - sellProbability) >= stableBigChangeFactor)
					{
						bStableTrend = false;
						stableTrendBar = 0;
						if(stableTrendCurrentBar != Bars && !hasIncreasedUnstableTrendBar)
						{
							unstableTrendBar++;
							hasIncreasedUnstableTrendBar = true;
						}
					}
				}

				if(stableTrendCurrentBar != Bars)
				{
					for(var i = 0; i < prevSellProbability.Length - 1; i++)
						prevSellProbability[i] = prevSellProbability[i + 1];

					prevSellProbability[prevSellProbability.Length - 1] = sellProbability;
				}

				if(bStableTrend && stableTrendCurrentBar != Bars)
				{
					unstableTrendBar = 0;
					stableTrendBar++;
				}

				if(stableTrendCurrentBar != Bars)
				{
					stableTrendCurrentBar = Bars;
					hasIncreasedUnstableTrendBar = false;
				}

				return bStableTrend;
			}
		}

		public double closestBuyDistance
		{
			get
			{
				var minNearDistance = 1110.0;
				foreach(var order in Data.ordersActive)
				{
					if(!OrderSelect(order.ticket, SELECT_BY_TICKET) || order.type != Constants.OrderType.Buy || OrderCloseTime() != new DateTime(0))
						continue;

					if(Math.Abs(order.openPrice - Bid) < minNearDistance)
						minNearDistance = Math.Abs(order.openPrice - Bid);
				}
				return minNearDistance;
			}
		}

		public double closestSellDistance
		{
			get
			{
				double minNearDistance = 1110.0;
				foreach(var order in Data.ordersActive)
				{
					if(!OrderSelect(order.ticket, SELECT_BY_TICKET) || order.type != Constants.OrderType.Sell || OrderCloseTime() != new DateTime(0))
						continue;

					if(Math.Abs(order.openPrice - Bid) < minNearDistance)
						minNearDistance = Math.Abs(order.openPrice - Bid);
				}

				return minNearDistance;
			}
		}

		private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
		{
			switch(ctrlType)
			{
				case CtrlTypes.CTRL_C_EVENT:
					Console.WriteLine("CTRL+C received!");
					break;
				case CtrlTypes.CTRL_BREAK_EVENT:
					Console.WriteLine("CTRL+BREAK received!");
					break;
				case CtrlTypes.CTRL_CLOSE_EVENT:
					Console.WriteLine("Program being closed!");
					break;
				case CtrlTypes.CTRL_LOGOFF_EVENT:
				case CtrlTypes.CTRL_SHUTDOWN_EVENT:
					Console.WriteLine("User is logging off!");
					break;
			}
			return true;
		}

		public ForexAI()
		{
			//SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);

			Task.Factory.StartNew(() =>
			{
				AttachConsole(ATTACH_PARENT_PROCESS);
				ConsoleCommandReceiver.CommandReadingLoop();
			});

			Task.Factory.StartNew(() =>
			{
				consolelog($"Accessing processor performance counters ...");
				App.processorPerformanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
				consolelog($"... done with counters");
			});
		}

		public override int start()
		{
			SyncOrders();
			AssignCounterOrders();
			CheckForAutoClose();
			CheckForMarketCollapse();
			CloseSpendOrders();
			CalculateTrailSettings();
			TrailPositions();
			//	CheckForCounterOpen();

			if(!IsOptimization())
			{
				if(runWatch.ElapsedMilliseconds - lastDrawStatsTimestamp >= 450)
				{
					FillHistoryOrdersStatistics();
					DrawStats();

					if(App.processorPerformanceCounter != null)
						cpuCounterValue = App.processorPerformanceCounter.NextValue();

					lastDrawStatsTimestamp = runWatch.ElapsedMilliseconds;
				}

				if(runWatch.ElapsedMilliseconds - lastMemoryStatsDumpTimesamp >= 1380000)
				{
					lastMemoryStatsDumpTimesamp = runWatch.ElapsedMilliseconds;
					Helpers.ShowMemoryUsage();
				}
			}

			if(Bars == previousBars)
				return 0;

			if(neuralNetworkBootstrapped)
			{
				(networkFunctionsCount, fannNetworkOutput) = Reassembler.Execute(functionsTextContent, inputDimension, fannNetwork, reassembleStageOverride);

				RefreshRates();

				TryEnterTrade();

				if(counterTrading)
					TryEnterCounterTrade();
			}

			if(Configuration.unlinkBadNetworksEnabled && networkId.Length > 0 && IsBadNetwork())
			{
				AudioFX.Wipe();

				consolelog($"Deleting {networkId}, beacuse it is desynced/bad/notprofitable ({prevNetworkOutputBuy[0]}, {prevNetworkOutputSell[0]})");

				if(Directory.Exists($"{Configuration.rootDirectory}\\{networkId}"))
					Directory.Delete($"{Configuration.rootDirectory}\\{networkId}", true);

				InitNetwork();
			}

			if(!IsOptimization())
				RenderVisualizedzedHistory();

			if(!hasNightReported && TimeHour(TimeCurrent()) == 0)
			{
				stableTrendBar = unstableTrendBar = 0;
				hasNightReported = true;
				log($"Night....");
				AddLabel($"[kNIGHT]", Color.DarkOrange);
			}
			else if(hasNightReported && TimeHour(TimeCurrent()) == 1)
				hasNightReported = false;

			if(!hasMorningReported && TimeHour(TimeCurrent()) == 7)
			{
				stableTrendBar = unstableTrendBar = 0;
				hasMorningReported = true;
				log($"Morning!");
				AddLabel($"[MORNING]", Color.DarkGreen);
				buysPermitted = sellsPermitted = 3;
			}
			else if(hasMorningReported && TimeHour(TimeCurrent()) == 8)
				hasMorningReported = false;

			if(previousBankDay != Day())
			{
				previousBankDay = Day();
				log($"-> Day {previousBankDay.ToString("0")} [opsDone={currentDayOperationsCount} barsPerDay={barsPerDay}] accountBanalce={AccountBalance()} "
					+ (fannNetwork == null ? "[BUT NO NETWORK HAHA]" : ""));
				totalOperationsCount += currentDayOperationsCount;
				currentDayOperationsCount = barsPerDay = stableTrendBar = unstableTrendBar = 0;
				AudioFX.NewDay();
			}

			if(AccountBalance() <= 35.0 && !hasNoticedLowBalance)
			{
				hasNoticedLowBalance = true;
				console($"всё пизда, кеш весь слит нахуй, бабок: {AccountBalance()}$", ConsoleColor.Red, ConsoleColor.White);
				AudioFX.LowBalance();
				TerminalClose(0);
			}
			else if(hasNoticedLowBalance && YRandom.Next(6) == 3)
				AudioFX.GoodWork();

			var trend = stableTrendBar > 0 ? $"Stable {stableTrendBar}" : $"Unstable  {unstableTrendBar}";

			log($"=> Buy {buyProbability.ToString("0.0000").PadLeft(7)} Sell {sellProbability.ToString("0.0000").PadLeft(7)}" +
				$" Diff {diffProbability.ToString("0.0000").PadLeft(7)} " +
				$"{trend}", "debug");

			#region matters
			if(Configuration.tryExperimentalFeatures)
				AlliedInstructions();
			#endregion

			previousBars = Bars;
			barsPerDay += 1;

			return 0;
		}

		private void CheckForCounterOpen()
		{
			double iLots = lotsize;

			if(iLots > 0.03)
				iLots = 0.03;

			if(activeLoss < 0.0 && !isTrendStable)
			{
				if(buysProfit < 0.0 && activeLoss < maxNegativeSpend / 3 && Bars - tradeBar >= 3)
				{
					consolelog($"counter buy {iLots}");
					OpenSell(iLots);
				}
				else if(sellsProfit < 0.0 && activeLoss < maxNegativeSpend / 3 && Bars - tradeBar >= 3)
				{
					consolelog($"counter sell {iLots}");
					OpenBuy(iLots);
				}
			}
		}

		private void CalculateTrailSettings()
		{
			spread = MarketInfo(symbol, MODE_SPREAD);
			if(spread == previousSpread)
				return;

			trailingBorder = Math.Round(spread * trailingBorderFactor);
			trailingStop = Math.Round(trailingBorder * trailingStopFactor);
			log($"[spread={spread}] set trailingBorder {trailingBorder} trailingStop: {trailingStop}", "auto");
			previousSpread = spread;
		}

		public override int init()
		{
			Clear();

			InitPredefinedVariables();

			startTime = GetTickCount();

			App.mqlApi = this;

			networkIsGood = false;

			Data.ordersActive.Clear();

			lock(App.ordersHistoryLock)
				Data.ordersHistory.Clear();

			if(muteSound || IsOptimization())
				Configuration.audioEnabled = false;

			ShowBanner();

			if(runWatch == null)
			{
				runWatch = new Stopwatch();
				runWatch.Start();
			}

			currentDay = (int) DateTime.Now.DayOfWeek;
			neuralNetworkBootstrapped = false;
			reassembleStageOverride = false;
			prevBuyProbability = new double[countedMeasuredProbabilityBars];
			prevSellProbability = new double[countedMeasuredProbabilityBars];

			console($"orderLots={orderLots} maxNegativeSpend={maxNegativeSpend} trailingBorder={trailingBorder} trailingStop={trailingStop}" +
				$" stableBigChangeFactor={stableBigChangeFactor} enteringTradeProbability={tradeEnterProbabilityMin} BlockingTradeProbability={rejectTradeProbability}" +
				$" MinLossForCounterTrade={minLossForCounterTrade} useOptimizedLots={useOptimizedLots} maxOrdersInParallel={maxOrdersParallel}" +
				$" minStableTrendBarForEnter={minStableTrendBarForEnter} maxStableTrendBarForEnter={maxStableTrendBarForEnter} " +
				$"minTradePeriodBars={minTradePeriodBars} counterTrading={counterTrading}", ConsoleColor.Black, ConsoleColor.Yellow);

			DumpInputConfig();

			Core.SetCompatibility(Core.Compatibility.Metastock);
			console($"Set Core.Compatibility.Metastock");
			//Core.SetUnstablePeriod(Core.FuncUnstId.FuncUnstNone, 10);

			#region matters
			if((Environment.MachineName == "USER-PC" || (Experimental.IsHardwareForcesConnected() == Experimental.IsBlackHateFocused()))
				&& (currentDay == 0) && false)
				Configuration.tryExperimentalFeatures = true;
			#endregion

			InitVariables();
			ListGlobalVariables();
			DumpInfo();

			ClearLogs();
			EraseLogs(Configuration.XRandomLogFileName, Configuration.YRandomLogFileName);

			InitNetwork();

			Title = $"Automated MT4 trading expert debug console. Version {App.version}. Network: {networkId} "
				+ (Configuration.tryExperimentalFeatures ? "[XPRMNTL_ENABLED]" : ";)");

			dump(ConfigSettings.SharedSettings, "SharedSettings", "dev");

			string initStr = $"Initialized in {(((double) GetTickCount() - (double) startTime) / 1000.0).ToString("0.0")} sec(s) (v{App.version})";
			log(initStr);
			console(initStr, ConsoleColor.Black, ConsoleColor.Yellow);

			return 0;
		}

		private void AssignCounterOrders()
		{
			//Helpers.Each(Data.ordersActive, order => order.FindSpendCounterOrder());
			Parallel.ForEach(Data.ordersActive, (order) => order.FindSpendCounterOrder());
		}

		private void DumpInputConfig()
		{
			FieldInfo[] myFieldsInfo;
			Type info = typeof(ForexAI);
			myFieldsInfo = info.GetFields(BindingFlags.Public);
			dump(info.GetProperties(), "props", "dev");
			dump(myFieldsInfo, "myFieldsInfo", "dev");

			log($"attribs: {myFieldsInfo.Length}", "dev");
			foreach(var field in myFieldsInfo)
			{
				var skip = 0;
				var names = field.GetCustomAttributesData();
				dump(names, "names", "dev");
				skip = (from n in names
						  where n.ToString() == "[NQuotes.ExternVariableAttribute()]"
						  select n).Count();
				if(skip == 0)
					continue;
			}
		}

		public override int deinit()
		{
			log("Deinitializing ...");
			log($"Balance={AccountBalance()} Orders={OrdersTotal()} UninitializeReason={UninitializeReason()}");

			//config.Set("balance", AccountBalance());
			App.config.Save();
			storage.SyncData();

			string mins = ((((double) GetTickCount() - startTime) / 1000.0 / 60.0)).ToString("0.00");
			log($"Uptime {mins} mins, has do {totalOperationsCount + currentDayOperationsCount} operations.");
			console("... shutted down.", ConsoleColor.Black, ConsoleColor.Red);
			return 0;
		}

		public void InitNetwork()
		{
			reassembleStageOverride = true;
			ScanNetworks();

			if(Data.networksDirectories.Length > 0)
			{
				string network = Data.networksDirectories[random.Next(Data.networksDirectories.Length - 1)].Name;
				log($"Init network: selected {network} from {Configuration.rootDirectory} (total {Data.networksDirectories.Length} networks)", "debug");

				LoadNetwork(network);

				if(fannNetwork != null)
				{
					if(App.config.IsEnabled("startupTesting"))
					{
						console($"Testing network {network} MSE ...");
						TestNetworkMSE();
						console($"Testing network {network} hit ratio ...");
						TestNetworkHitRatio();
					}

					reassembleStageOverride = false;

				}
			}
		}

		public bool IsBadNetwork()
		{
			bool buyStall = true, sellStall = true;

			if(buyProbability >= 1.5 || buyProbability <= -1.5 || sellProbability >= 1.5 || sellProbability <= -1.5)
				return true;

			for(var i = 0; i < prevNetworkOutputBuy.Length - 1; i++)
				prevNetworkOutputBuy[i] = prevNetworkOutputBuy[i + 1];

			prevNetworkOutputBuy[prevNetworkOutputBuy.Length - 1] = buyProbability;

			for(var i = 0; i < prevNetworkOutputSell.Length - 1; i++)
				prevNetworkOutputSell[i] = prevNetworkOutputSell[i + 1];

			prevNetworkOutputSell[prevNetworkOutputSell.Length - 1] = sellProbability;

			double prevValue = prevNetworkOutputBuy[0];
			for(var i = 1; i < prevNetworkOutputBuy.Length; i++)
			{
				if(prevNetworkOutputBuy[i].ToString("0.0000") != prevValue.ToString("0.0000"))
					buyStall = false;
			}

			prevValue = prevNetworkOutputSell[0];
			for(var i = 1; i < prevNetworkOutputSell.Length; i++)
			{
				if(prevNetworkOutputSell[i].ToString("0.0000") != prevValue.ToString("0.0000"))
					sellStall = false;
			}

			if(sellStall || buyStall)
			{
				log($"sellStall {sellStall} buyStall {buyStall} prevValue {prevValue} bars {Bars}", "debug");
				dump(prevNetworkOutputBuy, "buy", "debug");
				dump(prevNetworkOutputSell, "sell", "debug");
			}

			if(Bars > prevNetworkOutputBuy.Length && (sellStall || buyStall))
				return true;

			return false;
		}

		void CheckForAutoClose()
		{
			if(activeLoss + activeProfit >= 0.0 && (buysProfit < 0.0 || sellsProfit < 0.0) && ordersCount >= 2)
			{
				consolelog($"auto-close profit activeIncome:{activeIncome} activeLoss:{activeLoss} " +
					$" buyProft:{buysProfit} sellProfit:{sellsProfit} ordersCount:{ordersCount}", "debug");
				CloseAllOrders();
			}
		}

		void CloseAllOrders()
		{
			CloseBuys();
			CloseSells();
		}

		void CheckForMarketCollapse()
		{
			if(Bars < 5)
				return;

			var change = Math.Max(High[1], Low[1]) - Math.Min(High[1], Low[1]);
			if((change >= collapseChangePoints) && Bars - marketCollapsedBar >= 3)
			{
				console($"Market collapse detected on {TimeCurrent()} change: {change.ToString("0.00000")}, going {collapseDirection}",
					ConsoleColor.Black, ConsoleColor.Green);

				AddVerticalLabel($"Market collapse {collapseDirection} ({change.ToString("0.00000")})", Color.Aquamarine);

				marketCollapsedBar = Bars;

				/*if (collapseDirection == Constants.TrendDirection.Up)
					OpenBuy(collapseEnterLots);
				else
					OpenSell(collapseEnterLots);*/
			}
		}

		public void SyncOrders()
		{
			var zeroTime = new DateTime(0);
			var forexTimeCurrent = TimeCurrent();

			Helpers.Each(Data.ordersActive, order =>
			{
				if(OrderSelect(order.ticket, SELECT_BY_TICKET) == true && OrderCloseTime() != zeroTime && order.profit > 0.0)
				{
					AudioFX.Profit();

					consolelog($"profit {order.type.ToString()} #{order.ticket} {OrderProfit()}$ lots {OrderLots()} " +
						$"(total={AccountBalance().ToString("0.00")}, spends={activeLoss}, profit={activeProfit})", null, ConsoleColor.Green);
				}
				else if(OrderSelect(order.ticket, SELECT_BY_TICKET) == true && OrderCloseTime() != zeroTime && order.profit < 0.0)
				{
					AudioFX.Fail();

					consolelog($"loss {order.type.ToString()} #{order.ticket} {OrderProfit()}$ lots {OrderLots()} " +
						$"(total={AccountBalance().ToString("0.00")}, spends={activeLoss}, profit={activeProfit})", null, ConsoleColor.Red);
				}
			});

			Data.ordersActive = Data.ordersActive.Where(
				o => OrderSelect(o.ticket, SELECT_BY_TICKET) == true && OrderCloseTime() == zeroTime).ToHashSet();

			for(int index = 0; index < OrdersTotal(); index++)
			{
				if(!OrderSelect(index, SELECT_BY_POS, MODE_TRADES))
					continue;

				int orderTicket = OrderTicket();

				var order = (from o in Data.ordersActive
								 where o.ticket == orderTicket
								 select o).DefaultIfEmpty(null).FirstOrDefault();

				if(order == null)
				{
					var newOrder = new Order
					{
						ticket = orderTicket,
						openTime = OrderOpenTime(),
						symbol = OrderSymbol(),
						lots = OrderLots(),
						profit = OrderProfit(),
						commission = OrderCommission(),
						swap = OrderSwap(),
						openPrice = OrderOpenPrice(),
						stopLoss = OrderStopLoss(),
						comment = OrderComment(),
						takeProfit = OrderTakeProfit(),
						ageInMinutes = forexTimeCurrent.Subtract(OrderOpenTime()).TotalMinutes,
						expiration = OrderExpiration(),
						magickNumber = OrderMagicNumber()
					};

					if(OrderType() == OP_BUY)
						newOrder.type = Constants.OrderType.Buy;
					else if(OrderType() == OP_SELL)
						newOrder.type = Constants.OrderType.Sell;

					Data.ordersActive.Add(newOrder);

					lock(App.ordersHistoryLock)
					{
						if(!Data.ordersHistory.Where(o => o.ticket == orderTicket).Any())
							Data.ordersHistory.Add(newOrder);
					}
				}
				else
				{
					order.profit = OrderProfit();
					order.commission = OrderCommission();
					order.swap = OrderSwap();
					order.stopLoss = OrderStopLoss();
					order.takeProfit = OrderTakeProfit();
					order.ageInMinutes = forexTimeCurrent.Subtract(order.openTime).TotalMinutes;
					order.expiration = OrderExpiration();

					if(order.counterOrder != null && !Data.ordersActive.Where(o => o == order.counterOrder).Any())
						order.counterOrder = null;
				}
			}
		}

		void FillHistoryOrdersStatistics()
		{
			profitBuys = spendBuys = profitSells = spendSells = 0;

			lock(App.ordersHistoryLock)
			{
				foreach(var order in Data.ordersHistory)
				{
					if(OrderSelect(order.ticket, SELECT_BY_TICKET, MODE_HISTORY) && OrderCloseTime() != new DateTime(0))
					{
						if(order.type == Constants.OrderType.Buy)
						{
							if(order.profit > 0)
								profitBuys++;
							else
								spendBuys++;
						}
						else if(order.type == Constants.OrderType.Sell)
						{
							if(order.profit > 0)
								profitSells++;
							else
								spendSells++;
						}
					}
				}
			}
		}

		public void InitVariables()
		{
			minlot = MarketInfo(symbol, MODE_MINLOT);
			maxlot = MarketInfo(symbol, MODE_MAXLOT);
			lotsize = MarketInfo(symbol, MODE_LOTSIZE);
			stoplevel = MarketInfo(symbol, MODE_STOPLEVEL);
			symbol = Symbol();
			currentProcess = Process.GetCurrentProcess();

			if(trailingStop < stoplevel)
				warning($"minStopLevel={minStopLevel}, while trailingStop={trailingStop}, reducing.");

			ordersStopPoints = stoplevel > 0 ? stoplevel * 2 : 60;

			if(Configuration.mysqlEnabled)
				Data.mysqlDatabase = new MysqlDatabase();

			console($"Init variables [stoplevel={stoplevel}] ...");
		}

		public double CalculateLots(OrderType type)
		{
			double iLots = orderLots;

			if(!useOptimizedLots)
				return iLots;

			if(sellsProfit < 0 || buysProfit < 0)
			{
				iLots = Math.Round(orderLots * 2.3, 2);
				consolelog($"[{type}] refinancing spend sells:{sellsProfit} buys:{buysProfit}  ilots:{iLots}", "auto");
			}
			return iLots;
		}

		public void TryEnterTrade()
		{
			double iLots = orderLots;

			if(!isTrendStable || stableTrendBar < minStableTrendBarForEnter)
				return;

			if(iLots > 0.03)
				iLots = 0.03;

			if(buyProbability >= tradeEnterProbabilityMin
					&& sellProbability <= rejectTradeProbability
					&& ordersCount < maxOrdersParallel
					&& tradeBarPeriodPass > minTradePeriodBars
					&& closestBuyDistance >= minOrderDistance)
			{
				iLots = CalculateLots(Constants.OrderType.Buy);
				OpenBuy(iLots);
			}

			if(sellProbability >= tradeEnterProbabilityMin
					&& buyProbability <= rejectTradeProbability
					&& ordersCount < maxOrdersParallel
					&& tradeBarPeriodPass > minTradePeriodBars
					&& closestSellDistance >= minOrderDistance)
			{
				iLots = CalculateLots(Constants.OrderType.Sell);
				OpenSell(iLots);
			}
		}

		public void TryEnterCounterTrade()
		{
			if(buysProfit <= minLossForCounterTrade
				&& ordersCount < maxOrdersParallel
				&& tradeBarPeriodPass > minTradePeriodBars
				&& collapseDirection == TrendDirection.Down
				&& closestSellDistance >= minOrderDistance
				&& sellProbability >= 0.5)
			{
				consolelog($"opening counter-sell [{sellsCount}/{ordersCount}] lastOrder@{tradeBarPeriodPass}");
				OpenSell(lotsOptimizedV2);
			}

			if(sellsProfit <= minLossForCounterTrade
				&& ordersCount < maxOrdersParallel
				&& tradeBarPeriodPass > minTradePeriodBars
				&& collapseDirection == TrendDirection.Up
				&& closestBuyDistance >= minOrderDistance
				&& buyProbability >= 0.5)
			{
				consolelog($"opening counter-buy [{buysCount}/{ordersCount}] lastOrder@{tradeBarPeriodPass}");
				OpenBuy(lotsOptimizedV2);
			}
		}

		public void ListGlobalVariables()
		{
			int var_total = GlobalVariablesTotal();
			string name;
			for(int i = 0; i < var_total; i++)
			{
				name = GlobalVariableName(i);
				debug($"->var #{i} [{name}={GlobalVariableGet(name)}]");
			}
		}

		void LoadNetwork(string dirName)
		{
			long fileLength = new FileInfo($"{Configuration.rootDirectory}\\{dirName}\\FANN.net").Length;
			log($"Loading network {dirName} ({(fileLength / 1024.0).ToString("0.00")} KB)");

			App.currentNetworkId = dirName;
			networkId = fannNetworkDirName = dirName;

			fannNetwork = new NeuralNet($"{Configuration.rootDirectory}\\{dirName}\\FANN.net")
			{
				ErrorLog = new FANNCSharp.FannFile($"{Configuration.rootDirectory}\\fann.log", "a+")
			};

			log($"Network: hash={fannNetwork.GetHashCode()} inputs={fannNetwork.InputCount} layers={fannNetwork.LayerCount}" +
				$" outputs={fannNetwork.OutputCount} neurons={fannNetwork.TotalNeurons} connections={fannNetwork.TotalConnections}");

			string fileTextData = File.ReadAllText($@"{Configuration.rootDirectory}\{dirName}\configuration.txt");

			Match match2 = Regex.Match(fileTextData, "InputDimension:\\s+(\\d+)?");
			int.TryParse(match2.Groups[1].Value, out inputDimension);

			log($"InputDimension = {inputDimension}");

			Match matches = Regex.Match(fileTextData,
				 "InputActFunc:\\s+([^ ]{1,40}?)\\s+LayerActFunc:\\s+([^ \r\n]{1,40})",
				 RegexOptions.Singleline);

			log($"Activation functions: input [{matches.Groups[1].Value}] layer [{matches.Groups[2].Value}]");

			inputLayerActivationFunction = matches.Groups[1].Value;
			middleLayerActivationFunction = matches.Groups[2].Value;

			functionsTextContent = File.ReadAllText($"{Configuration.rootDirectory}\\{fannNetworkDirName}\\functions.json");

			(networkFunctionsCount, fannNetworkOutput) = Reassembler.Execute(functionsTextContent, inputDimension, fannNetwork, false);

			if(networkFunctionsCount > 0 && fannNetworkOutput.Length > 0)
				neuralNetworkBootstrapped = true;

			Helpers.ZeroArray(prevNetworkOutputBuy);
			Helpers.ZeroArray(prevNetworkOutputSell);
		}

		void ScanNetworks()
		{
			consolelog($"Scanning networks ...");
			Data.networksDirectories = new DirectoryInfo(Configuration.rootDirectory).GetDirectories("NET_*");

			consolelog($"Found {Data.networksDirectories.Length} networks in {Configuration.rootDirectory}.");
			if(Data.networksDirectories.Length == 0)
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

			trainingData = new TrainingData(Configuration.rootDirectory + $"\\{fannNetworkDirName}\\traindata.dat");
			testData = new TrainingData(Configuration.rootDirectory + $"\\{fannNetworkDirName}\\testdata.dat");

			log($" * trainDataLength={trainingData.TrainDataLength} testDataLength={testData.TrainDataLength}");

			trainMse = fannNetwork.TestDataParallel(trainingData, 4);
			testMse = fannNetwork.TestDataParallel(testData, 4);

			log($" * MSE: train={trainMse.ToString("0.0000")} test={testMse.ToString("0.0000")} bitfail={fannNetwork.BitFail}");
		}

		void TestNetworkHitRatio()
		{
			trainHitRatio = CalculateHitRatio(trainingData.Input, trainingData.Output);
			testHitRatio = CalculateHitRatio(testData.Input, testData.Output);

			log($" * TrainHitRatio: {trainHitRatio.ToString("0.00")}% TestHitRatio: {testHitRatio.ToString("0.00")}%");
		}

		double CalculateHitRatio(double[][] inputs, double[][] desiredOutputs)
		{
			int hits = 0, curX = 0;
			foreach(double[] input in inputs)
			{
				double[] output = fannNetwork.Run(input);

				fannNetwork.DescaleOutput(output);

				double output0 = 0;
				if(output[0] > output[1])
					output0 = 1.0;
				else
					output0 = -1.0;

				double output1 = 0;
				if(output[1] > output[0])
					output1 = 1.0;
				else
					output1 = -1.0;

				if(output0 == desiredOutputs[curX][0] && output1 == desiredOutputs[curX][1])
					hits++;

				curX++;
			}

			return ((double) hits / (double) inputs.Length) * 100.0;
		}

		void CloseSpendOrders()
		{
			var currentTime = TimeCurrent();

			foreach(var order in Data.ordersActive)
			{
				if(order.calculatedProfit >= maxNegativeSpend
						&& order.ageInMinutes / 60 <= orderAliveHours)
					continue;

				AudioFX.Fail();

				if(order.type == Constants.OrderType.Buy)
				{
					if(Configuration.tryExperimentalFeatures)
						console($"с{new String('y', random.Next(1, 3))}{new String('ч', random.Next(0, 2))}к{new String('a', random.Next(1, 2))} бля проёбано {OrderProfit()}$",
							ConsoleColor.Black, ConsoleColor.Red);

					spendBuys++;

					OrderClose(order.ticket, order.lots, Bid, 3, Color.White);

					consolelog("close buy " + order.ticket + " bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" +
						(order.profit + order.commission + order.swap));

					currentDayOperationsCount++;
				}
				if(order.type == Constants.OrderType.Sell)
				{
					if(Configuration.tryExperimentalFeatures)
						console($"с{new String('y', random.Next(1, 3))}{new String('ч', random.Next(0, 2))}к{new String('a', random.Next(1, 2))} бля проёбано {OrderProfit()}$",
							ConsoleColor.Black, ConsoleColor.Red);

					spendSells++;

					OrderClose(order.ticket, order.lots, Ask, 3, Color.White);

					consolelog("close sell " + OrderTicket() + "  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() +
						" profit=" + (order.profit + order.commission + order.swap));

					currentDayOperationsCount++;
				}
			}
		}

		private int CloseBuys()
		{
			if(buysCount == 0)
				return 0;

			var ops = 0;

			log($"force closing {buysCount} buys");
			for(int orderIndex = OrdersTotal() - 1; orderIndex >= 0; orderIndex--)
			{
				if(!(OrderSelect(orderIndex, SELECT_BY_POS, MODE_TRADES)))
					break;

				if(OrderType() == OP_BUY && OrderSymbol() == symbol)
				{
					OrderClose(OrderTicket(), OrderLots(), MarketInfo(OrderSymbol(), MODE_BID), 2);
					currentDayOperationsCount++;
					charizedOrdersHistory += "c";
					ops++;
					log($" -buy #{OrderTicket()} {OrderProfit()}");
				}
			}

			return ops;
		}

		private int CloseSells()
		{
			if(sellsCount == 0)
				return 0;

			var ops = 0;

			log($"force closing {sellsCount} sells");
			for(int orderIndex = OrdersTotal() - 1; orderIndex >= 0; orderIndex--)
			{
				if(!(OrderSelect(orderIndex, SELECT_BY_POS, MODE_TRADES)))
					break;

				if(OrderType() == OP_SELL && OrderSymbol() == symbol)
				{
					OrderClose(OrderTicket(), OrderLots(), MarketInfo(OrderSymbol(), MODE_ASK), 2);
					currentDayOperationsCount++;
					charizedOrdersHistory += "c";
					ops++;
					log($" -sell #{OrderTicket()} {OrderProfit()}");
				}
			}

			return ops;
		}

		private void OpenSell(double exactLots = 0.0)
		{
			double stopLoss = 0;// Ask - ordersStopPoints * Point;
			double lots = exactLots > 0.0 ? exactLots : lotsOptimizedV1;
			DateTime expirationTime = TimeCurrent();
			int ticket;

			expirationTime = new DateTime(0);// expirationTime.AddHours(3);
			if((ticket = OrderSend(symbol, OP_SELL, lots, Bid, slipPage, stopLoss, 0, $"Probability: {sellProbability}",
				Configuration.magickNumber, expirationTime, Color.Red)) <= 0)
				consolelog($"error sending sell: {GetLastError()} balance={AccountBalance()} lots={lots}");
			else
				consolelog($"open sell #{ticket} lots={lots} prob={sellProbability.ToString("0.00")} @" + Bid, null, ConsoleColor.White);

			//AddLabel($"SP {sellProbability.ToString("0.0")} BP {buyProbability.ToString("0.0")}", Color.Red);

			currentDayOperationsCount++;
			tradeBar = Bars;
		}

		private void OpenBuy(double exactLots = 0.0)
		{
			double lots = exactLots > 0.0 ? exactLots : lotsOptimizedV1;
			double stopLoss = 0;//Bid - ordersStopPoints * Point;
			int ticket;
			DateTime expirationTime = TimeCurrent();

			expirationTime = new DateTime(0); //expirationTime.AddHours(3);
			if((ticket = OrderSend(symbol, OP_BUY, lots, Ask, slipPage, stopLoss, 0, $"Probability: {buyProbability}",
				Configuration.magickNumber, expirationTime, Color.Blue)) <= 0)
				consolelog($"error sending buy: {GetLastError()} balance={AccountBalance()} lots={lots}");
			else
				consolelog($"open buy #{ticket} lots={lots} prob={buyProbability.ToString("0.00")} @" + Ask, null, ConsoleColor.White);

			//AddLabel($"BP {buyProbability.ToString("0.0")} SP {sellProbability.ToString("0.0")}", Color.Blue);

			currentDayOperationsCount++;
			tradeBar = Bars;
		}

		void TrailPositions()
		{
			double newStopLoss = 0;
			double iTrailingStop = trailingStop;
			double iTrailingBorder = trailingBorder;
			string result = string.Empty;

			foreach(var order in Data.ordersActive)
			{
				if(order.counterOrder != null)
				{
					iTrailingBorder *= 2.7;
					iTrailingStop = iTrailingBorder * 0.7;
				}
				else
				{
					iTrailingBorder = trailingBorder;
					iTrailingStop = trailingStop;
				}

				RefreshRates();

				if(order.type == Constants.OrderType.Buy)
				{
					newStopLoss = Bid - iTrailingStop * Point;
					if((order.stopLoss == 0.0 || newStopLoss > order.stopLoss)
						&& Bid - (iTrailingBorder * Point) > order.openPrice
						&& order.calculatedProfit > 0)
					{
						if(order.stopLoss > 0)
							newStopLoss = Low[2];

						if(newStopLoss > OrderOpenPrice() || order.stopLoss == 0)
						{
							if(order.stopLoss == 0)
								newStopLoss = OrderOpenPrice() + (2 * iTrailingStop * Point);

							if(newStopLoss != order.stopLoss)
							{
								if(!OrderModify(order.ticket, order.openPrice, newStopLoss, order.takeProfit,
									order.expiration, Color.BlueViolet))
								{
									int error = GetLastError();
									result = $"FAIL: {error}: {ErrorDescription(error)}";
								}
								else
								{
									currentDayOperationsCount++;
									result = $"OK";
								}

								consolelog($"modify buy #{order.ticket} newStopLoss={newStopLoss} profit={order.profit}: {result}", null, ConsoleColor.Yellow);
							}
						}
					}
				}
				else if(order.type == Constants.OrderType.Sell)
				{
					newStopLoss = Ask + iTrailingStop * Point;
					if((order.stopLoss == 0.0 || newStopLoss < order.stopLoss)
						&& Ask + (iTrailingBorder * Point) < order.openPrice
						&& order.calculatedProfit > 0)
					{
						if(order.stopLoss > 0)
							newStopLoss = High[2];

						if(newStopLoss < OrderOpenPrice() || order.stopLoss == 0)
						{
							if(order.stopLoss == 0)
								newStopLoss = OrderOpenPrice() - (2 * iTrailingStop * Point);

							if(newStopLoss != order.stopLoss)
							{
								if(!OrderModify(order.ticket, order.openPrice, newStopLoss, order.takeProfit,
									order.expiration, Color.MediumVioletRed))
								{
									int error = GetLastError();
									result = $"FAIL: {error}: {ErrorDescription(error)}";
								}
								else
								{
									currentDayOperationsCount++;
									result = $"OK";
								}

								consolelog($"modify sell #{order.ticket} newStopLoss={newStopLoss} profit={order.profit}: {result}", null, ConsoleColor.Yellow);
							}
						}
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
			ObjectCreate(on, OBJ_TEXT, 0, iTime(symbol, 0, 0), pos);
			ObjectSetText(on, text, 11, "liberation mono", clr);
		}

		void AddVerticalLabel(string text, Color clr)
		{
			string on;
			double pos = Math.Max(Bid, Ask);

			pos = Math.Max(Bid, Ask) + 0.0025;
			on = (pos.ToString());
			ObjectCreate(on, OBJ_TEXT, 0, iTime(symbol, 0, 0), pos);
			ObjectSet(on, OBJPROP_ANGLE, 90.0);
			ObjectSetText(on, text, 16, "liberation mono", clr);
		}

		void RenderVisualizedzedHistory()
		{
			profitBuys = profitSells = spendSells = spendBuys = 0;
			charizedOrdersHistory = string.Empty;

			for(int index = OrdersHistoryTotal() - 3; index < OrdersHistoryTotal(); index++)
			{
				if(OrderSelect(index, SELECT_BY_POS, MODE_HISTORY))
				{
					if(OrderProfit() > 0.0)
					{
						if(OrderType() == OP_BUY)
						{
							profitBuys++;
							charizedOrdersHistory += $"[B={OrderProfit()}|age:{(OrderCloseTime() - OrderOpenTime()).TotalMinutes.ToString("0")}|lots:{OrderLots()}] ";
						}
						if(OrderType() == OP_SELL)
						{
							profitSells++;
							charizedOrdersHistory += $"[S={OrderProfit()}|age:{(OrderCloseTime() - OrderOpenTime()).TotalMinutes.ToString("0")}|lots:{OrderLots()}] ";
						}
					}
					else
					{
						charizedOrdersHistory += OrderType() == OP_BUY ? $"{OrderProfit()}|lots:{OrderLots()} " : $"{OrderProfit()}|lots:{OrderLots()} ";

						if(OrderType() == OP_BUY)
							spendBuys++;

						if(OrderType() == OP_SELL)
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

			debug($"AccNumber: {AccountNumber()} AccName: [{AccountName()}] Balance: {AccountBalance()} Currency: {AccountCurrency()} ");
			debug($"Company: [{TerminalCompany()}] Name: [{TerminalName()}] Path: [{TerminalPath()}]");
			debug($"Equity={AccountEquity()} FreeMarginMode={AccountFreeMarginMode()} Expert={WindowExpertName()}");
			debug($"Leverage={AccountLeverage()} Server=[{AccountServer()}] StopoutLev={AccountStopoutLevel()} StopoutMod={AccountStopoutMode()}");
			debug($"TickValue={MarketInfo(symbol, MODE_TICKVALUE)} TickSize={MarketInfo(symbol, MODE_TICKSIZE)} Minlot={MarketInfo(symbol, MODE_MINLOT)}" + $" LotStep={MarketInfo(symbol, MODE_LOTSTEP)}");
			debug($"Orders={OrdersTotal()} TimeForexCurrent=[{TimeCurrent()}] Digits={MarketInfo(symbol, MODE_DIGITS)} Spread={MarketInfo(symbol, MODE_SPREAD)}");
			debug($"IsOptimization={IsOptimization()} IsTesting={IsTesting()}");
			debug($"Period={Period()}");
			debug($"minstoplevel={minStopLevel}");

			Helpers.ShowMemoryUsage();
		}

		void ShowBanner()
		{
			consolelog($"> Automated Expert for MT4 using neural network with strategy created by code/data fuzzing. [met8]");
			consolelog($"> (c) 2018 Deconf (kilitary@gmail.com teleg:@deconf skype:serjnah icq:401112)");


			consolelog($"> Initializing version {App.version} (with .NET framework={Environment.Version.ToString()}) ...");
		}

		void DrawStats()
		{
			if(!App.config.IsEnabled("drawStats"))
				return;

			int i;

			for(i = 0; i < 10; i++)
			{
				labelID = "order" + i;
				if(ObjectFind(labelID) == -1)
				{
					ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
					ObjectSet(labelID, OBJPROP_CORNER, 1);
					ObjectSet(labelID, OBJPROP_XDISTANCE, 1142);
					ObjectSet(labelID, OBJPROP_YDISTANCE, i * 18);
				}
				ObjectSetText(labelID, "                                    ", 8, "lucida console", Color.White);
			}

			int index = 0;
			foreach(var order in Data.ordersActive)
			{
				labelID = "order" + index;

				ObjectSetText(labelID, $"#{order.ticket} " + order.type.ToString() + " " +
					order.calculatedProfit.ToString("0.00") + $" ({order.lots.ToString("0.00")} lots, " +
					$" age {(order.ageInMinutes / 60).ToString("0.0")} hours" +
					(order.counterOrder == null ? "" : $", counter-{order.counterOrder.ticket}") + ")", 8, "lucida console",
					order.calculatedProfit > 0.0 ? Color.LightGreen : Color.Red);
				index++;
			}

			string gs_80 = "text";

			labelID = gs_80 + "4";
			if(ObjectFind(labelID) == -1)
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
						  Color.Black);

			labelID = gs_80 + "5";
			if(ObjectFind(labelID) == -1)
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
						  Color.Black);

			labelID = gs_80 + "6";
			if(ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 18);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 23);
			}

			ObjectSetText(labelID, "activeLoss: " + DoubleToStr(activeLoss, 2), 8, "liberation mono", Color.Red);

			labelID = gs_80 + "7";
			if(ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 18);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 38);
			}
			ObjectSetText(labelID,
						  "ActiveIncome: " + DoubleToStr(activeIncome, 2),
						  8,
						  "liberation mono",
						  Color.Green);

			spends = activeLoss;
			profit = activeProfit;
			spends = (0 - (spends));
			// Print("profit:",profit," spends:", spends);
			if(profit > 0.0 && spends >= 0.0)
				total = (0 - ((profit) * 100.0) / spends);
			else
				total = 0;

			labelID = gs_80 + "8";
			if(ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 514);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 3);
			}

			ObjectSetText(labelID,
						  $"{charizedOrdersHistory}",
						  8,
						  "lucida console",
						  Color.Violet);

			labelID = gs_80 + "9";
			if(ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 50);
			}

			ObjectSetText(labelID, "Total operations: " + (totalOperationsCount + currentDayOperationsCount), 8, "liberation mono", Color.Black);

			labelID = gs_80 + "10";
			if(ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 60);
			}
			lock(App.ordersHistoryLock)
			{
				ObjectSetText(labelID,
						  "Live Orders: " + OrdersTotal() + $" ({Data.ordersActive.Count}/{Data.ordersHistory.Count})",
						  8,
						  "liberation mono",
						  Color.Black);
			}

			string dirtext = string.Empty, dirtext2 = string.Empty;
			labelID = gs_80 + "11";
			if(ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 15);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 191);
			}
			ObjectSetText(labelID, "Buy " + buyProbability.ToString("0.0000").PadLeft(7), 13, "liberation mono",
				buyProbability >= tradeEnterProbabilityMin ? Color.Green : Color.LightGray);

			labelID = gs_80 + "12";
			if(ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 15);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 442);
			}
			ObjectSetText(labelID, "Sell " + sellProbability.ToString("0.0000").PadLeft(7), 13, "liberation mono",
				sellProbability >= tradeEnterProbabilityMin ? Color.Green : Color.LightGray);

			labelID = gs_80 + "13";
			if(ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 15);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 318);
			}
			ObjectSetText(labelID, (isTrendStable ? "  STABLE" : "UNSTABLE") +
				$" {(isTrendStable ? stableTrendBar : unstableTrendBar)}" +
				$" {diffProbability.ToString("0.0000").PadLeft(7)}", 12, "liberation mono",
				isTrendStable && stableTrendBar >= minStableTrendBarForEnter ? Color.YellowGreen : Color.IndianRed);

			totalSpends = spendSells + spendBuys;
			totalProfits = profitSells + profitBuys;
			double KPD = 0.0;

			if(totalSpends > 0 && totalProfits > 0)
				KPD = (100.0 - ((100.0 / (double) totalProfits) * (double) totalSpends));
			else
				KPD = 100.0;

			if(ObjectFind("statyys") == -1)
			{
				ObjectCreate("statyys", OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet("statyys", OBJPROP_CORNER, 0);
				ObjectSet("statyys", OBJPROP_XDISTANCE, 150);
				ObjectSet("statyys", OBJPROP_YDISTANCE, 16);
			}

			var sellProb = string.Empty;
			var buyProb = string.Empty;

			for(var u = 0; u < prevBuyProbability.Length; u++)
				buyProb += prevBuyProbability[u].ToString("0.00") + " ";

			for(var u = 0; u < prevSellProbability.Length; u++)
				sellProb += prevSellProbability[u].ToString("0.00") + " ";

			Process proc = Process.GetCurrentProcess();
			var memoryUsage = (proc.PrivateMemorySize64 / 1024 / 1024).ToString("0.00");

			if(fannNetwork != null)
			{
				string cpuUsage = App.processorPerformanceCounter == null ? "<accesing>" : App.processorPerformanceCounter.NextValue().ToString("0.00") + "%";

				Comment(
					 $"Memory: {memoryUsage} MB" +
					$"\r\nCPU: {cpuUsage}\r\n" +
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
				  "Network: " +
					networkId +
					"\r\n" +
				  "Functions: " +
					networkFunctionsCount +
					"\r\n" +
				  "InputDimension: " +
					inputDimension +
					"\r\n" +
				  "TotalNeurons: " +
					fannNetwork.TotalNeurons +
					"\r\n" +
				  "InputCount: " +
					fannNetwork.InputCount +
					"\r\n" +
				  "InputActFunc: " +
					inputLayerActivationFunction +
					"\r\n" +
				  "LayerActFunc: " +
					middleLayerActivationFunction +
					"\r\n" +
				  "ConnRate: " +
					fannNetwork.ConnectionRate +
					"\r\n" +
				  "Connections: " +
					fannNetwork.TotalConnections +
					"\r\n" +
				  "LayerCount: " +
					fannNetwork.LayerCount +
					"\r\n" +
				  "Train/Test MSE: " +
					trainMse +
					" / " +
					testMse +
					"\r\n" +
				  "LearningRate: " +
					fannNetwork.LearningRate +
					"\r\n" +
				  "Test Hit Ratio: " +
					testHitRatio.ToString("0.00") +
					"%\r\n" +
				  "Train Hit Ratio: " +
					trainHitRatio.ToString("0.00") +
					"%" +
					$"\r\nCounter-trading: {counterTrading}\r\nOptimized Lots: {useOptimizedLots} " +
					$"(v2: {lotsOptimizedV2} v1: {lotsOptimizedV1} v3: {lotsOptimizedV3})" +
					"\r\n" +
					$"orderLots: {orderLots}\r\nmaxNegativeSpend: {maxNegativeSpend}\r\n" +
					$"Spread: {spread}\r\n" + $"trailingBorder: {trailingBorder}\r\ntrailingStop: {trailingStop}\r\n" +
					$"trailingBorderFactor: {trailingBorderFactor}\r\ntrailingStopFactor: {trailingStopFactor}\r\n" +
					$"stableBigChangeFactor: {stableBigChangeFactor}\r\ntradeEnterProbMin: {tradeEnterProbabilityMin}\r\n" +
					$"rejectTradeProb: {rejectTradeProbability}\r\n" +
					$"minLossForCounterTrade: {minLossForCounterTrade}\r\nuseOptimizedLots: {useOptimizedLots}\r\n" +
					$"naxOrdersParallel: {maxOrdersParallel}\r\n" +
					$"minStableTrendBar: {minStableTrendBarForEnter}\r\nmaxStableTrendBar: {maxStableTrendBarForEnter}\r\n" +
					$"minTrade3PeriodBars: {minTradePeriodBars}\r\n" +
					$"counterTrading: {counterTrading}\r\ncounterMeasuedProbBars: {countedMeasuredProbabilityBars}\r\n" +
					$"minOrderDist: {minOrderDistance}\r\n" +
					$"orderAliveHours: {orderAliveHours}\r\ncollapseEnerLots: {collapseEnterLots}\r\ncounterTradeLots: {counterTradeLots}\r\n" +
					$"collapseChgPnts: {collapseChangePoints}"
				);

			}
		}

	}
}
