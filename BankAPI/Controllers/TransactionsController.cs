using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankAPI.Model;
using BankAPI.Repositories;
using BankAPI.Repositories.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionsRepository _transactionsRepository;

        public TransactionsController(ITransactionsRepository transactionsRepository)
        {
            _transactionsRepository = transactionsRepository;
        }

        [HttpGet]
        [Route("hello")]
        public ActionResult<List<TransactionEntity>> Get()
        {
            return Ok(new { message = "Hello world!" });
        }

        [HttpGet]
        [Route("hello/{name}")]
        public ActionResult<List<TransactionEntity>> Get(string name)
        {
            return Ok("Hello world! Welcome " + name);
        }

        [HttpPost]
        [Route("credit")]
        public async Task<IActionResult> Credit(TransactionEntity obj)
        {
            if (obj.Account <= 0)
                return BadRequest(new ErrorViewModel { Error = "Account number must be greater than 0" });

            if (obj.IsDebit)
                return BadRequest(new ErrorViewModel { Error = "Operation must be Credit" });

            if (obj.Value <= 0)
                return BadRequest(new ErrorViewModel { Error = "Cannot credit account with value less than 0 or 0" });

            await _transactionsRepository.InsertAsync(obj);
            return Ok(new SuccessViewModel
            {
                Message = "Successfully inserted credits",
                Value = obj.Value,
                Balance = await _transactionsRepository.BalanceAsync(obj.Account)
            });
        }

        [HttpPost]
        [Route("debit")]
        public async Task<IActionResult> Debit(TransactionEntity obj)
        {
            if (obj.Account <= 0)
                return BadRequest(new ErrorViewModel { Error = "Account number must be greater than 0" });

            if (await _transactionsRepository.FindByAccountAsync(obj.Account) == null)
                return NotFound(new ErrorViewModel { Error = "Account not found" });

            if (!obj.IsDebit)
                return BadRequest(new ErrorViewModel { Error = "Operation must be Debit" });

            if (obj.Value <= 0)
                return BadRequest(new ErrorViewModel { Error = "Cannot debit account with value less than 0 or 0" });

            if (await _transactionsRepository.BalanceAsync(obj.Account) < obj.Value)
                return BadRequest(new ErrorViewModel { Error = "Balance must be greater than amount to be debited" });

            await _transactionsRepository.InsertAsync(obj);
            return Ok(new SuccessViewModel
            {
                Message = "Successfully debited credits",
                Value = obj.Value,
                Balance = await _transactionsRepository.BalanceAsync(obj.Account)
            });
        }

        [HttpGet]
        [Route("accountextract/{account}")]
        public async Task<IActionResult> AccountExtract(int? account)
        {
            if (!account.HasValue)
                return BadRequest(new ErrorViewModel { Error = "Account not provided" });

            if (account.Value <= 0)
                return BadRequest(new ErrorViewModel { Error = "Account number must be greater than 0" });

            if (await _transactionsRepository.FindByAccountAsync(account.Value) == null)
                return NotFound(new ErrorViewModel { Error = "Account not found" });

            var model = await _transactionsRepository.ExtractAsync(account);
            return Ok(model);
        }

        [HttpGet]
        [Route("monthlyreport/{year}/{account}")]
        public async Task<IActionResult> MonthlyReport(int? account, int? year)
        {
            if (!account.HasValue)
                return BadRequest(new ErrorViewModel { Error = "Account not provided" });

            if (account.Value <= 0)
                return BadRequest(new ErrorViewModel { Error = "Account number must be greater than 0" });

            if (!year.HasValue)
                return BadRequest(new ErrorViewModel { Error = "Year not provided" });

            if (year.Value < 1900 || year.Value > DateTime.Now.Year)
                return BadRequest(new ErrorViewModel { Error = "Year is not valid" });

            if (await _transactionsRepository.FindByAccountAsync(account.Value) == null)
                return NotFound(new ErrorViewModel { Error = "Account not found" });

            var model = await _transactionsRepository.MonthlyReportAsync(account, year);
            return Ok(model);
        }
    }
}