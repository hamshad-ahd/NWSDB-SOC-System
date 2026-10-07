using System.ComponentModel.DataAnnotations;
using NWSDB.Api.Models;

namespace NWSDB.Api.DTOs
{
    // Auth & Customer DTOs
    public class CustomerVerifyRequestDto
    {
        [Required(ErrorMessage = "Account number is required.")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer name is required.")]
        public string Name { get; set; } = string.Empty;

        public string CustomerName { get => Name; set => Name = value; }
    }

    public class CustomerResponseDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CustomerVerifyResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CustomerResponseDto? Customer { get; set; }
    }

    // Bill DTOs
    public class BillResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public string BillingMonth { get; set; } = string.Empty;
        public int UnitsUsed { get; set; }
        public decimal BillAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount => BillAmount - PaidAmount;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public CustomerResponseDto? Customer { get; set; }
    }

    // Usage DTO
    public class WaterUsageResponseDto
    {
        public string MonthYear { get; set; } = string.Empty;
        public int UnitsConsumed { get; set; }
        public DateTime ReadingDate { get; set; }
    }

    // Payment DTOs
    public class PaymentRecordResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int BillId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ProcessCardPaymentRequestDto
    {
        [Required(ErrorMessage = "Bill number is required.")]
        public string BillNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account number is required.")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card number is required.")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cardholder name is required.")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry date is required.")]
        public string ExpiryDate { get; set; } = string.Empty; // MM/YY

        [Required(ErrorMessage = "CVV is required.")]
        public string CVV { get; set; } = string.Empty;

        [Range(0.01, 1000000.00, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        public string CardExpiry { get => ExpiryDate; set => ExpiryDate = value; }
        public string CardCvc { get => CVV; set => CVV = value; }
    }

    // Third-Party Transfer DTOs
    public class ThirdPartyCardCollectionRequestDto
    {
        [Required(ErrorMessage = "Bill number is required.")]
        public string BillNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account number is required.")]
        public string AccountNumber { get; set; } = string.Empty;

        [Range(0.01, 1000000.00, ErrorMessage = "Collection amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Card number is required.")]
        public string CardNumber { get; set; } = string.Empty;

        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry date is required.")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "CVV is required.")]
        public string CVV { get; set; } = string.Empty;

        public string CollectorId { get; set; } = "AGENT-5501";
    }

    public class ThirdPartyCardCollectionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int CollectionId { get; set; }
        public string? BankTransactionId { get; set; }
        public string CollectionStatus { get; set; } = "Collected"; // PendingCollection, Collected, Failed
        public string TransferStatus { get; set; } = "Pending";
    }

    public class ThirdPartyCashCollectionRequestDto
    {
        [Required(ErrorMessage = "Bill number is required.")]
        public string BillNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account number is required.")]
        public string AccountNumber { get; set; } = string.Empty;

        [Range(0.01, 1000000.00, ErrorMessage = "Collection amount must be greater than zero.")]
        public decimal Amount { get; set; }

        public string CollectorId { get; set; } = "AGENT-5501";
    }

    public class ThirdPartyCashCollectionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int CollectionId { get; set; }
        public string CollectionStatus { get; set; } = "Collected"; // Collected, Failed
        public string TransferStatus { get; set; } = "Pending";
    }

    public class ThirdPartyTransferRequestDto
    {
        [Required(ErrorMessage = "Collection ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "A valid Collection ID is required.")]
        public int CollectionId { get; set; }
    }

    public class PaymentReceiptResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ReceiptNumber { get; set; }
        public string? TransferReference { get; set; }
        public string? BillNumber { get; set; }
        public string? AccountNumber { get; set; }
        public string? CustomerName { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string? BankTransactionId { get; set; }
        public DateTime PaidAt { get; set; }
        public decimal RemainingBillBalance { get; set; }
        public string BillStatus { get; set; } = string.Empty;
    }

    // Bank API DTOs
    public class BankPaymentRequestDto
    {
        public string CardNumber { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;
        public decimal Amount { get; set; }

        public string CardExpiry { get => ExpiryDate; set => ExpiryDate = value; }
        public string CardCvc { get => CVV; set => CVV = value; }
    }

    public class BankPaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal RemainingBalance { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
