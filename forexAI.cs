using System;
using Color = System.Drawing.Color;
using NQuotes;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Logger;
using FANNCSharp.Double;
using System.Text.RegularExpressions;
using TicTacTec.TA.Library;

namespace forexAI
{
    public class ForexAI : MqlApi
    {
        private Random random = new Random((int) DateTimeOffset.Now.ToUnixTimeMilliseconds());
        private int inputDimension = 0;
        private string inputLayerActivationFunction, middleLayerActivationFunction;
        private int previousBars = 0;
        private double total;
        private double spends;
        private NeuralNet ai;
        private Storage storage = new Storage();
        private double profit;
        private string symbol = "";
        private int spend_sells = 0, spend_buys = 0, profitsells = 0, profitbuys = 0, tot_spends = 0, tot_profits = 0;
        private int ticket = 0, opnum = 0;
        private string l_name_8, type = "";
        private int startTime = 0;
        private string aiName;
        private int previousDay = 0;
        private int totalNeurons;

        public double this[string name]
        {
            get
            {
                return GlobalVariableGet(name);
            }
            set
            {
                GlobalVariableSet(name, value);
            }
        }

        //+------------------------------------------------------------------+
        //| Start function                                                   |
        //+------------------------------------------------------------------+
        public override int start()
        {
            if (Bars == previousBars)
                return 0;

            DrawStats();

            if (previousDay != Day())
            {
                previousDay = Day();
                log($"Day {previousDay} opnum {opnum}");
                opnum = 0;
            }

            AddText("SDF SD F");
            previousBars = Bars;
            ////---- calculate open orders by current symbol
            //string symbol = Symbol();

            //if (CalculateCurrentOrders() == 0)
            //    CheckForOpen(symbol);
            //else
            //    CheckForClose(symbol);

            return 0;
        }

        public override int deinit()
        {
            log("Deinitializing ...");
            debug($"Balance={AccountBalance()} Orders={OrdersTotal()}");

            storage.SyncData();

            string mins = (((GetTickCount() - startTime) / 1000.0 / 60.0)).ToString("0");
            debug($"Uptime {mins} mins");
            log("... done");
            return 0;
        }

        public override int init()
        {
            startTime = GetTickCount();

            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2018, 1, 6)
                                    .AddDays(version.Build).AddSeconds(version.Revision * 2);

            string displayableVersion = $"{version} ({buildDate})";
            log($"* Automatic Trading Extension for MT4 based on neural networks. (c) 2018 Sergey Efimov (deconf@ya.ru). ");
            log("Initializing ...");
            log($"Version: {displayableVersion}");
            log($"Company: [{TerminalCompany()}] Name: [{TerminalName()}] Path: [{TerminalPath()}]");
            log($"AccNumber={AccountNumber()} AccName=[{AccountName()}] Balance={AccountBalance()} Currency={AccountCurrency()} ");
            debug($"equity={AccountEquity()} marginMode={AccountFreeMarginMode()} expert={WindowExpertName()}");
            debug($"leverage={AccountLeverage()} server=[{AccountServer()}] stopoutLev={AccountStopoutLevel()} stopoutMod={AccountStopoutMode()}");

            symbol = Symbol();
            int runNum = (int) this["runNum"];
            debug($"IsOptimization={IsOptimization()} IsTesting={IsTesting()} runNum={runNum}");
            debug($"orders={OrdersTotal()} timeCurrent={TimeCurrent()} digits={MarketInfo(symbol, MODE_DIGITS)} spred={MarketInfo(symbol, MODE_SPREAD)}");
            debug($"tickValue={MarketInfo(symbol, MODE_TICKVALUE)} tickSize={MarketInfo(symbol, MODE_TICKSIZE)} minlot={MarketInfo(symbol, MODE_MINLOT)}" +
                $" lotStep={MarketInfo(symbol, MODE_LOTSTEP)}");

            int var_total = GlobalVariablesTotal();
            string name;
            for (int i = 0; i < var_total; i++)
            {
                name = GlobalVariableName(i);
                debug($"global var {name}={GlobalVariableGet(name)}");
            }

