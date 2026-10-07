using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank.Api.Models
{
    public class BankTransaction
    {
        public int Id { get; set; }

        public int BankAccountId { get; set; }

        public BankAccount BankAccount { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(30)]
        public string TransactionType { get; set; } = "Debit";

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Success"; // Success, Failed, Declined

        [Required]
        [StringLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty;
    }
}
