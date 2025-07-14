namespace IMS.Models.ProMan
{
    public class ConversionPrintViewModel
    {
        public string DocumentNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ConversionItemViewModel> ConsumedItems { get; set; }
        public List<ConversionItemViewModel> ProducedItems { get; set; }
    }
}
