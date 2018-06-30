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
using DataType = System.Double;

namespace forexAI
{
	public class ForexAI : MqlApi
	{
		[ExternVariable]
		public double orderLots = 0.01;

		[ExternVariable]
		public double maxNegativeSpend = -8;

		[ExternVariable]
		public double trailingBorder = 30;

		[ExternVariable]
		public double trailingStop = 15;

		[ExternVariable]
		public double stableBigChangeFactor = 0.2;

		[ExternVariable]
		public double enteringTradeProbability = 0.9;

		[ExternVariable]
		public double blockingTradeProbability = -0.2;

		[ExternVariable]
		public double minLossForCounterTrade = -5.0;

		[ExternVariable]
		public bool useOptimizedLots = true;

		[ExternVariable]
		public int maxOrdersInParallel = 10;

		[ExternVariable]
		public int minStableTrendBarForEnter = 2;

		[ExternVariable]
		public int maxStableTrendBarForEnter = 50;

		[ExternVariable]
		public int minTradePeriodBars = 6;

		[ExternVariable]
		public bool counterTrading = true;

		[ExternVariable]
		public int countedMeasuredProbabilityBars = 2;

		[ExternVariable]
		public double maxOrderOpenHours = 8.5;

		[ExternVariable]
		public double minOrderDistance = 0.003;

		[ExternVariable]
		public double orderAliveHours = 4;

		[ExternVariable]
		public double collapseEnterLots = 1.0;

		[ExternVariable]
		public double counterTradeLots = 0.02;

		[ExternVariable]
		public double collapseChangePoints = 0.0043;

		//  props
		Random random = new Random((int) DateTimeOffset.Now.ToUnixTimeMilliseconds() + 314);
		Settings settings = new Settings();
		List<Order> activeOrders = new List<Order>();
		List<Order> historyOrders = new List<Order>();
		NeuralNet forexFannNetwork = null;
		DirectoryInfo[] networkDirs = null;
		Storage storage = new Storage();
		PerformanceCounter cpuCounter = null;
		Process currentProcess = null;
		TrainingData trainData = null;
		TrainingData testData = null;
		Stopwatch runTimer = null;
		Version version = null;

		// MqlApi object (this)
		MqlApi mqlApi = null;

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
		double minStopLevel = 0;
		double ordersStopPoints = 0;
		double risky2_Risk = 2;
		double risky2_Lots = 0.04;
		double risky2_LotDigits = 2;
		double minlot = 0.01;
		double maxlot = 0.08;
		double leverage = 0.0;
		double lotsize = 0.01;
		double stoplevel = 0.0;
		double[] fannNetworkOutput = null;
		double[] prevBuyProbability = null;
		double[] prevSellProbability = null;
		float testMse = 0.0f;
		float trainMse = 0.0f;
		long lastDrawStatsTimestamp = 0;
		long lastMemoryStatsDump = 0;
		bool reassembleStageOverride = true;
		bool hasNoticedLowBalance = false;
		bool profitTrailing = true;
		bool hasNightReported = false;
		bool hasMorningReported = false;
		bool neuralNetworkBootstrapped = false;
		bool hasIncreasedUnstableTrendBar = false;
		int stableTrendCurrentBar = 0;
		int spendSells = 0;
		int spendBuys = 0;
		int profitSells = 0;
		int profitBuys = 0;
		int totalSpends = 0;
		int totalProfits = 0;
		int openedBuys = 0;
		int openedSells = 0;
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
		int marketCollapsedBar = 0;

		// computed properties
		int ordersCount => activeOrders.Count();
		int tradeBarPeriodGone => Bars - lastTradeBar;
		double buyProbability => fannNetworkOutput == null ? 0.0 : fannNetworkOutput[0];
		double sellProbability => fannNetworkOutput == null ? 0.0 : fannNetworkOutput[1];
		double orderProfit => buyProfit + sellProfit;
		TrendDirection collapseDirection => High[0] - Low[0] > 0.0 ? TrendDirection.Up : TrendDirection.Down;
		double diffProbability => sellProbability + buyProbability;
		int buyCount => activeOrders.Where(o => o.type == Constants.OrderType.Buy).Count();
		int sellCount => activeOrders.Where(o => o.type == Constants.OrderType.Sell).Count();

