using System;

namespace BankAPI.Model
{
    public class MonthlyReportViewModel
    {
        public int Id { get; set; }
        public int Account { get; set; }
        public DateTime Date { get; set; }
        public double Credit { get; set; }
        public double Debit { get; set; }
        public double Balance { get; set; }
    }
}
