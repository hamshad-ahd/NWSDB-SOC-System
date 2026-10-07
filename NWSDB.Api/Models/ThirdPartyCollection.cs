using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWSDB.Api.Models
{
    public class ThirdPartyCollection
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;

        public int BillId { get; set; }

        public Bill Bill { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(30)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card

        [Required]
        [StringLength(30)]
        public string CollectionStatus { get; set; } = "Collected"; // Collected, Pending

        public DateTime CollectedDate { get; set; } = DateTime.UtcNow;

        public DateTime? TransferDate { get; set; }

        [Required]
        [StringLength(30)]
        public string TransferStatus { get; set; } = "Transferred"; // Transferred, Pending

        [StringLength(100)]
        public string? TransferReference { get; set; }

        /// <summary>
        /// Reference to the external bank transaction (e.g. TXN-BANK-XXXXXX) for CARD collections.
        /// Preserved as a decoupled string reference (no direct database FK or cross-database coupling).
        /// Null for CASH collections.
        /// </summary>
        [StringLength(100)]
        public string? BankTransactionId { get; set; }
    }
}