		double activeIncome
		{
			get
			{
				double total = 0.0;
				double spends = activeLoss;
				double profit = activeProfit;

				if (profit + spends >= 0.0)
					total = profit + spends;

				return total > 0 ? total : 0;
			}
		}

		double activeProfit
		{
			get
			{
				double total = 0.0;

				foreach (var order in activeOrders)
				{
					var orderTotal = order.profit + order.commission + order.swap;

					if (orderTotal > 0.0)
						total += orderTotal;
				}

				return total;
			}
		}

		double buyProfit
		{
			get
			{
				double buyIncome = 0.0;

				Helpers.Each(activeOrders.Where(o => o.type == Constants.OrderType.Buy), delegate (Order order)
				{
					buyIncome += order.profit + order.commission + order.swap;
				});

				return buyIncome;
			}
		}

		double sellProfit
		{
			get
			{
				double sellIncome = 0.0;

				Helpers.Each(activeOrders.Where(o => o.type == Constants.OrderType.Sell), delegate (Order order)
				{
					sellIncome += order.profit + order.commission + order.swap;
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

				foreach (var order in activeOrders)
				{
					var orderTotal = order.profit + order.commission + order.swap;
					if (orderTotal < 0.0)
						loss += orderTotal;
				}

				return loss;
			}
		}

		public double lotsOptimized3
		{
			get
			{
				double lots = MathMin(AccountBalance(), AccountFreeMargin()) * 0.01 / 1000 / (MarketInfo(symbol, MODE_TICKVALUE));
				if (lots < 0.01)
					lots = 0.01;
				return lots;
			}
		}

		public double lotsOptimized2
		{
			get
			{
				leverage = AccountLeverage();
				double MinLots = lotsize;
				double MaximalLots = 0.08;
				double lots = risky2_Lots;

				lots = NormalizeDouble(AccountFreeMargin() * risky2_Risk / 100 / 1000.0, 5);

				if (lots < minlot) lots = minlot;

				if (lots > MaximalLots) lots = MaximalLots;

				if (AccountFreeMargin() < Ask * lots * lotsize / leverage)
					consolelog("fail to calc lots, too les money");
				else
					lots = NormalizeDouble(risky2_Lots, Digits);

				return lots;
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
						if (OrderSymbol() != symbol || OrderType() > OP_SELL)
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
				foreach (var order in activeOrders)
				{
					if (!OrderSelect(order.ticket, SELECT_BY_TICKET) || OrderType() != OP_BUY || OrderCloseTime() != new DateTime(0))
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
				foreach (var order in activeOrders)
				{
					if (!OrderSelect(order.ticket, SELECT_BY_TICKET) || OrderType() != OP_SELL || OrderCloseTime() != new DateTime(0))
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
			if (runTimer == null)
			{
				runTimer = new Stopwatch();
				runTimer.Start();
			}
			currentDay = (int) DateTime.Now.DayOfWeek;

			console($"--------------[ START tick={startTime} day={currentDay} ]-----------------",
				ConsoleColor.Black, ConsoleColor.Cyan);

			console($"> Initializing (with Net.framework={Environment.Version.ToString()}) ...");

			mqlApi = this;

			neuralNetworkBootstrapped = false;
			reassembleStageOverride = false;

			version = Assembly.GetExecutingAssembly().GetName().Version;
			ShowBanner();

			prevBuyProbability = new double[countedMeasuredProbabilityBars];
			prevSellProbability = new double[countedMeasuredProbabilityBars];

			if (IsOptimization())
				Configuration.useAudio = false;

			ClearLogs();
			EraseLogs(Configuration.XXRandomLogFileName, Configuration.YYYRandomLogFileName);

			console($"orderLots={orderLots} maxNegativeSpend={maxNegativeSpend} trailingBorder={trailingBorder} trailingStop={trailingStop}" +
				$" stableBigChangeFactor={stableBigChangeFactor} enteringTradeProbability={enteringTradeProbability} BlockingTradeProbability={blockingTradeProbability}" +
				$" MinLossForCounterTrade={minLossForCounterTrade} useOptimizedLots={useOptimizedLots} maxOrdersInParallel={maxOrdersInParallel}" +
				$" minStableTrendBarForEnter={minStableTrendBarForEnter} maxStableTrendBarForEnter={maxStableTrendBarForEnter} " +
				$"minTradePeriodBars={minTradePeriodBars} counterTrading={counterTrading}", ConsoleColor.Black, ConsoleColor.Yellow);

			console($"Scanning processor resources ...");
			cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

			console($"Set Core.Compatibility.Metastock");
			Core.SetCompatibility(Core.Compatibility.Metastock);
			//Core.SetUnstablePeriod(Core.FuncUnstId.FuncUnstNone, 10);

			#region matters
			if ((Environment.MachineName == "USER-PC" || (Experimental.IsHardwareForcesConnected() == Experimental.IsBlackHateFocused()))
				&& (currentDay == 0) && false)
				Configuration.tryExperimentalFeatures = true;
			#endregion

			console($"Init variables ...");
			InitVariables();
			ListGlobalVariables();
			console($"Scanning networks ...");
			ScanNetworks();

			if (networkDirs.Length > 0)
			{
				string network = networkDirs[random.Next(networkDirs.Length - 1)].Name;
				console($"Loading network [{network}]");
				LoadNetwork(network);
				if (forexFannNetwork != null)
				{
					console($"Testing network MSE ");
					TestNetworkMSE();
					console($"Testing network hit ratio ");
					TestNetworkHitRatio();
				}
			}

			DumpInfo();

			string initStr = $"Initialized in {(((double) GetTickCount() - (double) startTime) / 1000.0).ToString("0.0")} sec(s) (v{version})";
			log(initStr);
			console(initStr, ConsoleColor.Black, ConsoleColor.Yellow);

			reassembleStageOverride = false;
			neuralNetworkBootstrapped = true;

			return 0;
		}

		public override int start()
		{
			SyncOrders();
			CheckForAutoClose();
			CheckForMarketCollapse();
			CloseNegativeOrders();
			TrailPositions();

			if (!IsOptimization())
			{
				if (runTimer.ElapsedMilliseconds - lastDrawStatsTimestamp >= 600)
				{
					CalculateHistoryOrders();
					DrawStats();
					lastDrawStatsTimestamp = runTimer.ElapsedMilliseconds;
				}

				if (runTimer.ElapsedMilliseconds - lastMemoryStatsDump >= 380000)
				{
					lastMemoryStatsDump = runTimer.ElapsedMilliseconds;
					Helpers.ShowMemoryUsage();
				}
			}

			if (Bars == previousBars)
				return 0;

			if (forexFannNetwork != null && neuralNetworkBootstrapped)
			{
				(networkFunctionsCount, fannNetworkOutput) = Reassembler.Execute(functionsTextContent,
					inputDimension, forexFannNetwork, reassembleStageOverride, mqlApi);

				EnterTrade();

				if (counterTrading)
					EnterCounterTrade();
			}

			if (!IsOptimization())
				RenderVisualizedzedHistory();

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
				AudioFX.TheNewDay();
			}

			if (AccountBalance() <= 35.0 && !hasNoticedLowBalance)
			{
				hasNoticedLowBalance = true;
				console($"всё пизда, кеш весь слит нахуй, бабок: {AccountBalance()}$", ConsoleColor.Red, ConsoleColor.White);
				AudioFX.LowBalance();
				TerminalClose(0);
			}
			else if (hasNoticedLowBalance && YRandom.Next(6) == 3)
				AudioFX.GoodWork();

			log($"=> Probability: Buy={buyProbability.ToString("0.0000")} Sell={sellProbability.ToString("0.0000")}", "debug");

			#region matters
			if (Configuration.tryExperimentalFeatures)
				AlliedInstructions();
			#endregion

			previousBars = Bars;
			barsPerDay += 1;

			return 0;
		}

		private void CheckForAutoClose()
		{
			if (activeLoss + activeProfit >= 0.0 && (buyProfit < 0.0 || sellProfit < 0.0) && ordersCount >= 2)
			{
				log($"auto-close profit activeIncome:{activeIncome} activeLoss:{activeLoss} " +
					$" buyProft:{buyProfit} sellProfit:{sellProfit} ordersCount:{ordersCount}", "debug");
				CloseAllOrders();
			}
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

		private void CloseAllOrders()
		{
			CloseBuys();
			CloseSells();
		}

		private void CheckForMarketCollapse()
		{
			var change = Math.Max(High[0], Low[0]) - Math.Min(High[1], Low[1]);
			if ((change >= collapseChangePoints) && Bars - marketCollapsedBar >= 3)
			{
				console($"Market collapse detected on {TimeCurrent()} change: {change.ToString("0.00000")}, going {collapseDirection}",
					ConsoleColor.Black, ConsoleColor.Green);

				AddVerticalLabel($"Market collapse {collapseDirection}", Color.Aquamarine);

				marketCollapsedBar = Bars;
				if (collapseDirection == TrendDirection.Up)
					SendBuy(lotsOptimized2);
				else
					SendSell(lotsOptimized2);
			}
		}


		public void SyncOrders()
		{
			var zeroTime = new DateTime(0);
			var now = TimeCurrent();

			foreach (var order in activeOrders)
			{
				if (OrderSelect(order.ticket, SELECT_BY_TICKET) == true
					&& OrderCloseTime() != zeroTime && OrderProfit() > 0.0)
				{
					AudioFX.Profit();
					consolelog($"profit {order.type.ToString()} {OrderProfit()}$ lots {OrderLots()} " +
						$"(total={AccountBalance().ToString("0.00")}, spends={activeLoss}, profit={activeProfit})");
				}
				else if (OrderSelect(order.ticket, SELECT_BY_TICKET) == true
					&& OrderCloseTime() != zeroTime && OrderProfit() < 0.0)
				{
					AudioFX.Fail();
					consolelog($"loss {order.type.ToString()} {OrderProfit()}$ lots {OrderLots()} " +
						$"(total={AccountBalance().ToString("0.00")}, spends={activeLoss}, profit={activeProfit})");
				}
			}

			activeOrders = activeOrders.Where(order => OrderSelect(order.ticket, SELECT_BY_TICKET) == true && OrderCloseTime() == zeroTime).ToList();

			for (int index = 0; index < OrdersTotal(); index++)
			{
				if (!OrderSelect(index, SELECT_BY_POS, MODE_TRADES))
					break;

				int orderTicket = OrderTicket();

				var activeOrder = (from order in activeOrders
								   where order.ticket == orderTicket
								   select order).DefaultIfEmpty(null).FirstOrDefault();

				if (activeOrder == null)
				{
					var order = new Order
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
						ageInMinutes = now.Subtract(OrderOpenTime()).TotalMinutes,
						expiration = OrderExpiration(),
						magickNumber = OrderMagicNumber()
					};

					if (OrderType() == OP_BUY)
						order.type = Constants.OrderType.Buy;
					else if (OrderType() == OP_SELL)
						order.type = Constants.OrderType.Sell;

					activeOrders.Add(order);
					historyOrders.Add(order);
				}
				else
				{
					activeOrder.profit = OrderProfit();
					activeOrder.commission = OrderCommission();
					activeOrder.swap = OrderSwap();
					activeOrder.stopLoss = OrderStopLoss();
					activeOrder.takeProfit = OrderTakeProfit();
					activeOrder.ageInMinutes = now.Subtract(activeOrder.openTime).TotalMinutes;
					activeOrder.expiration = OrderExpiration();
				}
			}
		}

		private void CalculateHistoryOrders()
		{
			profitBuys = 0;
			spendBuys = 0;
			profitSells = 0;
			spendSells = 0;

			foreach (var order in historyOrders)
			{
				if (OrderSelect(order.ticket, SELECT_BY_TICKET, MODE_HISTORY))
				{
					if (OrderCloseTime() != new DateTime(0))
					{
						if (order.type == Constants.OrderType.Buy)
						{
							if (OrderProfit() + OrderCommission() + OrderSwap() > 0)
								profitBuys++;
							else
								spendBuys++;
						}

						if (order.type == Constants.OrderType.Sell)
						{
							if (OrderProfit() + OrderCommission() + OrderSwap() > 0)
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

			consolelog($"machineName={currentProcess.MachineName}");

			minStopLevel = MarketInfo(symbol, MODE_STOPLEVEL);
			if (trailingStop < minStopLevel)
				warning($"minStopLevel={minStopLevel}, while trailingStop={trailingStop}, reducing.");

			ordersStopPoints = minStopLevel > 0 ? minStopLevel * 2 : 60;

			if (Configuration.useMysql)
				Data.database = new Database();

			settings["yrandom"] = YRandom.Next(int.MaxValue);
			settings["random"] = random.Next(int.MaxValue);
		}

		public void EnterTrade()
		{
			RefreshRates();

			if (!isTrendStable
				|| stableTrendBar < minStableTrendBarForEnter)
				return;

			if (buyProbability >= enteringTradeProbability
					&& sellProbability <= blockingTradeProbability
					&& diffProbability >= 0
					&& ordersCount < maxOrdersInParallel
					&& tradeBarPeriodGone > minTradePeriodBars
					&& closestBuyDistance >= minOrderDistance)
				SendBuy(lotsOptimized2);

			if (sellProbability >= enteringTradeProbability
					&& buyProbability <= blockingTradeProbability
					&& diffProbability <= 0
					&& ordersCount < maxOrdersInParallel
					&& tradeBarPeriodGone > minTradePeriodBars
					&& closestSellDistance >= minOrderDistance)
				SendSell(lotsOptimized2);
		}

		public void EnterCounterTrade()
		{
			RefreshRates();

			if (buyProfit <= minLossForCounterTrade
				&& ordersCount < maxOrdersInParallel
				&& tradeBarPeriodGone > minTradePeriodBars
				&& collapseDirection == TrendDirection.Down
				&& closestSellDistance >= minOrderDistance
				&& sellProbability >= 0.2)
			{
				consolelog($"opening counter-sell [{sellCount}/{ordersCount}] lastOrder@{tradeBarPeriodGone}");
				SendSell(lotsOptimized3);
			}

			if (sellProfit <= minLossForCounterTrade
				&& ordersCount < maxOrdersInParallel
				&& tradeBarPeriodGone > minTradePeriodBars
				&& collapseDirection == TrendDirection.Up
				&& closestBuyDistance >= minOrderDistance
				&& buyProbability >= 0.2)
			{
				consolelog($"opening counter-buy [{buyCount}/{ordersCount}] lastOrder@{tradeBarPeriodGone}");
				SendBuy(lotsOptimized3);
			}
		}

		public void ListGlobalVariables()
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
				ErrorLog = new FANNCSharp.FannFile($"{Configuration.rootDirectory}\\fann.log", "a+")
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

			(networkFunctionsCount, fannNetworkOutput) = Reassembler.Execute(functionsTextContent, inputDimension, forexFannNetwork, true, mqlApi);
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

			foreach (var order in activeOrders)
			{
				if (order.profit + order.commission + order.swap >= maxNegativeSpend
						&& order.ageInMinutes / 60 <= orderAliveHours)
					continue;

				AudioFX.Fail();

				if (order.type == Constants.OrderType.Buy)
				{
					if (Configuration.tryExperimentalFeatures)
						console($"с{new String('y', random.Next(1, 3))}{new String('ч', random.Next(0, 2))}к{new String('a', random.Next(1, 2))} бля проёбано {OrderProfit()}$",
							ConsoleColor.Black, ConsoleColor.Red);

					spendBuys++;

					OrderClose(order.ticket, order.lots, Bid, 3, Color.White);

					consolelog("close buy " + order.ticket + " bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" +
						(order.profit + order.commission + order.swap));

					dayOperationsCount++;
				}
				if (order.type == Constants.OrderType.Sell)
				{
					if (Configuration.tryExperimentalFeatures)
						console($"с{new String('y', random.Next(1, 3))}{new String('ч', random.Next(0, 2))}к{new String('a', random.Next(1, 2))} бля проёбано {OrderProfit()}$",
							ConsoleColor.Black, ConsoleColor.Red);

					spendSells++;

					OrderClose(order.ticket, order.lots, Ask, 3, Color.White);

					consolelog("close sell " + OrderTicket() + "  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() +
						" profit=" + (order.profit + order.commission + order.swap));

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

				if (OrderType() == OP_BUY && OrderSymbol() == symbol)
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

				if (OrderType() == OP_SELL && OrderSymbol() == symbol)
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
			double stopLoss = 0;// Ask - ordersStopPoints * Point;
			DateTime expirationTime = TimeCurrent();
			expirationTime = expirationTime.AddHours(3);

			if (OrderSend(symbol, OP_SELL, exactLots > 0.0 ? exactLots : lotsOptimized, Bid, 50, stopLoss, 0, $"Probability:",
				Configuration.magickNumber, expirationTime, Color.Red) <= 0)
				log($"error sending sell: {GetLastError()} balance={AccountBalance()} lots={lotsOptimized}");
			else
				log($"open sell  prob:{sellProbability.ToString("0.00")} @" + Bid);

			AddLabel($"SP {sellProbability.ToString("0.0")} BP {buyProbability.ToString("0.0")}", Color.Red);

			dayOperationsCount++;
			lastTradeBar = Bars;
		}

		void SendBuy(double exactLots = 0.0)
		{
			double stopLoss = 0;//Bid - ordersStopPoints * Point;
			DateTime expirationTime = TimeCurrent();
			expirationTime = expirationTime.AddHours(3);

			if (OrderSend(symbol, OP_BUY, exactLots > 0.0 ? exactLots : lotsOptimized, Ask, 50, stopLoss, 0, $"Probability:",
				Configuration.magickNumber, expirationTime, Color.Blue) <= 0)
				log($"error sending buy: {GetLastError()} balance={AccountBalance()} lots={lotsOptimized}");
			else
				log($"open buy  prob:{buyProbability.ToString("0.00")} @" + Ask);

			AddLabel($"BP {buyProbability.ToString("0.0")} SP {sellProbability.ToString("0.0")}", Color.Blue);

			dayOperationsCount++;
			lastTradeBar = Bars;
		}

		void TrailPositions()
		{
			double TrailingStop = trailingStop;
			double TrailingBorder = trailingBorder;
			double newStopLoss = 0;

			if (TrailingStop < minStopLevel)
				TrailingStop = minStopLevel;

			RefreshRates();

			foreach (var order in activeOrders)
			{
				if (order.type == Constants.OrderType.Buy)
				{
					newStopLoss = Bid - TrailingStop * Point;
					if ((order.stopLoss == 0.0 || newStopLoss > order.stopLoss)
						&& Bid - (TrailingBorder * Point) > order.openPrice
						&& order.profit + order.commission + order.swap > 0)
					{
						log($"modify buy {order.ticket} newStopLoss={newStopLoss}");
						OrderModify(order.ticket, order.openPrice, newStopLoss, order.takeProfit,
							order.expiration, Color.BlueViolet);
						dayOperationsCount++;
					}
				}
				if (order.type == Constants.OrderType.Sell)
				{
					newStopLoss = Ask + TrailingStop * Point;
					if ((order.stopLoss == 0.0 || newStopLoss < order.stopLoss)
						&& Ask + (TrailingBorder * Point) < order.openPrice
						&& order.profit + order.commission + order.swap > 0)
					{
						log($"modify sell {OrderTicket()} newStopLoss={newStopLoss}");
						OrderModify(order.ticket, order.openPrice, newStopLoss, order.takeProfit,
							order.expiration, Color.MediumVioletRed);
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

			for (int index = OrdersHistoryTotal() - 3; index < OrdersHistoryTotal(); index++)
			{
				if (OrderSelect(index, SELECT_BY_POS, MODE_HISTORY))
				{
					if (OrderProfit() > 0.0)
					{
						if (OrderType() == OP_BUY)
						{
							profitBuys++;
							charizedOrdersHistory += $"[B={OrderProfit()}|{(OrderCloseTime() - OrderOpenTime()).TotalHours.ToString("0.00")}|{OrderLots()}] ";
						}
						if (OrderType() == OP_SELL)
						{
							profitSells++;
							charizedOrdersHistory += $"[S={OrderProfit()}|{(OrderCloseTime() - OrderOpenTime()).TotalHours.ToString("0.00")}|{OrderLots()}] ";
						}
					}
					else
					{
						charizedOrdersHistory += OrderType() == OP_BUY ? "-" : "-";

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
			consolelog($"> Automated Expert for MT4 using neural network with strategy created by code/data fuzzing. [met8]");
			consolelog($"> (c) 2018 Deconf (kilitary@gmail.com teleg:@deconf skype:serjnah icq:401112)");
			consolelog($"Initializing version {version} ...");
		}

		void DrawStats()
		{
			int i;

			for (i = 0; i < ordersCount; i++)
			{
				labelID = "order" + i;
				if (ObjectFind(labelID) == -1)
				{
					ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
					ObjectSet(labelID, OBJPROP_CORNER, 1);
					ObjectSet(labelID, OBJPROP_XDISTANCE, 1344);
					ObjectSet(labelID, OBJPROP_YDISTANCE, i * 18);
				}
				ObjectSetText(labelID, "                                    ", 8, "lucida console", Color.White);
			}

			int index = 0;
			foreach (var order in activeOrders)
			{
				if (order.type == Constants.OrderType.Buy)
					type = "BUY";
				if (order.type == Constants.OrderType.Sell)
					type = "SELL";

				//if (OrderType() == OP_BUYLIMIT)
				//	type = "BUY_LIMIT";
				//if (OrderType() == OP_SELLLIMIT)
				//	type = "SELL_LIMIT";
				//if (OrderType() == OP_BUYSTOP)
				//	type = "BUY_STOP";
				//if (OrderType() == OP_SELLSTOP)
				//	type = "SELL_STOP";

				OrderSelect(order.ticket, SELECT_BY_TICKET, MODE_TRADES);
				labelID = "order" + index;

				var now = TimeCurrent();
				var elapsed = now.Subtract(order.openTime);
				var profit = order.profit + order.commission + order.swap;

				ObjectSetText(labelID, type + " " +
					profit.ToString("0.00") + $" ({order.lots.ToString("0.00")} lots, " +
					$" age {(order.ageInMinutes / 60).ToString("0.0")} hours)", 8, "consolas",
					profit > 0.0 ? Color.LightGreen : Color.Red);
				index++;
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

			ObjectSetText(labelID, "activeLoss: " + DoubleToStr(activeLoss, 2), 8, "liberation mono", Color.Red);

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
				ObjectSet(labelID, OBJPROP_XDISTANCE, 1288);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 674);
			}

			ObjectSetText(labelID,
						  $"{charizedOrdersHistory}",
						  12,
						  "consolas",
						  Color.LimeGreen);

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
			ObjectSetText(labelID, "Buy Prob. " + buyProbability.ToString("0.00"), 17, "liberation mono",
				buyProbability > 0.0 ? Color.LightCyan : Color.Gray);

			labelID = gs_80 + "12";
			if (ObjectFind(labelID) == -1)
			{
				ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
				ObjectSet(labelID, OBJPROP_CORNER, 1);
				ObjectSet(labelID, OBJPROP_XDISTANCE, 15);
				ObjectSet(labelID, OBJPROP_YDISTANCE, 442);
			}
			ObjectSetText(labelID, "Sell Prob. " + sellProbability.ToString("0.00"), 17, "liberation mono",
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
				$" {(isTrendStable ? stableTrendBar : unstableTrendBar)}" +
				$" : {diffProbability.ToString("0.00")}]", 17, "liberation mono",
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
			  "Network: " +
			   networkName +
			   "\r\n" +
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
