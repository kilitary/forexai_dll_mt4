
using NQuotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static forexAI.Logger;
using Color = System.Drawing.Color;

namespace forexAI
{
    class Trailing : MqlApi
    {
        double Lots = 0.1;
        int trailingStop = 30;
        int trailingStep = 10;
        int magic = 123;
        int slippage = 5;

        int maPeriod = 42;
        int maShift = 1;

        DateTime timePrev;

        public override int init()
        {
            if(Digits==3 || Digits == 5)
            {
                trailingStep *= 10;
                trailingStop *= 10;
                slippage *= 10;
            }
            return 0;
        }

        public override int start()
        {
            if (timePrev == Time[0])
                return 0;

            timePrev = Time[0];
            double maPrice = iMA(Symbol(), 0, maPeriod, maShift, MODE_SMA, PRICE_CLOSE, 1);

            if (Ask > maPrice && CountBuy() == 0 && CountSells() == 0)
            {
                //SendBuy
                if (OrderSend(Symbol(), OP_BUY, Lots, Ask, slippage, 0, 0, "", magic, DateTime.MinValue, Color.Blue) < 0)
                {
                    debug("Error open Buy Order");
                }
            }

            if (Bid < maPrice && CountBuy() == 0 && CountSells() == 0)
            {
                //SendSell
                if (OrderSend(Symbol(), OP_SELL, Lots, Bid, slippage, 0, 0, "", magic, DateTime.MinValue, Color.Red) < 0)
                {
                    debug("Error open Sell Order");
                }
            }

            RunTrailing();

            return 0;
        }

        int CountBuys()
        {
            int count = 0;
            for (int cur_order = OrdersTotal() - 1; cur_order >= 0; cur_order--)
            {
                if (!(OrderSelect(cur_order, SELECT_BY_POS, MODE_TRADES)))
                    break;
                if (OrderType() == OP_BUY && OrderSymbol() == Symbol())
                    count++;
            }
            return count;
        }

        int CountBuy()
        {
            int count = 0;
            for(int i = OrdersTotal() - 1; i >= 0; i--)
            {
                if(OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
                {
                    if(OrderSymbol() == Symbol() && OrderMagicNumber() == magic && OrderType() == OP_BUY)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        int CountSells()
        {
            int count = 0;
            for (int l_pos_216 = OrdersTotal() - 1; l_pos_216 >= 0; l_pos_216--)
            {
                if (!(OrderSelect(l_pos_216, SELECT_BY_POS, MODE_TRADES)))
                    break;
                if (OrderType() == OP_SELL && OrderSymbol() == Symbol())
                    count++;
            }
            return count;
        }

        int CountSell()
        {
            int count = 0;
            for (int i = OrdersTotal() - 1; i >= 0; i--)
            {
                if (OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
                {
                    if (OrderSymbol() == Symbol() && OrderMagicNumber() == magic && OrderType() == OP_SELL)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
        
        void RunTrailing()
        {
            for(int i=OrdersTotal() - 1; i>=0; i--)
            {
                if(OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
                {
                    if(OrderSymbol() == Symbol() && OrderMagicNumber() == magic)
                    {
                        if(OrderType() == OP_BUY)
                        {
                            if(Bid - OrderOpenPrice() > trailingStop * Point || OrderStopLoss() == 0)
                            {
                                if(OrderStopLoss() < Bid - (trailingStop + trailingStep) * Point || OrderStopLoss() == 0)
                                {
                                    if(!OrderModify(OrderTicket(), OrderOpenPrice(), NormalizeDouble(Bid - trailingStop * Point, Digits), 0, DateTime.Now, Color.Blue))
                                    {
                                        debug("Error create Buy Order");
                                    }
                                }
                            }
                        }

                        if (OrderType() == OP_SELL)
                        {
                            if (OrderOpenPrice() - Ask > trailingStop * Point || OrderStopLoss() == 0)
                            {
                                if (OrderStopLoss() < Ask + (trailingStop + trailingStep) * Point || OrderStopLoss() == 0)
                                {
                                    if (!OrderModify(OrderTicket(), OrderOpenPrice(), NormalizeDouble(Ask + trailingStop * Point, Digits), 0, DateTime.Now, Color.Red))
                                    {
                                        debug("Error create Sell Order");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



    }
}
