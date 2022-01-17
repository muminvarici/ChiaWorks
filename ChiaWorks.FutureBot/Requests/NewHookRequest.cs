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
}