            this["runNum"] = runNum + 1;

            Data.db = new DB();

            if (Configuration.useMemcached)
                storage.UpMemcache();

            storage["random"] = random.Next(int.MaxValue);

            LoadNetworks();

            log($"... initialized in {(GetTickCount() - startTime) / 1000.0} sec.");

            return 0;
        }

        public void LoadNetwork(string dirName)
        {
            log($"Loading fann network [{dirName}]");

            aiName = dirName;

            ai = new NeuralNet($"d:\\temp\\forexAI\\{dirName}\\FANN.net");

            info($"Network: hash={ai.GetHashCode()} inputs={ai.InputCount} outputs={ai.OutputCount} neurons={ai.TotalNeurons}");

            totalNeurons = (int) ai.TotalNeurons;
            string fileTextData = File.ReadAllText($"d:\\temp\\forexAI\\{dirName}\\configuration.txt");
            Regex regex = new Regex(@"\[([^ \r\n\[\]]{1,10}?)\s+?", RegexOptions.Multiline | RegexOptions.Singleline);
            foreach (Match match in regex.Matches(fileTextData))
            {
                if (match.Groups[0].Value.Length <= 0)
                    continue;

                string funcName = match.Groups[0].Value.Trim('[', ' ');
                info($"* function [{funcName}]");

                Dictionary<string, string> data = new Dictionary<string, string>();

                data["name"] = funcName;

                if (Data.nnFunctions.ContainsKey(funcName))
                    continue;

                Data.nnFunctions.Add(funcName, data);
            }
            Match match2 = Regex.Match(fileTextData, "InputDimension:\\s+(\\d+)?");
            int.TryParse(match2.Groups[1].Value, out inputDimension);

            info($"* inputDimension = {inputDimension}");

            Match matchls = Regex.Match(fileTextData, "InputActFunc:\\s+([^ ]{1,40}?)\\s+LayerActFunc:\\s+([^ \r\n]{1,40})",
                 RegexOptions.Singleline);
            info($"* activation functions: input [{matchls.Groups[1].Value}] layer [{matchls.Groups[2].Value}]");

            inputLayerActivationFunction = matchls.Groups[1].Value;
            middleLayerActivationFunction = matchls.Groups[2].Value;
        }

        public void LoadNetworks()
        {
            DirectoryInfo d = new DirectoryInfo(Configuration.DataDirectory);
            DirectoryInfo[] Dirs = d.GetDirectories("*");

            log($"Looking for networks [{Configuration.DataDirectory}]: found {Dirs.Length} networks.");

            //foreach (DirectoryInfo dir in Dirs)
            //    storage[num++.ToString()] = $"{dir.Name}";

            LoadNetwork(Dirs[random.Next(Dirs.Length - 1)].Name);
        }

        private void AddText(string text)
        {
            string on;
            double pos = Bid + (Bid - Ask) / 2;
            pos = Open[0];
            on = (string) (pos.ToString());
            ObjectCreate(on, OBJ_TEXT, 0, iTime(Symbol(), 0, 0), pos);
            ObjectSetText(on, text, 8, "consolas", Color.Orange);
        }

        private double GetActiveProfit()
        {
            int orders = OrdersTotal();
            double total = 0.0;
            //Print("total orders: "+orders);
            for (int pos = 0; pos < orders; pos++)
            {
                if (OrderSelect(pos, SELECT_BY_POS, MODE_TRADES) == false)
                    continue;
                if (OrderProfit() > 0.0)
                    total = total + OrderProfit();
            }
            //   return (160);
            return (total);
        }

        private double GetActiveSpend()
        {
            int orders = OrdersTotal();
            double total = 0.0;
            //  Print("total orders: "+orders);
            for (int pos = 0; pos < orders; pos++)
            {
                if (OrderSelect(pos, SELECT_BY_POS, MODE_TRADES) == false)
                    continue;
                if (OrderProfit() < 0.0)
                    total = total + OrderProfit();
            }
            // return (170);
            return (total);
        }

