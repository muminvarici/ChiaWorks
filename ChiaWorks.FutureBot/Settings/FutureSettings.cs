namespace ChiaWorks.FutureBot.Settings
{
    public class FutureSettings
    {
        public FutureProvider GateConfig { get; set; }
    }

    public class FutureProvider
    {
        public string Url { get; set; }
        public string ApiKey { get; set; }
        public string Secret { get; set; }
    }
}