namespace ChiaWorks.FutureBot.Requests
{
    public abstract class HookRequestBase
    {
        public string Coin { get; set; }
    }

    public class NewSellRequest : HookRequestBase
    {
    }

    public class NewBuyRequest : HookRequestBase
    {
    }

    public class NewCommonRequest : HookRequestBase
    {
        public FutureDirection Direction { get; set; }
        public string Message { get; set; }
        public string Price { get; set; }
    }

    public enum FutureDirection
    {
        None = 0,
        Sell = 1,
        Buy = 2
    }
}