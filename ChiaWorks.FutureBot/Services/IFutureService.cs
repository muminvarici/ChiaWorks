namespace ChiaWorks.FutureBot.Services
{
    public interface IFutureService
    {
        void Buy(string coin);
        void Sell(string coin);
        void Test();
    }
}