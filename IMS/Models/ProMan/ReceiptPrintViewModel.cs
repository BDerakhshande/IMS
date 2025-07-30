namespace IMS.Models.ProMan
{
    public class ReceiptPrintViewModel
    {
        public IMS.Domain.WarehouseManagement.Entities.ReceiptOrIssue Receipt { get; set; }
        public string ProjectName { get; set; } = "—";
    }
}
