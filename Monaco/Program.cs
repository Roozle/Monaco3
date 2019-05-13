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
            Console.WriteLine(workerThreads + " " + completionPortThreads);
            LogVerbosity logVerbosity = LogVerbosity.Info;
            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials("5u4hnBfBDY3nYCmPTWLSzQe7LIlhZePrTEG8QgaJwShO2LOG2miCuNTgporIdSaM", "ixpwvtzv1Ro7gi4aajuBduTREKBLc08MaNDGUw4OjWwwHw7A2E47GvjNWlzcjbb6"),
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
                ApiCredentials = new ApiCredentials("5u4hnBfBDY3nYCmPTWLSzQe7LIlhZePrTEG8QgaJwShO2LOG2miCuNTgporIdSaM", "ixpwvtzv1Ro7gi4aajuBduTREKBLc08MaNDGUw4OjWwwHw7A2E47GvjNWlzcjbb6"),
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
           }
        }

    }
}
