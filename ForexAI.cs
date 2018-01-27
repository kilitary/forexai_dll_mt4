﻿//╮╰╮╮▕╲╰╮╭╯╱▏╭╭╭╭ 
//╰╰╮╰╭╱▔▔▔▔╲╮╯╭╯ 
//┏━┓┏┫╭▅╲╱▅╮┣┓╭║║║ 
//╰┳╯╰┫┗━╭╮━┛┣╯╯╚╬╝ 
//╭┻╮╱╰╮╰━━╯╭╯╲┊ ║ 
//╰┳┫▔╲╰┳━━┳╯╱▔┊ ║ 
//┈┃╰━━╲▕╲╱▏╱━━━┬╨╮ 
//┈╰━━╮┊▕╱╲▏┊╭━━┴╥╯
//                                                   |    |    |

//-------^-----------------^^-------------^------- ѼΞΞΞΞΞΞΞD -----------------------^---------
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

namespace forexAI
{
    public class ForexAI : MqlApi
    {
        Version version;
        NeuralNet FXNetwork;
        TrainingData trainData;
        TrainingData testData;
        Random random = new Random((int) DateTimeOffset.Now.ToUnixTimeMilliseconds());
        Storage storage = new Storage();
        Settings settings = new Settings();
        string NetName;
        string dirName;
        string inputLayerActivationFunction, middleLayerActivationFunction;
        string labelID, type = string.Empty;
        string symbol = string.Empty;
        int inputDimension = 0;
        int currentTicket = 0;
        int operationsCount = 0;
        int ordersTotal;
        int previousBars = 0;
        int barsPerDay = 0;
        int spend_sells = 0, spend_buys = 0, profitsells = 0, profitbuys = 0, tot_spends = 0, tot_profits = 0;
        int buys = 0, sells = 0;
        int startTime = 0;
        int previousBankDay = 0;
        double trainHitRatio;
        double testHitRatio;
        double total;
        double spends;
        double profit;
        float test_mse;
        float train_mse;
        int magickNumber = 0x25;

        //+------------------------------------------------------------------+■
        //| Start function                                                   |
        //+------------------------------------------------------------------+
        public override int start()
        {
            if (Bars == previousBars)
                return 0;

            if (previousBankDay != Day())
            {
                previousBankDay = Day();

                log($"> Day{previousBankDay.ToString(" 0")} opnum={operationsCount} barsPerDay={barsPerDay}");

                operationsCount = 0;
                barsPerDay = 0;

                Audio.FX.FXNewDay();
            }

            File.AppendAllText(@"d:\temp\forexAI\seed", random.Next(99).ToString("00") + " ");
            File.AppendAllText(@"d:\temp\forexAI\Yseed", YRandom.Next(99).ToString("00") + " ");

            CalculateCurrentOrders();

            if (OrdersTotal() < 3)
                CheckForOpen();

            CheckForClose();
            AlliedInstructions();

            DrawStats();

            previousBars = Bars;
            barsPerDay += 1;

            return 0;
        }



        public override int deinit()
        {
            log("Deinitializing ...");
            log($"Balance={AccountBalance()} Orders={OrdersTotal()}");

            settings.Set("functions", Data.nnFunctions);
            settings.Set("balance", AccountBalance());
            settings.Save();
            storage.SyncData();

            string mins = (((GetTickCount() - startTime) / 1000.0 / 60.0)).ToString("0");
            log($"Uptime {mins} mins, has do {operationsCount} operations");
            log("... shutting down");
            return 0;
        }

        public override int init()
        {
            startTime = GetTickCount();
            symbol = Symbol();
            version = Assembly.GetExecutingAssembly().GetName().Version;

            ResetLog();
            ShowBanner();
            InitStorages();
            DumpInfo();
            ListGlobalVariables();
            ScanNetworks();
            TestNetworkMSE();
            TestNetworkHitRatio();

            log($"Initialized in {((GetTickCount() - startTime) / 1000.0).ToString("0.0")} sec(s)");
            return 0;
        }

        void ShowBanner()
        {
            log($"# Automated Expert for MT4 using neural network with strategy created by code/data fuzzing.");
            log($"# (c) 2018 Deconf (kilitary@gmail.com telegram:@deconf skype:serjnah icq:401112)");
            log($"Initializing version {version} ...");
        }

        void TestNetworkHitRatio()
        {
            FXNetwork.SetOutputScalingParams(trainData, -1.0f, 1.0f);
            FXNetwork.SetInputScalingParams(trainData, -1.0f, 1.0f);
            FXNetwork.SetOutputScalingParams(testData, -1.0f, 1.0f);
            FXNetwork.SetInputScalingParams(testData, -1.0f, 1.0f);

            trainHitRatio = CalculateHitRatio(trainData.Input, trainData.Output);
            testHitRatio = CalculateHitRatio(testData.Input, testData.Output);

            log($" * TrainHitRatio: {trainHitRatio.ToString("0.00")}% TestHitRatio: {testHitRatio.ToString("0.00")}%");
        }

