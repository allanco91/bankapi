using BankAPI.Model;
using BankAPI.Repositories.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankAPI.Repositories
{
    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly IMongoDatabase mMongoDatabase;
        public TransactionsRepository(IMongoDatabase mongoDatabase)
        {
            mMongoDatabase = mongoDatabase;
        }

        public async Task InsertAsync(TransactionEntity transaction)
        {
            var collection = GetCollection();
            await collection.InsertOneAsync(transaction);
        }

        private IMongoCollection<TransactionEntity> GetCollection()
        {
            return mMongoDatabase.GetCollection<TransactionEntity>("Items");
        }

        public async Task<List<TransactionEntity>> ListAsync()
        {
            var collection = GetCollection();
            return await collection.Find(a => true).ToListAsync();
        }

        public async Task<TransactionEntity> GetAsync(string id)
        {
            var collection = GetCollection();
            return await collection.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        private async Task<List<TransactionEntity>> ListWithNegativeValuesAsync()
        {
            var transactions = from t in await ListAsync() select t;

            return transactions.Select(t => new TransactionEntity
            {
                Id = t.Id,
                Account = t.Account,
                Date = t.Date,
                IsDebit = t.IsDebit,
                Value = t.IsDebit ? t.Value * -1 : t.Value
            }).ToList();
        }

        public async Task<TransactionEntity> FindByAccountAsync(int account)
        {
            var collection = GetCollection();
            return await collection.Find(a => a.Account == account).FirstOrDefaultAsync();
        }

        public async Task<double> BalanceAsync(int account)
        {
            var transactions = from t in await ListWithNegativeValuesAsync() select t;

            return transactions.Where(t => t.Account == account).Sum(t => t.Value);
        }

        public async Task<List<TransactionEntity>> ExtractAsync(int? account)
        {
            var transactions = from t in await ListWithNegativeValuesAsync() select t;

            return transactions.Where(t => t.Account == account).ToList();
        }

        public async Task<List<MonthlyReportViewModel>> MonthlyReportAsync(int? account, int? year)
        {
            var transactions = from t in await ListWithNegativeValuesAsync() select t;
            Random rnd = new Random();

            return transactions.Where(t => t.Account == account && t.Date.Year == year)
                .GroupBy(g => new { g.Account, Date = new DateTime(g.Date.Year, g.Date.Month, 1) })
                .OrderBy(t => t.Key.Date)
                .Select(t => new MonthlyReportViewModel
                {
                    Id = rnd.Next(),
                    Account = t.Key.Account,
                    Date = t.Key.Date,
                    Credit = t.Where(x => !x.IsDebit).Sum(x => x.Value),
                    Debit = t.Where(x => x.IsDebit).Sum(x => x.Value),
                    Balance = t.Sum(x => x.Value)
                }).ToList();
        }
    }
}
