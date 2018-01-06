using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FANNCSharp.Double;
using Newtonsoft.Json;
using NQuotes;
using TicTacTec.TA.Library;
using static forexAI.Logger;
using Color = System.Drawing.Color;

namespace forexAI
{
    public class ForexAI : MqlApi
    {
        private NeuralNet aiNetwork;
        private string aiName;
        private string dirName;
        private int inputDimension = 0;
        private string inputLayerActivationFunction, middleLayerActivationFunction;
        private string l_name_8, type = string.Empty;
        private int previousBars = 0;
        private int previousDay = 0;
        private double profit;
        private Random random = new Random((int) DateTimeOffset.Now.ToUnixTimeMilliseconds());
        private int runNum;
        private int spend_sells = 0, spend_buys = 0, profitsells = 0, profitbuys = 0, tot_spends = 0, tot_profits = 0;
        private double spends;
        private int startTime = 0;
        private Storage storage = new Storage();
        private string symbol = string.Empty;
        private float test_mse;
        private int ticket = 0, opnum = 0;
        private double total;
        private int totalNeurons;
        private float train_mse;
        private TrainingData trainData;
        private TrainingData testData;
        private double trainHitRatio;
        private double testHitRatio;
        private int ordersTotal;

        public double this [string name]
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

        //+------------------------------------------------------------------+■
        //| Start function                                                   |
        //+------------------------------------------------------------------+
        public override int start ()
        {
            if(Bars == this.previousBars)
                return 0;

            DrawStats();

            if(this.previousDay != Day())
            {
                this.previousDay = Day();
                log($"> Day #{this.previousDay} opnum {this.opnum}");
                this.opnum = 0;
            }

            AddText("SDF SD F");
            this.previousBars = Bars;
            ////---- calculate open orders by current symbol
            //string symbol = Symbol();

            //if (CalculateCurrentOrders() == 0)
            //    CheckForOpen(symbol);
            //else
            //    CheckForClose(symbol);

            return 0;
        }

        public override int deinit ()
        {
            log("Deinitializing ...");
            debug($"Balance={AccountBalance()} Orders={OrdersTotal()}");

            this.storage["functions"] = JsonConvert.SerializeObject(Data.nnFunctions, Formatting.Indented);
            this.storage.SyncData();

            string mins = (((GetTickCount() - this.startTime) / 1000.0 / 60.0)).ToString("0");
            debug($"Uptime {mins} mins");
            log("... shutting down");
            return 0;
        }

        public override int init ()
        {
            this.startTime = GetTickCount();

            ClearLogs();
            Banner();

            this.symbol = Symbol();
            this["runNum"] += 1;

            DumpInfo();
            ListGlobalVariables();
            InitStorages();
            ScanNetworks();
            TestNetworkMSE();
            TestNetworkHitRatio();

            log($"... initialized in {((GetTickCount() - this.startTime) / 1000.0).ToString("0.0")} sec.");

            return 0;
        }

        private void Banner ()
        {
            log($"*** Automatic TradingExpert for MT4 with neural networks and auto-created strategy based on code mutation.");
            log($"*** (c) 2018 Sergey Efimov. (kilitary@gmail.com, telegram/phone: +79500426692, skype: serjnah, icq: 401112)");

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2018, 1, 6)
                                    .AddDays(version.Build)
                .AddSeconds(version.Revision * 2);

            log($"Initializing version: {version} ... ");
        }

        public void TestNetworkHitRatio ()
        {
            info($"Calculating hit ratio on train & test data ...");
            this.trainHitRatio = CalculateHitRatio(this.trainData.Input, this.trainData.Output);
            this.testHitRatio = CalculateHitRatio(this.testData.Input, this.testData.Output);
            info($"TrainHitRatio: {this.trainHitRatio.ToString("0.00")}% TestHitRatio: {this.testHitRatio.ToString("0.00")}%");
        }

