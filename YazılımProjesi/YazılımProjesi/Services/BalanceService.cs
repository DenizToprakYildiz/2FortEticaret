using YazılımProjesi.Models;

namespace YazılımProjesi.Services
{
    public interface IBalanceService
    {
        decimal GetBalance();
        void UpdateBalance(decimal amount);
        bool AddBalance(decimal amount);
    }

    public class BalanceService : IBalanceService
    {
        private static decimal _balance = 100000.00m; // Başlangıç bakiyesi (100,000 USD)

        public decimal GetBalance()
        {
            return _balance;
        }

        public void UpdateBalance(decimal amount)
        {
            _balance = amount;
        }

        public bool AddBalance(decimal amount)
        {
            if (amount <= 0)
                return false;

            _balance += amount;
            return true;
        }
    }
} 