using BankAPI.Model;
using BankAPI.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankAPI.Repositories
{
    public interface ITransactionsRepository
    {
        Task<TransactionEntity> GetAsync(string id);
        Task InsertAsync(TransactionEntity transaction);
        Task<List<TransactionEntity>> ListAsync();
        Task<double> BalanceAsync(int account);
        Task<TransactionEntity> FindByAccountAsync(int account);
        Task<List<TransactionEntity>> ExtractAsync(int? account);
        Task<List<MonthlyReportViewModel>> MonthlyReportAsync(int? account, int? year);
    }
}
