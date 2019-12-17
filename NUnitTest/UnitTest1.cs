using BankAPI.Controllers;
using BankAPI.Repositories;
using BankAPI.Repositories.Entities;
using Moq;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankAPI.Model;
using System.Collections.Generic;

namespace NUnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        #region Credit tests
        [Test]
        public async Task TestCredit()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            TransactionEntity obj = new TransactionEntity
            {
                Account = 1,
                Value = 50.0,
                IsDebit = false,
                Date = DateTime.Now
            };

            mok.Setup(a => a.BalanceAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(60.0 + obj.Value));

            var result = await controller.Credit(obj);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                .BeOfType<SuccessViewModel>().Which.Message.Should().Be("Successfully inserted credits");
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                .BeOfType<SuccessViewModel>().Which.Value.Should().Be(50.0);
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                .BeOfType<SuccessViewModel>().Which.Balance.Should().Be(110.0);
        }

        [Test]
        public async Task TestCreditNoAccount()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            TransactionEntity obj = new TransactionEntity
            {
                Account = default,
                Value = 50.0,
                IsDebit = false,
                Date = DateTime.Now
            };

            mok.Setup(a => a.BalanceAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(60.0 + obj.Value));

            var result = await controller.Credit(obj);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account number must be greater than 0");
        }

        [Test]
        public async Task TestCreditNegativeAccountNumber()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            TransactionEntity obj = new TransactionEntity
            {
                Account = -50,
                Value = 50.0,
                IsDebit = false,
                Date = DateTime.Now
            };

            mok.Setup(a => a.BalanceAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(60.0 + obj.Value));

            var result = await controller.Credit(obj);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account number must be greater than 0");
        }

        [Test]
        public async Task TestCreditNoValue()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            TransactionEntity obj = new TransactionEntity
            {
                Account = 1,
                Value = 0.0,
                IsDebit = false,
                Date = DateTime.Now
            };

            mok.Setup(a => a.BalanceAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(60.0 + obj.Value));

            var result = await controller.Credit(obj);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Cannot credit account with value less than 0 or 0");
        }

        [Test]
        public async Task TestCreditNegativeValue()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            TransactionEntity obj = new TransactionEntity
            {
                Account = 1,
                Value = -20.0,
                IsDebit = false,
                Date = DateTime.Now
            };

            mok.Setup(a => a.BalanceAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(60.0 + obj.Value));

            var result = await controller.Credit(obj);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Cannot credit account with value less than 0 or 0");
        }

        [Test]
        public async Task TestCreditIsCredit()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            TransactionEntity obj = new TransactionEntity
            {
                Account = 1,
                Value = 50.0,
                IsDebit = true,
                Date = DateTime.Now
            };

            var result = await controller.Credit(obj);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Operation must be Credit");
        }
        #endregion

        #region Debit tests
        [Test]
        public async Task TestDebit()
        {
            var mok = new Mock<ITransactionsRepository>();

            TransactionEntity obj = new TransactionEntity
            {
                Id = "test",
                Account = 1,
                Value = 60.0,
                IsDebit = true,
                Date = DateTime.Now
            };


            mok.Setup(a => a.FindByAccountAsync(It.IsAny<int>())).Returns(Task.FromResult(obj));

            var controller = new TransactionsController(mok.Object);

            TransactionEntity obj2 = new TransactionEntity
            {
                Account = 1,
                Value = 10.0,
                IsDebit = true,
                Date = DateTime.Now
            };

            mok.Setup(a => a.BalanceAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(60.0 - obj2.Value));

            var result = await controller.Debit(obj2);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                .BeOfType<SuccessViewModel>().Which.Message.Should().Be("Successfully debited credits");
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                .BeOfType<SuccessViewModel>().Which.Value.Should().Be(10.0);
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should()
                .BeOfType<SuccessViewModel>().Which.Balance.Should().Be(50.0);
        }

        [Test]
        public async Task TestDebitAccountNumberZero()
        {
            var mok = new Mock<ITransactionsRepository>();


            var controller = new TransactionsController(mok.Object);

            TransactionEntity obj2 = new TransactionEntity
            {
                Account = default,
                Value = 50.0,
                IsDebit = true,
                Date = DateTime.Now
            };

            var result = await controller.Debit(obj2);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account number must be greater than 0");
        }

        [Test]
        public async Task TestDebitAccountNumberNegative()
        {
            var mok = new Mock<ITransactionsRepository>();


            var controller = new TransactionsController(mok.Object);

            TransactionEntity obj2 = new TransactionEntity
            {
                Account = -50,
                Value = 50.0,
                IsDebit = true,
                Date = DateTime.Now
            };

            var result = await controller.Debit(obj2);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account number must be greater than 0");
        }

        [Test]
        public async Task TestDebitNoValue()
        {
            var mok = new Mock<ITransactionsRepository>();

            TransactionEntity obj = new TransactionEntity
            {
                Id = "test",
                Account = 1,
                Value = 60.0,
                IsDebit = true,
                Date = DateTime.Now
            };


            mok.Setup(a => a.FindByAccountAsync(It.IsAny<int>())).Returns(Task.FromResult(obj));

            var controller = new TransactionsController(mok.Object);

            TransactionEntity obj2 = new TransactionEntity
            {
                Account = 1,
                Value = 0.0,
                IsDebit = true,
                Date = DateTime.Now
            };

            var result = await controller.Debit(obj2);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Cannot debit account with value less than 0 or 0");
        }

        [Test]
        public async Task TestDebitNegativeValue()
        {
            var mok = new Mock<ITransactionsRepository>();

            TransactionEntity obj = new TransactionEntity
            {
                Id = "test",
                Account = 1,
                Value = 60.0,
                IsDebit = true,
                Date = DateTime.Now
            };


            mok.Setup(a => a.FindByAccountAsync(It.IsAny<int>())).Returns(Task.FromResult(obj));

            var controller = new TransactionsController(mok.Object);

            TransactionEntity obj2 = new TransactionEntity
            {
                Account = 1,
                Value = -50.0,
                IsDebit = true,
                Date = DateTime.Now
            };

            var result = await controller.Debit(obj2);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Cannot debit account with value less than 0 or 0");
        }

        [Test]
        public async Task TestDebitNoAccount()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            var result = await controller.Debit(new TransactionEntity
            {
                Account = 1,
                Value = 50.0,
                IsDebit = true,
                Date = DateTime.Now
            });

            result.Should().BeOfType<NotFoundObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account not found");
        }

        [Test]
        public async Task TestDebitSmallerBalance()
        {
            var mok = new Mock<ITransactionsRepository>();

            mok.Setup(a => a.FindByAccountAsync(It.IsAny<int>())).Returns(Task.FromResult(
                new TransactionEntity
                {
                    Id = "test",
                    Account = 1,
                    Date = DateTime.Now,
                    IsDebit = true,
                    Value = 10.0
                }));

            mok.Setup(a => a.BalanceAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(10.0));

            var controller = new TransactionsController(mok.Object);

            var result = await controller.Debit(new TransactionEntity
            {
                Account = 1,
                Value = 50.0,
                IsDebit = true,
                Date = DateTime.Now
            });

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Balance must be greater than amount to be debited");
        }

        [Test]
        public async Task TestDebitIsNotDebit()
        {
            var mok = new Mock<ITransactionsRepository>();

            mok.Setup(a => a.FindByAccountAsync(It.IsAny<int>())).Returns(Task.FromResult(
                new TransactionEntity
                {
                    Id = "test",
                    Account = 1,
                    Date = DateTime.Now,
                    IsDebit = true,
                    Value = 10.0
                }));

            mok.Setup(a => a.BalanceAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(60.0));

            var controller = new TransactionsController(mok.Object);

            var result = await controller.Debit(new TransactionEntity
            {
                Account = 1,
                Value = 50.0,
                IsDebit = false,
                Date = DateTime.Now
            });

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Operation must be Debit");
        }
        #endregion

        #region AccountExtract tests
        [Test]
        public async Task TestAccountExtract()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            mok.Setup(a => a.FindByAccountAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(
                        new TransactionEntity
                        {
                            Id = "test",
                            Account = 1,
                            Date = DateTime.Now,
                            IsDebit = true,
                            Value = 10.0
                        }));

            List<TransactionEntity> list = new List<TransactionEntity>();
            list.Add(new TransactionEntity
            {
                Id = "test",
                Account = 1,
                Date = DateTime.Now,
                IsDebit = true,
                Value = 10.0
            });

            mok.Setup(a => a.ExtractAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(list));

            var result = await controller.AccountExtract(1);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(list);
        }

        [Test]
        public async Task TestAccountExtractNoAccount()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            var result = await controller.AccountExtract(1);

            result.Should().BeOfType<NotFoundObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account not found");
        }

        [Test]
        public async Task TestAccountExtractNullAccount()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            var result = await controller.AccountExtract(null);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account not provided");
        }

        [Test]
        public async Task TestAccountExtractNegativeAccount()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            var result = await controller.AccountExtract(-50);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account number must be greater than 0");
        }

        [Test]
        public async Task TestAccountExtractAccountNumberZero()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            var result = await controller.AccountExtract(0);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account number must be greater than 0");
        }
        #endregion

        #region MonthlyReport tests
        [Test]
        public async Task TestMonthlyReport()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            mok.Setup(a => a.FindByAccountAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(
                        new TransactionEntity
                        {
                            Id = "test",
                            Account = 1,
                            Date = DateTime.Now,
                            IsDebit = true,
                            Value = 10.0
                        }));

            List<MonthlyReportViewModel> list = new List<MonthlyReportViewModel>();
            list.Add(new MonthlyReportViewModel
            {
                Account = 1,
                Date = DateTime.Now,
                Credit = 100.0,
                Debit = -50.0,
                Balance = 50.0
            });

            mok.Setup(a => a.MonthlyReportAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(list));

            var result = await controller.MonthlyReport(1, 2019);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(list);
        }

        [Test]
        public async Task TestMonthlyReportNoAccount()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            var result = await controller.MonthlyReport(1, 2019);

            result.Should().BeOfType<NotFoundObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account not found");
        }

        [Test]
        public async Task TestMonthlyReportNullAccount()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            var result = await controller.MonthlyReport(null, 1);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account not provided");
        }

        [Test]
        public async Task TestMonthlyReportNullYear()
        {
            var mok = new Mock<ITransactionsRepository>();

            mok.Setup(a => a.FindByAccountAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(
                    new TransactionEntity
                    {
                        Id = "test",
                        Account = 1,
                        Date = DateTime.Now,
                        IsDebit = true,
                        Value = 0.0
                    }));

            var controller = new TransactionsController(mok.Object);

            var result = await controller.MonthlyReport(1, null);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Year not provided");
        }

        [Test]
        public async Task TestMonthlyReportNullAccountAndYear()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            var result = await controller.MonthlyReport(null, null);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account not provided");
        }

        [Test]
        public async Task TestMonthlyReportNegativeAccount()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            var result = await controller.MonthlyReport(-50, 2019);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account number must be greater than 0");
        }

        [Test]
        public async Task TestMonthlyReportAccountValueZero()
        {
            var mok = new Mock<ITransactionsRepository>();

            var controller = new TransactionsController(mok.Object);

            var result = await controller.MonthlyReport(0, 2019);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Account number must be greater than 0");
        }

        [Test]
        public async Task TestMonthlyReportValueYearLessThan1950()
        {
            var mok = new Mock<ITransactionsRepository>();

            mok.Setup(a => a.FindByAccountAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(
                    new TransactionEntity
                    {
                        Id = "test",
                        Account = 1,
                        Date = DateTime.Now,
                        IsDebit = true,
                        Value = 0.0
                    }));

            var controller = new TransactionsController(mok.Object);

            var result = await controller.MonthlyReport(1, 1899);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Year is not valid");
        }

        [Test]
        public async Task TestMonthlyReportValueYearGreaterThanActualYear()
        {
            var mok = new Mock<ITransactionsRepository>();

            mok.Setup(a => a.FindByAccountAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(
                    new TransactionEntity
                    {
                        Id = "test",
                        Account = 1,
                        Date = DateTime.Now,
                        IsDebit = true,
                        Value = 0.0
                    }));

            var controller = new TransactionsController(mok.Object);

            var result = await controller.MonthlyReport(1, DateTime.Now.Year + 1);

            result.Should().BeOfType<BadRequestObjectResult>().Which.Value
                .Should().BeOfType<ErrorViewModel>().Which.Error.Should().Be("Year is not valid");
        }
        #endregion
    }
}