        private double GetActiveIncome()
        {
            double total = 0.0;
            double spends;
            double profit;

            spends = GetActiveSpend();
            profit = GetActiveProfit();
            if (profit > spends)
                total = profit + (spends);
            else
                total = 0.0;
            return (total);
        }

        public void DrawStats()
        {
            int i;

            for (i = 0; i < 9; i++)
            {
                l_name_8 = "order" + i;
                if (ObjectFind(l_name_8) == -1)
                {
                    ObjectCreate(l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                    ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                    ObjectSet(l_name_8, OBJPROP_XDISTANCE, 300);
                    ObjectSet(l_name_8, OBJPROP_YDISTANCE, i * 10);
                }

                ObjectSetText(l_name_8, "                                    ", 8, "consolas", Color.White);
            }

            for (i = 0; i < OrdersTotal(); i++)
            {
                OrderSelect(i, SELECT_BY_POS, MODE_TRADES);
                if (OrderSymbol() == Symbol())
                {
                    if (OrderType() == OP_BUY)
                    {
                        type = "BUY";
                    }
                    if (OrderType() == OP_SELL)
                    {
                        ticket = OrderTicket();
                        type = "SELL";
                    }
                    if (OrderType() == OP_BUYLIMIT)
                    {
                        type = "BUY_LIMIT";
                    }
                    if (OrderType() == OP_SELLLIMIT)
                    {
                        type = "SELL_LIMIT";
                    }
                    if (OrderType() == OP_BUYSTOP)
                    {
                        type = "BUY_STOP";
                    }
                    if (OrderType() == OP_SELLSTOP)
                    {
                        type = "SELL_STOP";
                    }

                    l_name_8 = "order" + i;

                    ObjectSetText(l_name_8, "îðäåð " + type + " #" + i + " ïðîôèò " + DoubleToStr(OrderProfit(), 2), 8, "consolas", Color.White);
                }
            }

            string gs_80 = "text";
            // double ld_0 = GetProfitForDay(0);
            l_name_8 = gs_80 + "1";
            /* if (ObjectFind(l_name_8) == -1) {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, 0, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 15);
             }
             ObjectSetText(l_name_8, "Çàðàáîòîê ñåãîäíÿ: " + DoubleToStr(ld_0, 2), 8, "consolas", Yellow);
             ld_0 = GetProfitForDay(1);
             l_name_8 = gs_80 + "2";
             if (ObjectFind(l_name_8) == -1) {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, 0, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 30);
             }
             ObjectSetText(l_name_8, "Çàðàáîòîê â÷åðà: " + DoubleToStr(ld_0, 2), 8, "consolas", Yellow);
             ld_0 = GetProfitForDay(2);
             l_name_8 = gs_80 + "3";
             if (ObjectFind(l_name_8) == -1) {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, 0, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 45);
             }
             ObjectSetText(l_name_8, "Çàðàáîòîê ïîçàâ÷åðà: " + DoubleToStr(ld_0, 2), 8, "consolas", Yellow);*/
            l_name_8 = gs_80 + "4";
            if (ObjectFind(l_name_8) == -1)
            {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 0);
            }
            ObjectSetText(l_name_8, "AccountEquity: " + DoubleToStr(AccountEquity(), 2), 8, "consolas", Color.Yellow);

            l_name_8 = gs_80 + "5";
            if (ObjectFind(l_name_8) == -1)
            {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 10);
            }

            total = GetActiveProfit();
            ObjectSetText(l_name_8, "ActiveProfit: " + DoubleToStr(total, 2), 8, "consolas", Color.Yellow);