        public double CalculateHitRatio (double[][] inputs, double[][] desiredOutputs)
        {
            int hits = 0, curX = 0;
            foreach(double[] input in inputs)
            {
                var output = this.aiNetwork.Run(input);

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

        private void ListGlobalVariables ()
        {
            int var_total = GlobalVariablesTotal();
            string name;
            for(int i = 0; i < var_total; i++)
            {
                name = GlobalVariableName(i);
                debug($"global var {i} [{name}={GlobalVariableGet(name)}]");
            }
        }

        private void InitStorages ()
        {
            if(Configuration.useMysql)
                Data.db = new DB();

            this.storage["random"] = this.random.Next(int.MaxValue);
        }

        public void LoadNetwork (string dirName)
        {
            long fileLength = new FileInfo($"{Configuration.DataDirectory}\\{dirName}\\FANN.net").Length;
            log($"Loading FANN network {dirName} ({(fileLength / 1024.0).ToString("0.00")} KB) ...");

            this.aiName = dirName;
            this.dirName = dirName;

            this.aiNetwork = new NeuralNet($"{Configuration.DataDirectory}\\{dirName}\\FANN.net");
            this.aiNetwork.ResetMSE();

            info($"Network: hash={this.aiNetwork.GetHashCode()} inputs={this.aiNetwork.InputCount} " +
                $" outputs={this.aiNetwork.OutputCount} neurons={this.aiNetwork.TotalNeurons} ");

            this.totalNeurons = (int) this.aiNetwork.TotalNeurons;
            string fileTextData = File.ReadAllText($"d:\\temp\\forexAI\\{dirName}\\configuration.txt");
            Regex regex = new Regex(@"\[([^ \r\n\[\]]{1,10}?)\s+?", RegexOptions.Multiline
                | RegexOptions.Singleline);
            foreach(Match match in regex.Matches(fileTextData))
            {
                if(match.Groups[0].Value.Length <= 0)
                    continue;

                string funcName = match.Groups[0].Value.Trim('[', ' ');
                info($"* Function <{funcName}>");

                Dictionary <string, string> data = new Dictionary <string, string>();

                data["name"] = funcName;

                if(Data.nnFunctions.ContainsKey(funcName))
                    continue;

                Data.nnFunctions.Add(funcName, data);
            }
            Match match2 = Regex.Match(fileTextData, "InputDimension:\\s+(\\d+)?");
            int.TryParse(match2.Groups[1].Value, out this.inputDimension);

            info($"InputDimension = {this.inputDimension}");

            Match matchls = Regex.Match(fileTextData,
                                        "InputActFunc:\\s+([^ ]{1,40}?)\\s+LayerActFunc:\\s+([^ \r\n]{1,40})",
                 RegexOptions.Singleline);

            info($"Activation functions: input [{matchls.Groups[1].Value}] layer [{matchls.Groups[2].Value}]");

            this.inputLayerActivationFunction = matchls.Groups[1].Value;
            this.middleLayerActivationFunction = matchls.Groups[2].Value;
        }

        public void ScanNetworks ()
        {
            DirectoryInfo d = new DirectoryInfo(Configuration.DataDirectory);
            DirectoryInfo[] Dirs = d.GetDirectories("*");

            log($"Looking for networks in {Configuration.DataDirectory}: found {Dirs.Length} networks.");

            //foreach (DirectoryInfo dir in Dirs)
            //    storage[num++.ToString()] = $"{dir.Name}";
            this.storage["networks"] = JsonConvert.SerializeObject(Dirs);

            LoadNetwork(Dirs[this.random.Next(Dirs.Length - 1)].Name);
        }

        public void TestNetworkMSE ()
        {
            debug($"Doing neural network MSE test of {this.dirName} ...");

            this.trainData = new TrainingData(Configuration.DataDirectory +
                $"\\{this.dirName}\\traindata.dat");
            this.testData = new TrainingData(Configuration.DataDirectory +
                $"\\{this.dirName}\\testdata.dat");

            debug($"Train Data: trainDataLength={this.trainData.TrainDataLength} testDataLength={this.testData.TrainDataLength}");

            this.train_mse = this.aiNetwork.TestDataParallel(this.trainData, 4);
            this.test_mse = this.aiNetwork.TestDataParallel(this.testData, 3);

            debug($"MSE: train={this.train_mse.ToString("0.0000")} test={this.test_mse.ToString("0.0000")} bitfail={this.aiNetwork.BitFail}");
        }

        private void AddText (string text)
        {
            string on;
            double pos = Bid + (Bid - Ask) / 2;
            pos = Open[0];
            on = (pos.ToString());
            ObjectCreate(on, OBJ_TEXT, 0, iTime(Symbol(), 0, 0), pos);
            ObjectSetText(on, text, 8, "consolas", Color.Orange);
        }

        private void DumpInfo ()
        {
            log($"Company: [{TerminalCompany()}] Name: [{TerminalName()}] Path: [{TerminalPath()}]");
            log($"AccNumber: {AccountNumber()} AccName: [{AccountName()}] Balance: {AccountBalance()} Currency: {AccountCurrency()} ");
            debug($"equity={AccountEquity()} marginMode={AccountFreeMarginMode()} expert={WindowExpertName()}");
            debug($"leverage={AccountLeverage()} server=[{AccountServer()}] stopoutLev={AccountStopoutLevel()} stopoutMod={AccountStopoutMode()}");

            this.runNum = (int) this["runNum"];
            debug($"IsOptimization={IsOptimization()} IsTesting={IsTesting()} runNum={this.runNum}");
            debug($"orders={OrdersTotal()} timeCurrent={TimeCurrent()} digits={MarketInfo(this.symbol, MODE_DIGITS)} spred={MarketInfo(this.symbol, MODE_SPREAD)}");
            debug($"tickValue={MarketInfo(this.symbol, MODE_TICKVALUE)} tickSize={MarketInfo(this.symbol, MODE_TICKSIZE)} minlot={MarketInfo(this.symbol, MODE_MINLOT)}" +
                $" lotStep={MarketInfo(this.symbol, MODE_LOTSTEP)}");
        }

        private double GetActiveIncome ()
        {
            this.total = 0.0;

            double spends = GetActiveSpend();
            double profit = GetActiveProfit();
            if(profit > spends)
                this.total = profit + (spends);
            else
                this.total = 0.0;

            return (this.total);
        }

        private double GetActiveProfit ()
        {
            this.ordersTotal = OrdersTotal();
            this.total = 0.0;

            for(int pos = 0; pos < this.ordersTotal; pos++)
            {
                if(OrderSelect(pos, SELECT_BY_POS, MODE_TRADES) == false)
                    continue;
                if(OrderProfit() > 0.0)
                    this.total = this.total + OrderProfit();
            }

            return (this.total);
        }

        private double GetActiveSpend ()
        {
            this.ordersTotal = OrdersTotal();
            this.total = 0.0;

            for(int pos = 0; pos < this.ordersTotal; pos++)
            {
                if(OrderSelect(pos, SELECT_BY_POS, MODE_TRADES) == false)
                    continue;
                if(OrderProfit() < 0.0)
                    this.total = this.total + OrderProfit();
            }

            return (this.total);
        }

        public void DrawStats ()
        {
            int i;

            for(i = 0; i < 9; i++)
            {
                this.l_name_8 = "order" + i;
                if(ObjectFind(this.l_name_8) == -1)
                {
                    ObjectCreate(this.l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                    ObjectSet(this.l_name_8, OBJPROP_CORNER, 1);
                    ObjectSet(this.l_name_8, OBJPROP_XDISTANCE, 300);
                    ObjectSet(this.l_name_8, OBJPROP_YDISTANCE, i * 10);
                }

                ObjectSetText(this.l_name_8,
                              "                                    ",
                              8,
                              "consolas",
                              Color.White);
            }

            for(i = 0; i < OrdersTotal(); i++)
            {
                OrderSelect(i, SELECT_BY_POS, MODE_TRADES);
                if(OrderSymbol() == Symbol())
                {
                    if(OrderType() == OP_BUY)
                    {
                        this.type = "BUY";
                    }
                    if(OrderType() == OP_SELL)
                    {
                        this.ticket = OrderTicket();
                        this.type = "SELL";
                    }
                    if(OrderType() == OP_BUYLIMIT)
                    {
                        this.type = "BUY_LIMIT";
                    }
                    if(OrderType() == OP_SELLLIMIT)
                    {
                        this.type = "SELL_LIMIT";
                    }
                    if(OrderType() == OP_BUYSTOP)
                    {
                        this.type = "BUY_STOP";
                    }
                    if(OrderType() == OP_SELLSTOP)
                    {
                        this.type = "SELL_STOP";
                    }

                    this.l_name_8 = "order" + i;

                    ObjectSetText(this.l_name_8, "îðäåð " + this.type + " #" + i +
                        " ïðîôèò " + DoubleToStr(OrderProfit(), 2), 8, "consolas", Color.White);
                }
            }

            string gs_80 = "text";
            // double ld_0 = GetProfitForDay(0);
            this.l_name_8 = gs_80 + "1";
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
            this.l_name_8 = gs_80 + "4";
            if(ObjectFind(this.l_name_8) == -1)
            {
                ObjectCreate(this.l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(this.l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(this.l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(this.l_name_8, OBJPROP_YDISTANCE, 0);
            }
            ObjectSetText(this.l_name_8,
                          "AccountEquity: " + DoubleToStr(AccountEquity(), 2),
                          8,
                          "consolas",
                          Color.Yellow);

            this.l_name_8 = gs_80 + "5";
            if(ObjectFind(this.l_name_8) == -1)
            {
                ObjectCreate(this.l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(this.l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(this.l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(this.l_name_8, OBJPROP_YDISTANCE, 10);
            }

            this.total = GetActiveProfit();
            ObjectSetText(this.l_name_8,
                          "ActiveProfit: " + DoubleToStr(this.total, 2),
                          8,
                          "consolas",
                          Color.Yellow);

            this.l_name_8 = gs_80 + "6";
            if(ObjectFind(this.l_name_8) == -1)
            {
                ObjectCreate(this.l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(this.l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(this.l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(this.l_name_8, OBJPROP_YDISTANCE, 20);
            }

            this.total = GetActiveSpend();
            ObjectSetText(this.l_name_8, "ActiveSpend: " + DoubleToStr(this.total, 2), 8, "consolas", Color.Yellow);

            this.total = GetActiveIncome();
            this.l_name_8 = gs_80 + "7";
            if(ObjectFind(this.l_name_8) == -1)
            {
                ObjectCreate(this.l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(this.l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(this.l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(this.l_name_8, OBJPROP_YDISTANCE, 30);
            }
            ObjectSetText(this.l_name_8,
                          "ActiveIncome: " + DoubleToStr(this.total, 2),
                          8,
                          "consolas",
                          Color.Yellow);

            this.spends = GetActiveSpend();
            this.profit = GetActiveProfit();
            this.spends = (0 - (this.spends));
            // Print("profit:",profit," spends:", spends);
            if(this.profit > 0.0 && this.spends >= 0.0)
                this.total = (0 - ((this.profit) * 100.0) / this.spends);
            else
                this.total = 0;

            this.l_name_8 = gs_80 + "8";
            if(ObjectFind(this.l_name_8) == -1)
            {
                ObjectCreate(this.l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(this.l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(this.l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(this.l_name_8, OBJPROP_YDISTANCE, 40);
            }
            ObjectSetText(this.l_name_8,
                          "kpd %: " + DoubleToStr(this.total, 0) + "%",
                          8,
                          "consolas",
                          Color.Yellow);

            this.l_name_8 = gs_80 + "9";
            if(ObjectFind(this.l_name_8) == -1)
            {
                ObjectCreate(this.l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(this.l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(this.l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(this.l_name_8, OBJPROP_YDISTANCE, 50);
            }

            ObjectSetText(this.l_name_8, "opnum: " + this.opnum, 8, "consolas", Color.Yellow);

            this.l_name_8 = gs_80 + "10";
            if(ObjectFind(this.l_name_8) == -1)
            {
                ObjectCreate(this.l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(this.l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(this.l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(this.l_name_8, OBJPROP_YDISTANCE, 60);
            }
            ObjectSetText(this.l_name_8,
                          "OrdersTotal: " + OrdersTotal(),
                          8,
                          "consolas",
                          Color.Yellow);

            string dirtext = string.Empty, dirtext2 = string.Empty;
            this.l_name_8 = gs_80 + "11";
            if(ObjectFind(this.l_name_8) == -1)
            {
                ObjectCreate(this.l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(this.l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(this.l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(this.l_name_8, OBJPROP_YDISTANCE, 70);
            }
            ObjectSetText(this.l_name_8, "dirtext: " + dirtext, 8, "consolas", Color.Yellow);

            this.l_name_8 = gs_80 + "12";
            if(ObjectFind(this.l_name_8) == -1)
            {
                ObjectCreate(this.l_name_8, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(this.l_name_8, OBJPROP_CORNER, 1);
                ObjectSet(this.l_name_8, OBJPROP_XDISTANCE, 10);
                ObjectSet(this.l_name_8, OBJPROP_YDISTANCE, 80);
            }
            ObjectSetText(this.l_name_8, "dirtext2: " + dirtext2, 8, "consolas", Color.Yellow);

            this.tot_spends = this.spend_sells + this.spend_buys;
            this.tot_profits = this.profitsells + this.profitbuys;
            string d = "0";

            if(this.tot_spends > 0 && this.tot_profits > 0)
                d = DoubleToStr(100 - ((100.0 / this.tot_profits) * this.tot_spends), 2);

            string funcsString = string.Empty;
            foreach(var func in Data.nnFunctions)
            {
                funcsString += $"|{func.Key}";
            }
            Comment(
               "profitsells: " +
                this.profitsells +
                "\r\n"
               +
                "profitbuys:   " +
                this.profitbuys +
                "\r\n" +
               "spend_sells:  " +
                this.spend_sells +
                "\r\n"
               +
                "spend_buys:    " +
                this.spend_buys +
                "\r\n"
              +
                "tot_profits: " +
                DoubleToStr(this.tot_profits, 0) +
                "\r\n" +
               "tot_spends:  " +
                DoubleToStr(this.tot_spends, 0) +
                "\r\n" +
               "КПД: " +
                d +
                "%" +
                "\r\n\r\n" +
               "[Network " +
                this.aiName +
                "]\r\n" +
               "Functions: " +
                funcsString +
                "\r\n" +
               "InputDimension: " +
                this.inputDimension +
                "\r\n" +
               "TotalNeurons: " +
                this.totalNeurons +
                "\r\n" +
               "InputCount: " +
                this.aiNetwork.InputCount +
                "\r\n" +
               "InputActFunc: " +
                this.inputLayerActivationFunction +
                "\r\n" +
               "LayerActFunc: " +
                this.middleLayerActivationFunction +
                "\r\n" +
               "ConnRate: " +
                this.aiNetwork.ConnectionRate +
                "\r\n" +
               "Connections: " +
                this.aiNetwork.TotalConnections +
                "\r\n" +
               "LayerCount: " +
                this.aiNetwork.LayerCount +
                "\r\n" +
               "Train/Test MSE: " +
                this.train_mse +
                "/" +
                this.test_mse +
                "\r\n" +
               "LearningRate: " +
                this.aiNetwork.LearningRate +
                "\r\n" +
               "Test Hit Ratio: " +
                this.testHitRatio.ToString("0.00") +
                "%\r\n" +
               "Train Hit Ratio: " +
                this.trainHitRatio.ToString("0.00") +
                "%\r\n");

            if(ObjectFind("statyys") == -1)
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
