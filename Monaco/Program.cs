using Binance.Net;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Monaco
{
    class Program
    {
        public static Account account = new Account();
        static void Main(string[] args)
        {
            #region Initialise
            ThreadPool.GetMaxThreads(out int workerThreads, out int completionPortThreads);
            ThreadPool.SetMinThreads(workerThreads, completionPortThreads);
            LogVerbosity logVerbosity = LogVerbosity.Info;
            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials("xRG2dHStpuX4jdHUsVul8YlChhAzHs3ksUe68PvOuespRNWgXQa4eSgqBHd3co2k", "69ZXyVEufb1BvEoD3MzqCCIaPKZAwEbm6OrL3wSESbYGUAaHD8jC5H2tAatqojMu"),
                LogVerbosity = logVerbosity,
                LogWriters = new List<TextWriter> { Console.Out },
                TradeRulesBehaviour = TradeRulesBehaviour.AutoComply,
                ReceiveWindow = TimeSpan.FromSeconds(5),
                TradeRulesUpdateInterval = TimeSpan.FromMinutes(60),
                RequestTimeout = TimeSpan.FromMinutes(1),
                AutoTimestamp = true
            });
            BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials("xRG2dHStpuX4jdHUsVul8YlChhAzHs3ksUe68PvOuespRNWgXQa4eSgqBHd3co2k", "69ZXyVEufb1BvEoD3MzqCCIaPKZAwEbm6OrL3wSESbYGUAaHD8jC5H2tAatqojMu"),
                LogVerbosity = logVerbosity,
                ReconnectInterval = TimeSpan.FromSeconds(0),
                LogWriters = new List<TextWriter> { Console.Out }
            });

            var client = new BinanceClient();
            var socketClient = new BinanceSocketClient();
            Symbols symbols = new Symbols();
            #endregion

            var exchangeInfo = client.GetExchangeInfo();
            var time = client.GetServerTime();
            var symbolsList = symbols.Init(client, socketClient);
            account.StartAccountDataCollection(client, socketClient, symbolsList);

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

    }
}
