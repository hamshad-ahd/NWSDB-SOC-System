using System.ComponentModel.DataAnnotations;

namespace ThirdParty.WebApp.Models.ViewModels
{
    public class CustomerLookupViewModel
    {
        [Required(ErrorMessage = "Please enter an Account Number.")]
        [Display(Name = "Account Number")]
        public string SearchTerm { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class BillDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public CustomerDto? Customer { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public string BillingMonth { get; set; } = string.Empty;
        public int UnitsUsed { get; set; }
        public decimal BillAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal DueAmount => BillAmount - PaidAmount;
    }

    public class CustomerBillDetailsViewModel
    {
        public CustomerDto Customer { get; set; } = null!;
        public BillDto? ActiveBill { get; set; }
        public List<BillDto> AllBills { get; set; } = new();
    }

    public class PaymentCollectionViewModel
    {
        public string BillNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal AmountDue { get; set; }
        public decimal AmountToCollect { get; set; }

        [Required(ErrorMessage = "Payment Method selection is required.")]
        public string PaymentMethod { get; set; } = "Cash"; // Cash or Card

        public string CollectorId { get; set; } = "AGENT-5501";

        // Cash Tendered & Change
        public decimal CashTendered { get; set; }
        public decimal ChangeDue => CashTendered > AmountToCollect ? CashTendered - AmountToCollect : 0;

        // Card Details (If CARD selected)
        [Display(Name = "Cardholder Name")]
        public string? CardHolderName { get; set; }

        public string? CardNumber { get; set; }
        public string? CardExpiry { get; set; }
        public string? CardCvc { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class CardCollectionResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int CollectionId { get; set; }
        public string? BankTransactionId { get; set; }
        public string CollectionStatus { get; set; } = "Collected"; // PendingCollection, Collected, Failed
        public string TransferStatus { get; set; } = "Pending";
    }

    public class CashCollectionResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int CollectionId { get; set; }
        public string CollectionStatus { get; set; } = "Collected"; // Collected, Failed
        public string TransferStatus { get; set; } = "Pending";
    }

    public class TransferViewModel
    {
        public int CollectionId { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal CollectedAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string CollectorId { get; set; } = "AGENT-5501";
        public DateTime CollectionTimestamp { get; set; } = DateTime.UtcNow;
        public string? BankTransactionId { get; set; }

        public string CollectionStatus { get; set; } = "Collected"; // Collected
        public string TransferStatus { get; set; } = "Pending"; // Pending, Transferred

        public string? ErrorMessage { get; set; }
    }

    public class SettlementReceiptViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ReceiptNumber { get; set; } = string.Empty;
        public string TransferReference { get; set; } = string.Empty;
        public string BillNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Channel { get; set; } = "ThirdParty WebApp";
        public string? BankTransactionId { get; set; }
        public DateTime PaidAt { get; set; }
        public decimal RemainingBillBalance { get; set; }
        public string BillStatus { get; set; } = string.Empty;
    }
}
