namespace IMS.Areas.AccountManagement.Models
{
    public class DashboardViewModel
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public int TotalTransactions { get; set; }  // تعداد تراکنش‌ها
        public List<Transaction> RecentTransactions { get; set; }
        public List<CostCenter> CostCenters { get; set; }
        public List<string> ChartLabels { get; set; }
        public List<double> ChartValues { get; set; }

        public List<PieChartItem> CostCenterChart { get; set; }
    }
    public class PieChartItem
    {
        public string Label { get; set; }
        public double Value { get; set; }
    }
}
