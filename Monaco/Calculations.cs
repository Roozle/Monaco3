using System;
using System.Collections.Generic;
using System.Text;
using Binance.Net.Objects;
using System.Linq;
//using MoreLinq;

namespace Monaco
{
    class Calculations
    {
        internal decimal CalculateWilliamsR(List<BinanceKline> _input, int _periods)
        {
            if (_input.Count() < _periods)
                return 0;

            decimal highest = _input.TakeLast(_periods).Max(x => x.High);
            decimal lowest = _input.TakeLast(_periods).Min(x => x.Low);
            decimal close = _input.TakeLast(_periods).Last().Close;

            return (highest - close)/(highest - lowest) * -100;
        }
        internal decimal calculatePercentageChange(decimal numberTo, decimal numberFrom)
        {
            if (numberTo == 0 || numberFrom == 0) { return 0; }
            return ((numberTo - numberFrom) / numberFrom) * 100;
        }

    }
}
