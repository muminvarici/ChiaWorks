namespace ChiaWorks.FutureBot.Services
{
    public interface IFutureService
    {
        string Buy(string coin);
        string Sell(string coin);
        void Test();
    }
}