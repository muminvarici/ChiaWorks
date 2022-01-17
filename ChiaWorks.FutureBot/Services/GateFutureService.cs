using Io.Gate.GateApi.Api;
using Io.Gate.GateApi.Model;

namespace ChiaWorks.FutureBot.Services
{
    public class GateFutureService : IFutureService
    {
        private readonly FuturesApi _futuresApi;

        public GateFutureService(FuturesApi futuresApi)
        {
            _futuresApi = futuresApi;
        }

        public void Test()
        {
            var settle = "usdt"; // string | Settle currency
            var contract = "BTC_USDT"; // string | Futures contract
            var status = "open"; // string | Only list the orders with this status
            var offset = 0; // int? | List offset, starting from 0 (optional)  (default to 0)
            var result = _futuresApi.ListFuturesOrders(settle, contract, status);

            var positions = _futuresApi.ListFuturesContracts(settle);
            var position = _futuresApi.GetPosition(settle, contract);
        }

        public string Buy(string coin)
        {
            const string settle = "usdt"; // string | Settle currency
            var contract = $"{coin.ToUpper()}_USDT"; // string | Futures contract
            return contract + " buy request";
            var order = new FuturesOrder(contract,
                size: 5,
                text: "t-api",
                price: "43108"
            );

            var result = _futuresApi.CreateFuturesOrder(settle, order);
        }

        public string Sell(string coin)
        {
            const string settle = "usdt"; // string | Settle currency
            var contract = $"{coin.ToUpper()}_USDT"; // string | Futures contract
            return contract + " sell request";

            var order = new FuturesOrder(contract,
                size: -5,
                text: "t-api",
                price: "43108"
            );

            var result = _futuresApi.CreateFuturesOrder(settle, order);
        }
    }
}