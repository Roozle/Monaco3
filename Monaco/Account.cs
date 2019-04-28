using Binance.Net;
using Binance.Net.Objects;
using System.Collections.Generic;

namespace Monaco
{
    public class Account
    {
        public BinanceStreamAccountInfo accountInfo = new BinanceStreamAccountInfo();
        public List<BinanceStreamOrderUpdate> orders = new List<BinanceStreamOrderUpdate>();

        internal void StartAccountDataCollection(BinanceClient _client, BinanceSocketClient _socketClient, List<Symbol> _symbols)
        {
            string listenKey;

            var result = _client.GetAccountInfo();
            List<BinanceStreamBalance> balances = new List<BinanceStreamBalance>();
            result.Data.Balances.ForEach(x => balances.Add(new BinanceStreamBalance { Asset = x.Asset, Free = x.Free, Locked = x.Locked }));
            accountInfo = new BinanceStreamAccountInfo { MakerCommission = result.Data.MakerCommission, TakerCommission = result.Data.TakerCommission, BuyerCommission = result.Data.BuyerCommission, CanDeposit = result.Data.CanDeposit, CanTrade = result.Data.CanTrade, CanWithdraw = result.Data.CanWithdraw, SellerCommission = result.Data.SellerCommission, Balances = balances };
            foreach(var symbol in _symbols)
            {
                foreach (var oH in _client.GetAllOrders(symbol.symbol.Name).Data)
                {
                    orders.Add(new BinanceStreamOrderUpdate
                    {
                        ClientOrderId = oH.ClientOrderId,
                        CummulativeQuoteQuantity = oH.CummulativeQuoteQuantity,
                        IcebergQuantity = oH.IcebergQuantity,
                        IsWorking = oH.IsWorking,
                        OrderId = oH.OrderId,
                        Quantity = oH.OriginalQuantity,
                        Price = oH.Price,
                        Side = oH.Side,
                        Status = oH.Status,
                        StopPrice = oH.StopPrice,
                        Time = oH.Time,
                        Symbol = oH.Symbol,
                        TimeInForce = oH.TimeInForce,
                        Type = oH.Type
                    });
                }
            }

            listenKey = _client.StartUserStream().Data;
            var successAccount = _socketClient.SubscribeToUserStream(listenKey, data =>
            {
                accountInfo = data;
            },
            data =>
            {
                if(data.Status == OrderStatus.Filled)
                {
                    orders.Add(data);
                }
            });
        }

    }
}