            l_name_8 = gs_80 + "6";
            if (ObjectFind(l_name_8) == -1)
            {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 20);
            }

            total = GetActiveSpend();
            ObjectSetText(l_name_8, "ActiveSpend: " + DoubleToStr(total, 2), 8, "consolas", Color.Yellow);

            total = GetActiveIncome();
            l_name_8 = gs_80 + "7";
            if (ObjectFind(l_name_8) == -1)
            {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 30);
            }
            ObjectSetText(l_name_8, "ActiveIncome: " + DoubleToStr(total, 2), 8, "consolas", Color.Yellow);

            spends = GetActiveSpend();
            profit = GetActiveProfit();
            spends = (0 - (spends));
            // Print("profit:",profit," spends:", spends);
            if (profit > 0.0 && spends >= 0.0)
                total = (0 - ((profit) * 100.0) / spends);
            else
                total = 0;

            l_name_8 = gs_80 + "8";
            if (ObjectFind(l_name_8) == -1)
            {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 40);
            }
            ObjectSetText(l_name_8, "kpd %: " + DoubleToStr(total, 0) + "%", 8, "consolas", Color.Yellow);

            l_name_8 = gs_80 + "9";
            if (ObjectFind(l_name_8) == -1)
            {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 50);
            }

            ObjectSetText(l_name_8, "opnum: " + opnum, 8, "consolas", Color.Yellow);

            l_name_8 = gs_80 + "10";
            if (ObjectFind(l_name_8) == -1)
            {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 60);
            }
            ObjectSetText(l_name_8, "OrdersTotal: " + OrdersTotal(), 8, "consolas", Color.Yellow);

            string dirtext = "", dirtext2 = "";
            l_name_8 = gs_80 + "11";
            if (ObjectFind(l_name_8) == -1)
            {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 70);
            }
            ObjectSetText(l_name_8, "dirtext: " + dirtext, 8, "consolas", Color.Yellow);

            l_name_8 = gs_80 + "12";
            if (ObjectFind(l_name_8) == -1)
            {
                ObjectCreate(l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(l_name_8, OBJPROP_YDISTANCE, 80);
            }
            ObjectSetText(l_name_8, "dirtext2: " + dirtext2, 8, "consolas", Color.Yellow);

            tot_spends = spend_sells + spend_buys;
            tot_profits = profitsells + profitbuys;
            string d = "0";

            if (tot_spends > 0 && tot_profits > 0)
                d = DoubleToStr(100 - ((100.0 / tot_profits) * tot_spends), 2);

            string funcsString = "";
            foreach (var func in Data.nnFunctions)
            {
                funcsString += $"|{func.Key}";
            }
            Comment(
               "profitsells: " + profitsells + "\r\n" +
               "spend_sells:  " + spend_sells + "\r\n"
              + "profitbuys:   " + profitbuys + "\r\n"
              + "spend_buys:    " + spend_buys + "\r\n"
              + "tot_profits: " + DoubleToStr(tot_profits, 0) + "\r\n" +
               "tot_spends:  " + DoubleToStr(tot_spends, 0) + "\r\n" +
               "КПД: " + d + "%" + "\r\n---------------\r\n" +
               "Network: " + aiName + "\r\n" +
               "Functions: " + funcsString + "\r\n" +
               "InputDimension: " + inputDimension + "\r\n" +
               "Neurons: " + totalNeurons + "\r\n" +
               "Input: " + ai.InputCount + "\r\n" +
               "InputActFunc: " + inputLayerActivationFunction + "\r\n" +
               "LayerActFunc: " + middleLayerActivationFunction);

            if (ObjectFind("statyys") == -1)
            {
                ObjectCreate("statyys", OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet("statyys", OBJPROP_CORNER, 0);
                ObjectSet("statyys", OBJPROP_XDISTANCE, 150);
                ObjectSet("statyys", OBJPROP_YDISTANCE, 16);
            }

            WindowRedraw();
        }

        //+------------------------------------------------------------------+
        //| Calculate open positions                                         |
        //+------------------------------------------------------------------+
        //private int CalculateCurrentOrders()
        //{
        //    int buys = 0, sells = 0;

        //    for (int i = 0; i < OrdersTotal(); i++)
        //    {
        //        if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
        //            break;
        //        if (OrderSymbol() == Symbol() && OrderMagicNumber() == MAGICMA)
        //        {
        //            if (OrderType() == OP_BUY)
        //                buys++;
        //            if (OrderType() == OP_SELL)
        //                sells++;
        //        }
        //    }

        //    //---- return orders volume
        //    if (buys > 0)
        //        return (buys);
        //    return (-sells);
        //}

        ////+------------------------------------------------------------------+
        ////| Check for close order conditions                                 |
        ////+------------------------------------------------------------------+
        //private void CheckForClose(string symbol)
        //{
        //    //---- go trading only for first tiks of new bar
        //    if (Volume[0] > 1)
        //        return;
        //    //---- get Moving Average
        //    double ma = iMA(symbol, 0, MovingPeriod, MovingShift, MODE_SMA, PRICE_CLOSE, 0);

        //    Print("orders " + OrdersTotal());

        //    for (int i = 0; i < OrdersTotal(); i++)
        //    {
        //        if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
        //            break;
        //        if (OrderMagicNumber() != MAGICMA || OrderSymbol() != Symbol())
        //            continue;
        //        //---- check order type
        //        if (OrderType() == OP_BUY)
        //        {
        //            log("test  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" + OrderProfit());
        //            if (Open[1] > ma && Close[1] < ma)
        //                OrderClose(OrderTicket(), OrderLots(), Bid, 3, Color.White);

        //            break;
        //        }
        //        if (OrderType() == OP_SELL)
        //        {
        //            log("test  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" + OrderProfit());
        //            if (Open[1] < ma && Close[1] > ma)
        //                OrderClose(OrderTicket(), OrderLots(), Ask, 3, Color.White);

        //            break;
        //        }
        //    }
        //}

        ////+------------------------------------------------------------------+
        ////| Check for open order conditions                                  |
        ////+------------------------------------------------------------------+
        //private void CheckForOpen(string symbol)
        //{
        //    //---- go trading only for first tiks of new bar
        //    if (Volume[0] > 1)
        //        return;

        //    //---- get Moving Average
        //    double ma = iMA(symbol, 0, MovingPeriod, MovingShift, MODE_SMA, PRICE_CLOSE, 0);

        //    //---- sell conditions
        //    if (Open[1] > ma && Close[1] < ma)
        //    {
        //        OrderSend(Symbol(), OP_SELL, LotsOptimized(), Bid, 3, 0, 0, "", MAGICMA, DateTime.MinValue, Color.Red);
        //        return;
        //    }
        //    //---- buy conditions
        //    if (Open[1] < ma && Close[1] > ma)
        //    {
        //        OrderSend(Symbol(), OP_BUY, LotsOptimized(), Ask, 3, 0, 0, "", MAGICMA, DateTime.MinValue, Color.Blue);
        //    }
        //}

        ////+------------------------------------------------------------------+
        ////| Calculate optimal lot size                                       |
        ////+------------------------------------------------------------------+
        //private double LotsOptimized()
        //{
        //    // history orders total
        //    int orders = OrdersHistoryTotal();
        //    // number of losses orders without a break
        //    int losses = 0;
        //    //---- select lot size
        //    double lot = NormalizeDouble(AccountFreeMargin() * MaximumRisk / 1000.0, 1);
        //    //---- calcuulate number of losses orders without a break
        //    if (DecreaseFactor > 0)
        //    {
        //        for (int i = orders - 1; i >= 0; i--)
        //        {
        //            if (!OrderSelect(i, SELECT_BY_POS, MODE_HISTORY))
        //            {
        //                Print("Error in history!");
        //                break;
        //            }
        //            if (OrderSymbol() != Symbol() || OrderType() > OP_SELL)
        //                continue;

        //            if (OrderProfit() > 0)
        //                break;
        //            if (OrderProfit() < 0)
        //                losses++;
        //        }
        //        if (losses > 1)
        //            lot = NormalizeDouble(lot - lot * losses / DecreaseFactor, 1);
        //    }
        //    //---- return lot size
        //    if (lot < 0.1)
        //        lot = 0.1;
        //    return (lot);
        //}
    }
}