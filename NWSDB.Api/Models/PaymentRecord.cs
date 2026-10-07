namespace NWSDB.Api.Models
{
    public class PaymentRecord
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public int CustomerId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty; // e.g. REC-2026-8801
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // "Card", "Cash"
        public string Channel { get; set; } = string.Empty; // "NWSDB Website", "ThirdParty WebApp"
        public string? CollectorId { get; set; } // Counter Agent ID
        public string? BankTransactionId { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    }
}
