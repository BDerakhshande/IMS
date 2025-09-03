using IMS.Domain.WarehouseManagement.Entities;

namespace IMS.Models.ProMan
{
    public class ReceiptPrintViewModel
    {
        public ReceiptOrIssue Receipt { get; set; }
        

        public List<ReceiptItemPrintViewModel> ItemsWithProject { get; set; }
    }
}
