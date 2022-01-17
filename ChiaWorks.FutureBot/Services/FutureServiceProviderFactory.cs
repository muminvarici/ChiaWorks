using ChiaWorks.FutureBot.Settings;
using Io.Gate.GateApi.Api;
using Io.Gate.GateApi.Client;
using Microsoft.Extensions.Options;

namespace ChiaWorks.FutureBot.Services
{
    public class FutureServiceFactory
    {
        private readonly FutureProvider _setting;

        public FutureServiceFactory(IOptions<FutureSettings> setting)
        {
            _setting = setting.Value.GateConfig;
        }

        public FuturesApi GetGateFuturesApi()
        {
            var config = new Configuration
            {
                BasePath = _setting.Url,
                ApiV4Key = _setting.ApiKey,
                ApiV4Secret = _setting.Secret
            };
            return new FuturesApi(config);
        }

        public DeliveryApi GetGateDeliveryApi()
        {
            var config = new Configuration
            {
                BasePath = _setting.Url,
            };
            return new DeliveryApi(config);
        }
    }
}