        double CalculateHitRatio(double[][] inputs, double[][] desiredOutputs)
        {
            int hits = 0, curX = 0;
            foreach (double[] input in inputs)
            {
                double[] output = FXNetwork.Run(input);
                FXNetwork.DescaleOutput(output);

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

        void ListGlobalVariables()
        {
            int var_total = GlobalVariablesTotal();
            string name;
            for (int i = 0; i < var_total; i++)
            {
                name = GlobalVariableName(i);
                debug($"global var {i} [{name}={GlobalVariableGet(name)}]");
            }
        }

        void InitStorages()
        {
            if (Configuration.useMysql)
                Data.db = new DB();

            settings["yrandom"] = YRandom.Next(int.MaxValue);
            settings["random"] = random.Next(int.MaxValue);
        }

        void LoadNetwork(string dirName)
        {
            long fileLength = new FileInfo($"{Configuration.dataDirectory}\\{dirName}\\FANN.net").Length;
            log($"Loading network {dirName} ({(fileLength / 1024.0).ToString("0.00")} KB)");

            NetName = dirName;
            this.dirName = dirName;

            FXNetwork = new NeuralNet($"{Configuration.dataDirectory}\\{dirName}\\FANN.net");

            log($"Network: hash={FXNetwork.GetHashCode()} inputs={FXNetwork.InputCount} layers={FXNetwork.LayerCount}" +
                $" outputs={FXNetwork.OutputCount} neurons={FXNetwork.TotalNeurons} connections={FXNetwork.TotalConnections}");

            string fileTextData = File.ReadAllText($"d:\\temp\\forexAI\\{dirName}\\configuration.txt");
            Regex regex = new Regex(@"\[([^ \r\n\[\]]{1,10}?)\s+?", RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in regex.Matches(fileTextData))
            {
                if (match.Groups[0].Value.Length <= 0)
                    continue;

                string funcName = match.Groups[0].Value.Trim('[', ' ');
                log($" * Function <{funcName}>");

                Dictionary<string, string> data = new Dictionary<string, string>();

                data["name"] = funcName;

                if (Data.nnFunctions.ContainsKey(funcName))
                    continue;

                Data.nnFunctions.Add(funcName, data);
            }

            Match match2 = Regex.Match(fileTextData, "InputDimension:\\s+(\\d+)?");
            int.TryParse(match2.Groups[1].Value, out inputDimension);

            log($" * InputDimension = {inputDimension}");

            Match matches = Regex.Match(fileTextData,
                                        "InputActFunc:\\s+([^ ]{1,40}?)\\s+LayerActFunc:\\s+([^ \r\n]{1,40})",
                 RegexOptions.Singleline);

            log($" * Activation functions: input [{matches.Groups[1].Value}] layer [{matches.Groups[2].Value}]");

            inputLayerActivationFunction = matches.Groups[1].Value;
            middleLayerActivationFunction = matches.Groups[2].Value;
        }

        void ScanNetworks()
        {
            DirectoryInfo d = new DirectoryInfo(Configuration.dataDirectory);
            DirectoryInfo[] Dirs = d.GetDirectories("*");

            log($"Looking for networks in {Configuration.dataDirectory}: found {Dirs.Length} networks.");

            settings["networks"] = JsonConvert.SerializeObject(Dirs);

            LoadNetwork(Dirs[random.Next(Dirs.Length - 1)].Name);
        }

        void TestNetworkMSE()
        {
            trainData = new TrainingData(Configuration.dataDirectory + $"\\{dirName}\\traindata.dat");
            testData = new TrainingData(Configuration.dataDirectory + $"\\{dirName}\\testdata.dat");

            log($" * trainDataLength={trainData.TrainDataLength} testDataLength={testData.TrainDataLength}");

            train_mse = FXNetwork.TestDataParallel(trainData, 4);
            test_mse = FXNetwork.TestDataParallel(testData, 3);

            log($" * MSE: train={train_mse.ToString("0.0000")} test={test_mse.ToString("0.0000")} bitfail={FXNetwork.BitFail}");
        }

        void DumpInfo()
        {
            log($"AccNumber: {AccountNumber()} AccName: [{AccountName()}] Balance: {AccountBalance()} Currency: {AccountCurrency()} ");
            log($"Company: [{TerminalCompany()}] Name: [{TerminalName()}] Path: [{TerminalPath()}]");

            debug($"equity={AccountEquity()} marginMode={AccountFreeMarginMode()} expert={WindowExpertName()}");
            debug($"leverage={AccountLeverage()} server=[{AccountServer()}] stopoutLev={AccountStopoutLevel()} stopoutMod={AccountStopoutMode()}");
            debug($"IsOptimization={IsOptimization()} IsTesting={IsTesting()}");
            debug($"orders={OrdersTotal()} timeCurrent={TimeCurrent()} digits={MarketInfo(symbol, MODE_DIGITS)} spred={MarketInfo(symbol, MODE_SPREAD)}");
            debug($"tickValue={MarketInfo(symbol, MODE_TICKVALUE)} tickSize={MarketInfo(symbol, MODE_TICKSIZE)} minlot={MarketInfo(symbol, MODE_MINLOT)}" +
                $" lotStep={MarketInfo(symbol, MODE_LOTSTEP)}");

            Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            console($"mem={(currentProcess.WorkingSet64 / 1024.0 / 1024.0).ToString("0.00")}MB");
        }

        double GetActiveIncome()
        {
            total = 0.0;

            double spends = GetActiveSpend();
            double profit = GetActiveProfit();
            if (profit > spends)
                total = profit + (spends);
            else
                total = 0.0;

            return (total);
        }

        double GetActiveProfit()
        {
            ordersTotal = OrdersTotal();
            total = 0.0;

            for (int pos = 0; pos < ordersTotal; pos++)
            {
                if (OrderSelect(pos, SELECT_BY_POS, MODE_TRADES) == false)
                    continue;
                if (OrderProfit() > 0.0)
                    total = total + OrderProfit();
            }

            return (total);
        }

        double GetActiveSpend()
        {
            ordersTotal = OrdersTotal();
            total = 0.0;

            for (int pos = 0; pos < ordersTotal; pos++)
            {
                if (OrderSelect(pos, SELECT_BY_POS, MODE_TRADES) == false)
                    continue;
                if (OrderProfit() < 0.0)
                    total = total + OrderProfit();
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
            ObjectSetText(on, text, 8, "lucida console", Color.DarkRed);
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
                        OrderProfit().ToString(), 10, "lucida console", Color.LightGreen);
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

            total = GetActiveSpend();
            ObjectSetText(labelID, "ActiveSpend: " + DoubleToStr(total, 2), 8, "lucida console", Color.Yellow);

            total = GetActiveIncome();
            labelID = gs_80 + "7";
            if (ObjectFind(labelID) == -1)
            {
                ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(labelID, OBJPROP_CORNER, 1);
                ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
                ObjectSet(labelID, OBJPROP_YDISTANCE, 30);
            }
            ObjectSetText(labelID,
                          "ActiveIncome: " + DoubleToStr(total, 2),
                          8,
                          "lucida console",
                          Color.Yellow);

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
                          "kpd %: " + DoubleToStr(total, 0) + "%",
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

            ObjectSetText(labelID, "opnum: " + operationsCount, 8, "lucida console", Color.Yellow);

            labelID = gs_80 + "10";
            if (ObjectFind(labelID) == -1)
            {
                ObjectCreate(labelID, OBJ_LABEL, 0, DateTime.Now, 0);
                ObjectSet(labelID, OBJPROP_CORNER, 1);
                ObjectSet(labelID, OBJPROP_XDISTANCE, 10);
                ObjectSet(labelID, OBJPROP_YDISTANCE, 60);
            }
            ObjectSetText(labelID,
                          "OrdersTotal: " + OrdersTotal(),
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

            tot_spends = spend_sells + spend_buys;
            tot_profits = profitsells + profitbuys;
            string d = "0";

            if (tot_spends > 0 && tot_profits > 0)
                d = DoubleToStr(100 - ((100.0 / tot_profits) * tot_spends), 2);

            string funcsString = string.Empty;
            foreach (var func in Data.nnFunctions)
                funcsString += $"|{func.Key}";

            Comment(
               "profitsells: " +
                profitsells +
                "\r\n"
               +
                "profitbuys:   " +
                profitbuys +
                "\r\n" +
               "spend_sells:  " +
                spend_sells +
                "\r\n"
               +
                "spend_buys:    " +
                spend_buys +
                "\r\n"
              +
                "tot_profits: " +
                DoubleToStr(tot_profits, 0) +
                "\r\n" +
               "tot_spends:  " +
                DoubleToStr(tot_spends, 0) +
                "\r\n" +
               "КПД: " +
                d +
                "%" +
                "\r\n\r\n" +
               "[Network " +
                NetName +
                "]\r\n" +
               "Functions: " +
                funcsString +
                "\r\n" +
               "InputDimension: " +
                inputDimension +
                "\r\n" +
               "TotalNeurons: " +
                FXNetwork.TotalNeurons +
                "\r\n" +
               "InputCount: " +
                FXNetwork.InputCount +
                "\r\n" +
               "InputActFunc: " +
                inputLayerActivationFunction +
                "\r\n" +
               "LayerActFunc: " +
                middleLayerActivationFunction +
                "\r\n" +
               "ConnRate: " +
                FXNetwork.ConnectionRate +
                "\r\n" +
               "Connections: " +
                FXNetwork.TotalConnections +
                "\r\n" +
               "LayerCount: " +
                FXNetwork.LayerCount +
                "\r\n" +
               "Train/Test MSE: " +
                train_mse +
                "/" +
                test_mse +
                "\r\n" +
               "LearningRate: " +
                FXNetwork.LearningRate +
                "\r\n" +
               "Test Hit Ratio: " +
                testHitRatio.ToString("0.00") +
                "%\r\n" +
               "Train Hit Ratio: " +
                trainHitRatio.ToString("0.00") +
                "%\r\n");

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
        private int CalculateCurrentOrders()
        {
            for (int i = 0; i < OrdersTotal(); i++)
            {
                if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
                    break;
                if (OrderSymbol() == Symbol() && OrderMagicNumber() == magickNumber)
                {
                    if (OrderType() == OP_BUY)
                        buys++;
                    if (OrderType() == OP_SELL)
                        sells++;
                }
            }

            //---- return orders volume
            if (buys > 0)
                return (buys);
            return (-sells);
        }

        ////+------------------------------------------------------------------+
        ////| Check for close order conditions                                 |
        ////+------------------------------------------------------------------+
        private void CheckForClose()
        {
            //---- go trading only for first tiks of new bar
            if (Volume[0] > 1)
                return;
            //---- get Moving Average
            double ma = iMA(symbol, 0, 25, 1, MODE_SMA, PRICE_CLOSE, 0);

            for (int i = 0; i < OrdersTotal(); i++)
            {
                if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
                    break;
                if (OrderMagicNumber() != magickNumber || OrderSymbol() != symbol)
                    continue;
                //---- check order type
                if (OrderType() == OP_BUY)
                {
                    // log("test  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" + OrderProfit());
                    if (Open[1] > ma && Close[1] < ma)
                    {
                        if (OrderProfit() <= 0.0)
                            console($"сука бля проёбано {OrderProfit()}$");
                        else
                        {
                            console($"ееее профит {OrderProfit()}$");
                            Audio.FX.FXProfit();
                        }
                        OrderClose(OrderTicket(), OrderLots(), Bid, 3, Color.White);
                        log("# close buy " + OrderTicket() + " bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" + OrderProfit());
                        operationsCount++;
                    }

                    break;
                }
                if (OrderType() == OP_SELL)
                {
                    // log("test  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" + OrderProfit());
                    if (Open[1] < ma && Close[1] > ma)
                    {
                        if (OrderProfit() <= 0.0)
                            console($"сука бля проёбано {OrderProfit()}$");
                        else
                        {
                            console($"ееее профит {OrderProfit()}$");
                            Audio.FX.FXProfit();
                        }
                        OrderClose(OrderTicket(), OrderLots(), Ask, 3, Color.White);
                        log("# close sell " + OrderTicket() + "  bar " + Bars + " on " + symbol + " balance:" + AccountBalance() + " profit=" + OrderProfit());
                        operationsCount++;
                    }

                    break;
                }
            }
        }

        ////+------------------------------------------------------------------+
        ////| Check for open order conditions                                  |
        ////+------------------------------------------------------------------+
        private void CheckForOpen()
        {
            //---- go trading only for first tiks of new bar
            if (Volume[0] > 1)
            {
                log("vol bad");
                return;
            }

            //---- get Moving Average
            double ma = iMA(symbol, 0, 25, 1, MODE_SMA, PRICE_CLOSE, 0);


            //---- sell conditions
            if (Open[1] > ma && Close[1] < ma)
            {
                OrderSend(symbol, OP_SELL, 0.01, Bid, 3, 0, 0, "", magickNumber, DateTime.MinValue, Color.Red);
                log("# open sell  @" + Bid);
                operationsCount++;
            }
            //---- buy conditions
            if (Open[1] < ma && Close[1] > ma)
            {
                OrderSend(symbol, OP_BUY, 0.01, Ask, 3, 0, 0, "", magickNumber, DateTime.MinValue, Color.Blue);
                log("# open buy @" + Ask);
                operationsCount++;
            }
        }
    }
}
