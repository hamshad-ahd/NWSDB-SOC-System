using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWSDB.Api.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;

        public int BillId { get; set; }

        public Bill Bill { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string ReceiptNumber { get; set; } = string.Empty; // e.g. REC-2026-8801

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(30)]
        public string PaymentMethod { get; set; } = string.Empty; // "Card", "Cash"

        [StringLength(100)]
        public string? TransactionId { get; set; } // Bank transaction ID

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Success"; // Success, Failed
    }
}
