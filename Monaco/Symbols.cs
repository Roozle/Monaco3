using Binance.Net;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monaco
{
    internal class Symbols
    {
        private static List<Symbol> symbols = new List<Symbol>();

        internal List<Symbol> Init(BinanceClient _client, BinanceSocketClient _socketClient)
        {
            var BookPrices = _client.Get24HPricesList().Data.Where(x => x.Symbol.Contains("BTC") && x.QuoteVolume > 200 && !x.Symbol.Contains("BNB")).OrderByDescending(x => x.QuoteVolume).ToList();
            _client.GetExchangeInfo().Data.Symbols.Where(x => BookPrices.Any(y => x.Name == y.Symbol)).ForEach(x => symbols.Add(new Symbol { symbol = x, bookPrice = BookPrices.First(y => x.Name == y.Symbol) }));
            Parallel.ForEach(symbols, sym =>
            {
                Console.WriteLine(sym.symbol.Name);
                Task.Run(() => sym.Init(_client, _socketClient));
            });
            return symbols;
        }

    }
}
