namespace IMS.Models.ProMan
{
    public class ConversionDocumentViewModel
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ConsumedItemCount { get; set; }
        public int ProducedItemCount { get; set; }
    }
}
