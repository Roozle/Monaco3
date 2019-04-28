using Binance.Net;
using Binance.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Monaco
{
    internal class Symbol
    {
        public BinanceSymbol symbol = new BinanceSymbol();
        public Binance24HPrice bookPrice = new Binance24HPrice();
        private List<BinanceKline> fifteenMinutes = new List<BinanceKline>();
        private List<BinanceKline> oneMinute = new List<BinanceKline>();
        private BinanceStreamTrade tradeData;
        private Calculations calculations = new Calculations();
        public decimal WilliamsR;
        private decimal lastWilliamsR;
        private BinanceStreamOrderUpdate lastOrder = new BinanceStreamOrderUpdate();


        public void Init(BinanceClient _client, BinanceSocketClient _socketClient)
        {
            initFifteenMinuteKline(_client, _socketClient);
            initOneMinuteKline(_client, _socketClient);
            initTradeStream(_client, _socketClient);
        }

        private void initTradeStream(BinanceClient _client, BinanceSocketClient _socketClient)
        {
            var tradeStream = _socketClient.SubscribeToTradesStreamAsync(symbol.Name, data =>
            {
                tradeData = data;
                processTrades(_client);
            });
            if (tradeStream.Result.Success)
            {
                tradeStream.Result.Data.ConnectionLost += () =>
                {
                    initTradeStream(_client, _socketClient);
                };
            }
        }

        public void processTrades(BinanceClient _client)
        {
            if (lastOrder.Price != 0)
            {
                Console.WriteLine($"{symbol.Name} /// {Math.Round(calculations.calculatePercentageChange(tradeData.Price, lastOrder.Price), 2)} /// {Math.Round(tradeData.Price, 7)} /// {DateTime.UtcNow - lastOrder.Time}");
            }
            try
            {
                if (Program.account.accountInfo.Balances == null || !Program.account.accountInfo.Balances.Any())
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }

            var symbolBalance = Program.account.accountInfo.Balances.FirstOrDefault(x => x.Asset == symbol.BaseAsset);

            /// Calls to logic go here
            /// 
            if (tradeData.Price != 0)
            {
                // Check that price is in the bottom 20% of prices and that the high and low prices for the day are wider than 0.8%
                var dailyLowPrice = fifteenMinutes.TakeLast(96).Min(x => x.Low);
                var dailyHighPrice = fifteenMinutes.TakeLast(96).Max(x => x.High);
                var dailyPriceRange = dailyHighPrice - dailyLowPrice;

                var bottom20Percent = (dailyPriceRange * 0.2m) + dailyLowPrice;
                if (tradeData.Price > bottom20Percent || calculations.calculatePercentageChange(dailyHighPrice, dailyLowPrice) < 0.8m)
                    return;

                // Check that the latest 1M is higher than 1M-1, and that the close of the latest 1M is closer to high than low
                var oneMinuteList = oneMinute.TakeLast(2).ToList();
                var oneMinuteLowPrice = oneMinuteList[1].Low;
                var oneMinuteHighPrice = oneMinuteList[1].High;
                var oneMinutePriceRange = oneMinuteHighPrice - oneMinuteLowPrice;
                var bottom50Percent = (oneMinutePriceRange * 0.5m) + oneMinuteLowPrice;


                if (tradeData.Price < oneMinuteList[1].Low || oneMinuteList[1].Close < bottom50Percent)
                    return;

                //Walls
                var orderBook = _client.GetOrderBook(symbol.Name, 10).Data;
                var redWallQty = orderBook.Asks.Where(x => x.Price >= tradeData.Price).Sum(x => x.Price * x.Quantity);
                var greenWallQty = orderBook.Bids.Where(x => x.Price <= tradeData.Price).Sum(x => x.Price * x.Quantity);
                var tradeQuantity = fifteenMinutes.Last().QuoteAssetVolume;

                if (greenWallQty < (redWallQty * 2))
                    return;

                if (symbolBalance.Free == 0)
                {
                    var Quantity = (symbol.MinNotionalFilter.MinNotional * 1.5m / tradeData.Price);
                    var newBuyOrder = _client.PlaceTestOrder(symbol.Name, OrderSide.Buy, OrderType.Limit, Quantity, null, tradeData.Price, TimeInForce.GoodTillCancel);
                    Console.WriteLine($"Bought {symbol.Name} at {DateTime.UtcNow} for {Math.Round(tradeData.Price, 7)}");

                }
            }
            if (symbolBalance.Free > 0)
            {
                if (Program.account.orders.Any() && Program.account.orders.Any(x => x.Symbol == symbol.Name))
                {
                    lastOrder = Program.account.orders.Last(x => x.Symbol == symbol.Name && x.Side == OrderSide.Buy && x.Status == OrderStatus.Filled);
                    if (calculations.calculatePercentageChange(tradeData.Price, lastOrder.Price) > 0.4m)
                    {
                        var OneMinuteList = oneMinute.TakeLast(3).ToList();
                        if (OneMinuteList[0].Close < OneMinuteList[0].Open
                            && OneMinuteList[0].Open < OneMinuteList[1].Open
                            && OneMinuteList[1].Open < OneMinuteList[2].Open
                            && calculations.calculatePercentageChange(OneMinuteList[2].Open, OneMinuteList[0].Close) < -0.02m
                            || calculations.calculatePercentageChange(OneMinuteList[0].Open, OneMinuteList[0].Close) < -0.02m)
                        {
                            var result = _client.PlaceTestOrder(symbol.Name, OrderSide.Sell, OrderType.Limit, symbolBalance.Free, null, tradeData.Price, TimeInForce.GoodTillCancel);
                        }
                    }
                    if (lastOrder.Time <= DateTimeOffset.UtcNow.AddDays(-4))
                    {
                        var result = _client.PlaceTestOrder(symbol.Name, OrderSide.Sell, OrderType.Limit, symbolBalance.Free, null, tradeData.Price, TimeInForce.GoodTillCancel);
                    }
                }
            }
        }

        private void initFifteenMinuteKline(BinanceClient _client, BinanceSocketClient _socketClient)
        {
            var time = DateTime.UtcNow;
            var timespan = TimeSpan.FromMinutes(15);
            var endTime = time - new TimeSpan(TimeSpan.FromMinutes(15).Ticks * 500);
            foreach (BinanceKline kline in _client.GetKlines(symbol.Name, KlineInterval.FifteenMinutes, endTime, time).Data)
            {
                fifteenMinutes.Add(kline);
            }
            lastWilliamsR = WilliamsR;
            WilliamsR = calculations.CalculateWilliamsR(fifteenMinutes, 14);


            var fifteenMinutesStream = _socketClient.SubscribeToKlineStreamAsync(symbol.Name, KlineInterval.FifteenMinutes, data =>
            {
                if (data.Data.Final)
                {
                    fifteenMinutes.Add(data.Data.ToKline());
                }
            });
        }
        private void initOneMinuteKline(BinanceClient _client, BinanceSocketClient _socketClient)
        {
            var time = DateTime.UtcNow;
            var timespan = TimeSpan.FromMinutes(1);
            var endTime = time - new TimeSpan(TimeSpan.FromMinutes(1).Ticks * 500);
            foreach (BinanceKline kline in _client.GetKlines(symbol.Name, KlineInterval.OneMinute, endTime, time).Data)
            {
                oneMinute.Add(kline);
            }

            var oneMinutesStream = _socketClient.SubscribeToKlineStreamAsync(symbol.Name, KlineInterval.OneMinute, data =>
            {
                if (data.Data.Final && data.Data.CloseTime != oneMinute.Last().CloseTime)
                {
                    oneMinute.Add(data.Data.ToKline());
                }

            });
        }

    }
}