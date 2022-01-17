namespace ChiaWorks.FutureBot.Services
{
    public interface IFutureService
    {
        void Buy(string coin, float price);
        void Sell(string coin , float price);
        void Test();
    }
}