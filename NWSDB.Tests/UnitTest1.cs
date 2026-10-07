using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NWSDB.Api.Data;
using NWSDB.Api.DTOs;
using NWSDB.Api.Models;
using NWSDB.Api.Services;
using Bank.Api.Data;
using Bank.Api.Services;
using BankAccountModel = Bank.Api.Models.BankAccount;
using BankPaymentRequestDto = Bank.Api.DTOs.BankPaymentRequestDto;
using Xunit;

namespace NWSDB.Tests
{
    public class SystemUnitTests
    {
        private (NwsdbDbContext context, SqliteConnection connection) GetNwsdbTestDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<NwsdbDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new NwsdbDbContext(options);
            context.Database.EnsureCreated();

            // Seed Customer 1
            var customer1 = new Customer
            {
                AccountNumber = "NWSDB-1001",
                Name = "Kavindu Perera",
                Address = "142/B, Galle Road",
                Phone = "+94771234567",
                Email = "kavindu@gmail.com"
            };
            context.Customers.Add(customer1);

            // Seed Customer 2
            var customer2 = new Customer
            {
                AccountNumber = "NWSDB-1002",
                Name = "Nimali Fernando",
                Address = "55, Kandy Road, Kiribathgoda",
                Phone = "+94719876543",
                Email = "nimali.f@yahoo.com"
            };
            context.Customers.Add(customer2);
            context.SaveChanges();

            // Seed Test Bill for Customer 1
            var bill1 = new Bill
            {
                CustomerId = customer1.Id,
                BillNumber = "BILL-TEST-001",
                BillingMonth = "2026-08",
                UnitsUsed = 28,
                BillAmount = 4850.00m,
                PaidAmount = 0,
                DueDate = DateTime.UtcNow.AddDays(10),
                Status = "Pending"
            };
            context.Bills.Add(bill1);

            // Seed Test Bill for Customer 2
            var bill2 = new Bill
            {
                CustomerId = customer2.Id,
                BillNumber = "BILL-TEST-002",
                BillingMonth = "2026-08",
                UnitsUsed = 32,
                BillAmount = 3170.00m,
                PaidAmount = 0,
                DueDate = DateTime.UtcNow.AddDays(10),
                Status = "Pending"
            };
            context.Bills.Add(bill2);
            context.SaveChanges();

            return (context, connection);
        }

        private (BankDbContext context, SqliteConnection connection) GetBankTestDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new BankDbContext(options);
            context.Database.EnsureCreated();

            context.BankAccounts.Add(new BankAccountModel
            {
                CardNumber = "4532718293841029",
                CardHolderName = "Kavindu Perera",
                ExpiryDate = "12/28",
                CVV = "123",
                Balance = 150000.00m,
                IsActive = true
            });

            context.BankAccounts.Add(new BankAccountModel
            {
                CardNumber = "5412751234567890",
                CardHolderName = "Nimali Fernando",
                ExpiryDate = "09/27",
                CVV = "456",
                Balance = 32500.50m,
                IsActive = true
            });

            context.SaveChanges();
            return (context, connection);
        }

        [Fact]
        public async Task CustomerService_VerifyCustomer_ValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var (context, connection) = GetNwsdbTestDbContext();
            using (connection)
            using (context)
            {
                var service = new CustomerService(context);

                // Act
                var result = await service.VerifyCustomerAsync(new CustomerVerifyRequestDto
                {
                    AccountNumber = "NWSDB-1001",
                    Name = "Kavindu Perera"
                });

                // Assert
                Assert.True(result.Success);
                Assert.NotNull(result.Customer);
                Assert.Equal("Kavindu Perera", result.Customer.Name);
            }
        }

        [Fact]
        public async Task CustomerService_VerifyCustomer_InvalidName_ReturnsUnauthorized()
        {
            // Arrange
            var (context, connection) = GetNwsdbTestDbContext();
            using (connection)
            using (context)
            {
                var service = new CustomerService(context);

                // Act
                var result = await service.VerifyCustomerAsync(new CustomerVerifyRequestDto
                {
                    AccountNumber = "NWSDB-1001",
                    Name = "Wrong Name"
                });

                // Assert
                Assert.False(result.Success);
                Assert.Null(result.Customer);
            }
        }

        [Fact]
        public async Task BillService_GetCurrentBill_ReturnsActiveBill()
        {
            // Arrange
            var (context, connection) = GetNwsdbTestDbContext();
            using (connection)
            using (context)
            {
                var service = new BillService(context);

                // Act
                var bill = await service.GetCurrentBillAsync("NWSDB-1001");

                // Assert
                Assert.NotNull(bill);
                Assert.Equal("BILL-TEST-001", bill.BillNumber);
                Assert.Equal(4850.00m, bill.BillAmount);
            }
        }

        [Fact]
        public async Task BankService_ProcessPayment_ValidCard_DeductsBalance()
        {
            // Arrange
            var (context, connection) = GetBankTestDbContext();
            using (connection)
            using (context)
            {
                var service = new BankService(context);

                // Act
                var response = await service.ProcessPaymentAsync(new BankPaymentRequestDto
                {
                    CardNumber = "4532718293841029",
                    CardHolderName = "Kavindu Perera",
                    ExpiryDate = "12/28",
                    CVV = "123",
                    Amount = 5000.00m
                });

                // Assert
                Assert.True(response.Success);
                Assert.StartsWith("TXN-BANK-", response.ReferenceNumber);
                Assert.Equal(145000.00m, response.RemainingBalance);
            }
        }

        [Fact]
        public async Task ThirdPartyPaymentService_DuplicateTransfer_IsPrevented()
        {
            // Arrange
            var (nwsdbDb, nwsdbConn) = GetNwsdbTestDbContext();
            var (bankDb, bankConn) = GetBankTestDbContext();

            using (nwsdbConn)
            using (bankConn)
            using (nwsdbDb)
            using (bankDb)
            {
                var bill = await nwsdbDb.Bills.FirstAsync();
                var customer = await nwsdbDb.Customers.FirstAsync();

                // Add an existing transferred collection
                var transferredCollection = new ThirdPartyCollection
                {
                    BillId = bill.Id,
                    CustomerId = customer.Id,
                    Amount = 4850.00m,
                    PaymentMethod = "Cash",
                    CollectionStatus = "Collected",
                    CollectedDate = DateTime.UtcNow,
                    TransferDate = DateTime.UtcNow,
                    TransferStatus = "Transferred",
                    TransferReference = "TRF-TP-123456"
                };
                nwsdbDb.ThirdPartyCollections.Add(transferredCollection);
                await nwsdbDb.SaveChangesAsync();

                var tpService = new ThirdPartyPaymentService(nwsdbDb, null!);

                // Act - Attempt to transfer already transferred collection using CollectionId
                var result = await tpService.TransferCollectionAsync(new ThirdPartyTransferRequestDto
                {
                    CollectionId = transferredCollection.Id
                });

                // Assert
                Assert.False(result.Success);
                Assert.Contains("Duplicate transfer rejected", result.Message);
            }
        }

        [Fact]
        public async Task ThirdPartyPaymentService_CashCollection_CreatesPendingCollection()
        {
            // Arrange
            var (nwsdbDb, nwsdbConn) = GetNwsdbTestDbContext();
            using (nwsdbConn)
            using (nwsdbDb)
            {
                var bill = await nwsdbDb.Bills.FirstAsync();
                var customer = await nwsdbDb.Customers.FirstAsync();

                // Note: BankApiService is null! Verifying Bank.Api is NEVER called for Cash collections.
                var tpService = new ThirdPartyPaymentService(nwsdbDb, null!);

                // Act - Stage 1 Cash Collection
                var result = await tpService.CollectCashPaymentAsync(new ThirdPartyCashCollectionRequestDto
                {
                    BillNumber = bill.BillNumber,
                    AccountNumber = customer.AccountNumber,
                    Amount = 4850.00m,
                    CollectorId = "AGENT-5501"
                });

                // Assert
                Assert.True(result.Success);
                Assert.Equal("Collected", result.CollectionStatus);
                Assert.Equal("Pending", result.TransferStatus);
                Assert.True(result.CollectionId > 0);

                var savedCollection = await nwsdbDb.ThirdPartyCollections.FindAsync(result.CollectionId);
                Assert.NotNull(savedCollection);
                Assert.Equal("Cash", savedCollection.PaymentMethod);
                Assert.Equal("Collected", savedCollection.CollectionStatus);
                Assert.Equal("Pending", savedCollection.TransferStatus);
                Assert.Null(savedCollection.TransferReference);
                Assert.Null(savedCollection.BankTransactionId); // Null for Cash collections
            }
        }

        [Fact]
        public async Task ThirdPartyPaymentService_CardCollection_PersistsBankTransactionId()
        {
            // Arrange
            var (nwsdbDb, nwsdbConn) = GetNwsdbTestDbContext();
            using (nwsdbConn)
            using (nwsdbDb)
            {
                var bill = await nwsdbDb.Bills.FirstAsync();
                var customer = await nwsdbDb.Customers.FirstAsync();

                var fakeBankApi = new FakeBankApiService("TXN-BANK-CARD-991122");
                var tpService = new ThirdPartyPaymentService(nwsdbDb, fakeBankApi);

                // Act - Stage 1 Card Collection
                var result = await tpService.CollectCardPaymentAsync(new ThirdPartyCardCollectionRequestDto
                {
                    BillNumber = bill.BillNumber,
                    AccountNumber = customer.AccountNumber,
                    Amount = 4850.00m,
                    CardNumber = "4532718293841029",
                    CardHolderName = "Kavindu Perera",
                    ExpiryDate = "12/28",
                    CVV = "123",
                    CollectorId = "AGENT-5501"
                });

                // Assert
                Assert.True(result.Success);
                Assert.Equal("TXN-BANK-CARD-991122", result.BankTransactionId);
                Assert.Equal("Collected", result.CollectionStatus);
                Assert.Equal("Pending", result.TransferStatus);
                Assert.True(result.CollectionId > 0);

                // Verify ThirdPartyCollection entity in NWSDB.db persists BankTransactionId as a decoupled string reference
                var savedCollection = await nwsdbDb.ThirdPartyCollections.FindAsync(result.CollectionId);
                Assert.NotNull(savedCollection);
                Assert.Equal("Card", savedCollection.PaymentMethod);
                Assert.Equal("TXN-BANK-CARD-991122", savedCollection.BankTransactionId);
                Assert.Equal("Collected", savedCollection.CollectionStatus);
                Assert.Equal("Pending", savedCollection.TransferStatus);
            }
        }

        [Fact]
        public async Task ThirdPartyPaymentService_Transfer_CardCollection_PropagatesBankTransactionId()
        {
            // Arrange
            var (nwsdbDb, nwsdbConn) = GetNwsdbTestDbContext();
            using (nwsdbConn)
            using (nwsdbDb)
            {
                var bill = await nwsdbDb.Bills.FirstAsync();
                var customer = await nwsdbDb.Customers.FirstAsync();

                // 1. Existing card collection in Stage 1 with BankTransactionId
                var cardCollection = new ThirdPartyCollection
                {
                    BillId = bill.Id,
                    CustomerId = customer.Id,
                    Amount = 4850.00m,
                    PaymentMethod = "Card",
                    BankTransactionId = "TXN-BANK-CARD-991122",
                    CollectionStatus = "Collected",
                    CollectedDate = DateTime.UtcNow,
                    TransferStatus = "Pending"
                };
                nwsdbDb.ThirdPartyCollections.Add(cardCollection);
                await nwsdbDb.SaveChangesAsync();

                // Note: BankApiService is null! Transfer NEVER calls Bank.Api.
                var tpService = new ThirdPartyPaymentService(nwsdbDb, null!);

                // Act - Stage 2 Transfer by CollectionId
                var result = await tpService.TransferCollectionAsync(new ThirdPartyTransferRequestDto
                {
                    CollectionId = cardCollection.Id
                });

                // Assert
                Assert.True(result.Success);
                Assert.Equal("TXN-BANK-CARD-991122", result.BankTransactionId);
                Assert.Equal("Transferred", result.BillStatus == "Paid" ? "Transferred" : "Pending");

                // Verify NWSDB Payment record has BankTransactionId as TransactionId
                var payment = await nwsdbDb.Payments.FirstOrDefaultAsync(p => p.BillId == bill.Id);
                Assert.NotNull(payment);
                Assert.Equal("TXN-BANK-CARD-991122", payment.TransactionId);

                // Verify ThirdPartyCollection retained BankTransactionId
                var updatedCollection = await nwsdbDb.ThirdPartyCollections.FindAsync(cardCollection.Id);
                Assert.NotNull(updatedCollection);
                Assert.Equal("TXN-BANK-CARD-991122", updatedCollection.BankTransactionId);
                Assert.Equal("Transferred", updatedCollection.TransferStatus);
            }
        }

        [Fact]
        public async Task ThirdPartyPaymentService_SuccessfulTransfer_SettlesBillAndMarksTransferred()
        {
            // Arrange
            var (nwsdbDb, nwsdbConn) = GetNwsdbTestDbContext();
            using (nwsdbConn)
            using (nwsdbDb)
            {
                var bill = await nwsdbDb.Bills.FirstAsync();
                var customer = await nwsdbDb.Customers.FirstAsync();

                // 1. Create a Pending collection in Stage 1
                var pendingCollection = new ThirdPartyCollection
                {
                    BillId = bill.Id,
                    CustomerId = customer.Id,
                    Amount = 4850.00m,
                    PaymentMethod = "Cash",
                    CollectionStatus = "Collected",
                    CollectedDate = DateTime.UtcNow,
                    TransferStatus = "Pending"
                };
                nwsdbDb.ThirdPartyCollections.Add(pendingCollection);
                await nwsdbDb.SaveChangesAsync();

                // Note: BankApiService is null! Verifying Bank.Api is NEVER called during transfer.
                var tpService = new ThirdPartyPaymentService(nwsdbDb, null!);

                // Act - Stage 2 Transfer by CollectionId
                var result = await tpService.TransferCollectionAsync(new ThirdPartyTransferRequestDto
                {
                    CollectionId = pendingCollection.Id
                });

                // Assert
                Assert.True(result.Success);
                Assert.NotNull(result.ReceiptNumber);
                Assert.NotNull(result.TransferReference);
                Assert.StartsWith("REC-TP-", result.ReceiptNumber);
                Assert.StartsWith("TRF-TP-", result.TransferReference);
                Assert.Equal(4850.00m, result.AmountPaid);
                Assert.Equal("Paid", result.BillStatus);
                Assert.Equal(0.00m, result.RemainingBillBalance);

                // Verify Bill was updated
                var updatedBill = await nwsdbDb.Bills.FindAsync(bill.Id);
                Assert.NotNull(updatedBill);
                Assert.Equal(4850.00m, updatedBill.PaidAmount);
                Assert.Equal("Paid", updatedBill.Status);

                // Verify Payment record was created
                var payment = await nwsdbDb.Payments.FirstOrDefaultAsync(p => p.BillId == bill.Id);
                Assert.NotNull(payment);
                Assert.Equal(result.ReceiptNumber, payment.ReceiptNumber);
                Assert.Equal(4850.00m, payment.Amount);

                // Verify ThirdPartyCollection was marked Transferred
                var updatedCollection = await nwsdbDb.ThirdPartyCollections.FindAsync(pendingCollection.Id);
                Assert.NotNull(updatedCollection);
                Assert.Equal("Transferred", updatedCollection.TransferStatus);
                Assert.NotNull(updatedCollection.TransferDate);
                Assert.Equal(result.TransferReference, updatedCollection.TransferReference);
            }
        }

        [Fact]
        public async Task ThirdPartyPaymentService_Transfer_InvalidCollectionId_Fails()
        {
            // Arrange
            var (nwsdbDb, nwsdbConn) = GetNwsdbTestDbContext();
            using (nwsdbConn)
            using (nwsdbDb)
            {
                var tpService = new ThirdPartyPaymentService(nwsdbDb, null!);

                // Act
                var result = await tpService.TransferCollectionAsync(new ThirdPartyTransferRequestDto
                {
                    CollectionId = 99999
                });

                // Assert
                Assert.False(result.Success);
                Assert.Contains("not found", result.Message);
            }
        }

        [Fact]
        public async Task ThirdPartyPaymentService_Transfer_DuplicateTransfer_RejectsDuplicate()
        {
            // Arrange
            var (nwsdbDb, nwsdbConn) = GetNwsdbTestDbContext();
            using (nwsdbConn)
            using (nwsdbDb)
            {
                var bill = await nwsdbDb.Bills.FirstAsync();
                var alreadyTransferred = new ThirdPartyCollection
                {
                    BillId = bill.Id,
                    CustomerId = bill.CustomerId,
                    Amount = 1000.00m,
                    PaymentMethod = "Cash",
                    CollectionStatus = "Collected",
                    CollectedDate = DateTime.UtcNow.AddMinutes(-10),
                    TransferStatus = "Transferred",
                    TransferDate = DateTime.UtcNow.AddMinutes(-5),
                    TransferReference = "TRF-TP-ALREADY-DONE"
                };
                nwsdbDb.ThirdPartyCollections.Add(alreadyTransferred);
                await nwsdbDb.SaveChangesAsync();

                var tpService = new ThirdPartyPaymentService(nwsdbDb, null!);

                // Act - Attempt duplicate transfer
                var result = await tpService.TransferCollectionAsync(new ThirdPartyTransferRequestDto
                {
                    CollectionId = alreadyTransferred.Id
                });

                // Assert
                Assert.False(result.Success);
                Assert.Contains("Duplicate transfer rejected", result.Message, StringComparison.OrdinalIgnoreCase);

                // Verify no payment was created for this duplicate attempt
                var paymentCount = await nwsdbDb.Payments.CountAsync();
                Assert.Equal(0, paymentCount);
            }
        }

        [Fact]
        public async Task ThirdPartyPaymentService_CollectCash_AmountExceedsDue_FailsValidation()
        {
            // Arrange
            var (nwsdbDb, nwsdbConn) = GetNwsdbTestDbContext();
            using (nwsdbConn)
            using (nwsdbDb)
            {
                var bill = await nwsdbDb.Bills.FirstAsync();
                var tpService = new ThirdPartyPaymentService(nwsdbDb, null!);

                // Act - Attempt to collect more than bill amount (bill is 4850, attempt 5000)
                var result = await tpService.CollectCashPaymentAsync(new ThirdPartyCashCollectionRequestDto
                {
                    AccountNumber = "NWSDB-1001",
                    BillNumber = bill.BillNumber,
                    Amount = 5000.00m,
                    CollectorId = "AGENT-001"
                });

                // Assert
                Assert.False(result.Success);
                Assert.Contains("cannot exceed", result.Message);
            }
        }

        [Fact]
        public async Task ThirdPartyPaymentService_CollectCard_AmountZeroOrNegative_FailsValidation()
        {
            // Arrange
            var (nwsdbDb, nwsdbConn) = GetNwsdbTestDbContext();
            using (nwsdbConn)
            using (nwsdbDb)
            {
                var bill = await nwsdbDb.Bills.FirstAsync();
                var tpService = new ThirdPartyPaymentService(nwsdbDb, null!);

                // Act - Attempt zero amount
                var result = await tpService.CollectCardPaymentAsync(new ThirdPartyCardCollectionRequestDto
                {
                    AccountNumber = "NWSDB-1001",
                    BillNumber = bill.BillNumber,
                    Amount = 0.00m,
                    CardNumber = "4532718293841029",
                    CardHolderName = "Kavindu Perera",
                    ExpiryDate = "12/28",
                    CVV = "123",
                    CollectorId = "AGENT-001"
                });

                // Assert
                Assert.False(result.Success);
                Assert.Contains("greater than zero", result.Message);
            }
        }

        [Fact]
        public async Task CustomerService_MultiUser_LookupIsIsolated()
        {
            var (context, connection) = GetNwsdbTestDbContext();
            using (connection)
            using (context)
            {
                var service = new CustomerService(context);

                var cust1 = await service.GetCustomerByAccountAsync("NWSDB-1001");
                var cust2 = await service.GetCustomerByAccountAsync("NWSDB-1002");
                var custNotFound = await service.GetCustomerByAccountAsync("NWSDB-9999");

                Assert.NotNull(cust1);
                Assert.Equal("Kavindu Perera", cust1.Name);

                Assert.NotNull(cust2);
                Assert.Equal("Nimali Fernando", cust2.Name);

                Assert.Null(custNotFound);
            }
        }

        [Fact]
        public async Task BillService_MultiUser_BillsAreIsolated()
        {
            var (context, connection) = GetNwsdbTestDbContext();
            using (connection)
            using (context)
            {
                var service = new BillService(context);

                var bill1 = await service.GetCurrentBillAsync("NWSDB-1001");
                var bill2 = await service.GetCurrentBillAsync("NWSDB-1002");

                Assert.NotNull(bill1);
                Assert.Equal("BILL-TEST-001", bill1.BillNumber);
                Assert.Equal(4850.00m, bill1.BillAmount);

                Assert.NotNull(bill2);
                Assert.Equal("BILL-TEST-002", bill2.BillNumber);
                Assert.Equal(3170.00m, bill2.BillAmount);
            }
        }

        [Fact]
        public async Task ThirdPartyPaymentService_CrossCustomerBillTampering_Fails()
        {
            var (context, connection) = GetNwsdbTestDbContext();
            using (connection)
            using (context)
            {
                var tpService = new ThirdPartyPaymentService(context, null!);

                // Attempting to collect payment for Customer 1 (NWSDB-1001) using Customer 2's bill (BILL-TEST-002)
                var result = await tpService.CollectCashPaymentAsync(new ThirdPartyCashCollectionRequestDto
                {
                    AccountNumber = "NWSDB-1001",
                    BillNumber = "BILL-TEST-002",
                    Amount = 1000.00m,
                    CollectorId = "AGENT-5501"
                });

                Assert.False(result.Success);
                Assert.Contains("not found for account", result.Message);
            }
        }

        [Fact]
        public async Task ThirdPartyPaymentService_MultiUser_IndependentCollectionsAndTransfers()
        {
            var (context, connection) = GetNwsdbTestDbContext();
            using (connection)
            using (context)
            {
                var tpService = new ThirdPartyPaymentService(context, null!);

                // 1. Cash collection for Customer 1
                var col1 = await tpService.CollectCashPaymentAsync(new ThirdPartyCashCollectionRequestDto
                {
                    AccountNumber = "NWSDB-1001",
                    BillNumber = "BILL-TEST-001",
                    Amount = 1000.00m,
                    CollectorId = "AGENT-5501"
                });

                // 2. Cash collection for Customer 2
                var col2 = await tpService.CollectCashPaymentAsync(new ThirdPartyCashCollectionRequestDto
                {
                    AccountNumber = "NWSDB-1002",
                    BillNumber = "BILL-TEST-002",
                    Amount = 1500.00m,
                    CollectorId = "AGENT-5502"
                });

                Assert.True(col1.Success);
                Assert.True(col2.Success);
                Assert.NotEqual(col1.CollectionId, col2.CollectionId);

                // 3. Transfer Customer 1's collection
                var trf1 = await tpService.TransferCollectionAsync(new ThirdPartyTransferRequestDto
                {
                    CollectionId = col1.CollectionId
                });
                Assert.True(trf1.Success);
                Assert.Equal(1000.00m, trf1.AmountPaid);

                // Verify ONLY Customer 1's bill changed; Customer 2's bill remains untouched
                var b1After = await context.Bills.FirstAsync(b => b.BillNumber == "BILL-TEST-001");
                var b2After = await context.Bills.FirstAsync(b => b.BillNumber == "BILL-TEST-002");
                Assert.Equal(1000.00m, b1After.PaidAmount);
                Assert.Equal(0.00m, b2After.PaidAmount);

                // 4. Duplicate transfer attempt on Customer 1's collection must be rejected
                var trf1Dup = await tpService.TransferCollectionAsync(new ThirdPartyTransferRequestDto
                {
                    CollectionId = col1.CollectionId
                });
                Assert.False(trf1Dup.Success);
                Assert.Contains("Duplicate transfer rejected", trf1Dup.Message);

                // 5. Transfer Customer 2's collection succeeds independently
                var trf2 = await tpService.TransferCollectionAsync(new ThirdPartyTransferRequestDto
                {
                    CollectionId = col2.CollectionId
                });
                Assert.True(trf2.Success);
                Assert.Equal(1500.00m, trf2.AmountPaid);

                var b2Final = await context.Bills.FirstAsync(b => b.BillNumber == "BILL-TEST-002");
                Assert.Equal(1500.00m, b2Final.PaidAmount);
            }
        }

        private class FakeBankApiService : IBankApiService
        {
            private readonly string _referenceNumber;

            public FakeBankApiService(string referenceNumber = "TXN-BANK-123456")
            {
                _referenceNumber = referenceNumber;
            }

            public Task<BankPaymentResponseDto> ProcessPaymentAsync(NWSDB.Api.DTOs.BankPaymentRequestDto request)
            {
                return Task.FromResult(new BankPaymentResponseDto
                {
                    Success = true,
                    Message = "Payment approved.",
                    ReferenceNumber = _referenceNumber,
                    RemainingBalance = 100000.00m,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        [Fact]
        public void ResetAndVerifyPhysicalDatabases()
        {
            var nwsdbDbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../NWSDB.Api/NWSDB.db"));
            Assert.True(File.Exists(nwsdbDbPath), $"NWSDB.db not found at {nwsdbDbPath}");

            var nwsdbOptions = new DbContextOptionsBuilder<NwsdbDbContext>()
                .UseSqlite($"Data Source={nwsdbDbPath}")
                .Options;

            using (var nwsdbContext = new NwsdbDbContext(nwsdbOptions))
            {
                DbInitializer.ResetTestData(nwsdbContext);

                var customers = nwsdbContext.Customers.ToList();
                Assert.Equal(5, customers.Count);

                var expectedAmounts = new Dictionary<string, decimal>
                {
                    { "NWSDB-1001", 3680.00m },
                    { "NWSDB-1002", 3170.00m },
                    { "NWSDB-1003", 375.00m },
                    { "NWSDB-1004", 1275.00m },
                    { "NWSDB-1005", 2150.00m }
                };

                foreach (var (accNum, expectedAmount) in expectedAmounts)
                {
                    var cust = customers.First(c => c.AccountNumber == accNum);
                    var augustBill = nwsdbContext.Bills
                        .FirstOrDefault(b => b.CustomerId == cust.Id && b.BillingMonth == "August 2026");

                    Assert.NotNull(augustBill);
                    Assert.Equal("Pending", augustBill.Status);
                    Assert.Equal(expectedAmount, augustBill.BillAmount);
                    Assert.Equal(0.00m, augustBill.PaidAmount);

                    // Ensure remaining balance equals amount due
                    var remaining = augustBill.BillAmount - augustBill.PaidAmount;
                    Assert.Equal(expectedAmount, remaining);

                    // Ensure no payment exists for this August bill
                    var hasPayment = nwsdbContext.Payments.Any(p => p.BillId == augustBill.Id);
                    Assert.False(hasPayment, $"August bill {augustBill.BillNumber} should not have payment records.");

                    // Ensure no collection exists for this August bill
                    var hasCollection = nwsdbContext.ThirdPartyCollections.Any(c => c.BillId == augustBill.Id);
                    Assert.False(hasCollection, $"August bill {augustBill.BillNumber} should not have collection records.");
                }
            }

            var bankDbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../Bank.Api/Bank.db"));
            Assert.True(File.Exists(bankDbPath), $"Bank.db not found at {bankDbPath}");

            var bankOptions = new DbContextOptionsBuilder<BankDbContext>()
                .UseSqlite($"Data Source={bankDbPath}")
                .Options;

            using (var bankContext = new BankDbContext(bankOptions))
            {
                BankDbInitializer.ResetTestData(bankContext);

                var acc1 = bankContext.BankAccounts.First(a => a.CardNumber == "4532718293841029");
                Assert.Equal(75000.00m, acc1.Balance);

                var acc2 = bankContext.BankAccounts.First(a => a.CardNumber == "5412751234567890");
                Assert.Equal(32500.50m, acc2.Balance);

                var acc3 = bankContext.BankAccounts.First(a => a.CardNumber == "4000123456789010");
                Assert.Equal(1500.00m, acc3.Balance);
            }
        }

        [Fact]
        public async Task FullEndToEndWorkflow_ValidateNwsdbAndThirdPartyAndBank()
        {
            var nwsdbDbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../NWSDB.Api/NWSDB.db"));
            var bankDbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../Bank.Api/Bank.db"));

            var nwsdbOptions = new DbContextOptionsBuilder<NwsdbDbContext>()
                .UseSqlite($"Data Source={nwsdbDbPath}")
                .Options;

            var bankOptions = new DbContextOptionsBuilder<BankDbContext>()
                .UseSqlite($"Data Source={bankDbPath}")
                .Options;

            using var nwsdbContext = new NwsdbDbContext(nwsdbOptions);
            using var bankContext = new BankDbContext(bankOptions);

            // 1. Initial Reset to clean state
            DbInitializer.ResetTestData(nwsdbContext);
            BankDbInitializer.ResetTestData(bankContext);

            var bankService = new BankService(bankContext);
            var directBankApiService = new LocalDirectBankApiService(bankService);
            var paymentService = new PaymentService(nwsdbContext, directBankApiService);
            var tpPaymentService = new ThirdPartyPaymentService(nwsdbContext, directBankApiService);
            var billService = new BillService(nwsdbContext);

            // ==========================================
            // TEST 1: NWSDB Website CARD payment on Account 1 (NWSDB-1001, Rs. 3,680.00)
            // ==========================================
            var bill1 = await billService.GetCurrentBillAsync("NWSDB-1001");
            Assert.NotNull(bill1);
            Assert.Equal("INV-2026-0801", bill1.BillNumber);
            Assert.Equal(3680.00m, bill1.BillAmount);
            Assert.Equal(0.00m, bill1.PaidAmount);
            Assert.Equal("Pending", bill1.Status);

            var cardPaymentRes = await paymentService.ProcessCardPaymentAsync(new ProcessCardPaymentRequestDto
            {
                AccountNumber = "NWSDB-1001",
                BillNumber = "INV-2026-0801",
                Amount = 3680.00m,
                CardNumber = "4532718293841029",
                CardHolderName = "Kavindu Perera",
                ExpiryDate = "12/28",
                CVV = "123"
            });

            Assert.True(cardPaymentRes.Success);
            Assert.Equal("Paid", cardPaymentRes.BillStatus);
            Assert.Equal(0.00m, cardPaymentRes.RemainingBillBalance);

            var bill1After = await billService.GetCurrentBillAsync("NWSDB-1001");
            Assert.Equal("Paid", bill1After?.Status);
            Assert.Equal(3680.00m, bill1After?.PaidAmount);

            // Verify bank balance was deducted
            var bankAcc1 = await bankContext.BankAccounts.FirstAsync(a => a.CardNumber == "4532718293841029");
            Assert.Equal(71320.00m, bankAcc1.Balance);

            // ==========================================
            // TEST 2: ThirdParty CASH collection & transfer on Account 2 (NWSDB-1002, Rs. 3,170.00)
            // ==========================================
            var bill2 = await billService.GetCurrentBillAsync("NWSDB-1002");
            Assert.NotNull(bill2);
            Assert.Equal(3170.00m, bill2.BillAmount);
            Assert.Equal(0.00m, bill2.PaidAmount);

            // Stage 1: Cash Collection (Bank.Api is NOT called)
            var cashColRes = await tpPaymentService.CollectCashPaymentAsync(new ThirdPartyCashCollectionRequestDto
            {
                AccountNumber = "NWSDB-1002",
                BillNumber = "INV-2026-0802",
                Amount = 3170.00m,
                CollectorId = "AGENT-5501"
            });
            Assert.True(cashColRes.Success);
            Assert.Equal("Collected", cashColRes.CollectionStatus);
            Assert.Equal("Pending", cashColRes.TransferStatus);

            // Bill remains Pending before transfer
            var bill2Mid = await nwsdbContext.Bills.FirstAsync(b => b.BillNumber == "INV-2026-0802");
            Assert.Equal("Pending", bill2Mid.Status);
            Assert.Equal(0.00m, bill2Mid.PaidAmount);

            // Stage 2: Transfer to NWSDB
            var cashTrfRes = await tpPaymentService.TransferCollectionAsync(new ThirdPartyTransferRequestDto
            {
                CollectionId = cashColRes.CollectionId
            });
            Assert.True(cashTrfRes.Success);
            Assert.Equal("Paid", cashTrfRes.BillStatus);
            Assert.Equal(0.00m, cashTrfRes.RemainingBillBalance);

            // Duplicate transfer must be rejected
            var cashDupRes = await tpPaymentService.TransferCollectionAsync(new ThirdPartyTransferRequestDto
            {
                CollectionId = cashColRes.CollectionId
            });
            Assert.False(cashDupRes.Success);
            Assert.Contains("Duplicate transfer rejected", cashDupRes.Message);

            // ==========================================
            // TEST 3: ThirdParty CARD collection & transfer on Account 3 (NWSDB-1003, Rs. 375.00)
            // ==========================================
            var bill3 = await billService.GetCurrentBillAsync("NWSDB-1003");
            Assert.NotNull(bill3);
            Assert.Equal(375.00m, bill3.BillAmount);

            // Stage 1: Card Collection (Debits Bank.Api)
            var cardColRes = await tpPaymentService.CollectCardPaymentAsync(new ThirdPartyCardCollectionRequestDto
            {
                AccountNumber = "NWSDB-1003",
                BillNumber = "INV-2026-0803",
                Amount = 375.00m,
                CollectorId = "AGENT-5501",
                CardNumber = "4000123456789010",
                CardHolderName = "Saman Silva",
                ExpiryDate = "11/26",
                CVV = "789"
            });
            Assert.True(cardColRes.Success);
            Assert.Equal("Collected", cardColRes.CollectionStatus);

            // Bank balance should be 1500 - 375 = 1125
            var bankAcc3 = await bankContext.BankAccounts.FirstAsync(a => a.CardNumber == "4000123456789010");
            Assert.Equal(1125.00m, bankAcc3.Balance);

            // Stage 2: Transfer to NWSDB (Bank is NOT debited again)
            var cardTrfRes = await tpPaymentService.TransferCollectionAsync(new ThirdPartyTransferRequestDto
            {
                CollectionId = cardColRes.CollectionId
            });
            Assert.True(cardTrfRes.Success);
            Assert.Equal("Paid", cardTrfRes.BillStatus);

            // Bank balance remains 1125
            bankAcc3 = await bankContext.BankAccounts.FirstAsync(a => a.CardNumber == "4000123456789010");
            Assert.Equal(1125.00m, bankAcc3.Balance);

            // ==========================================
            // TEST 4: Unsuccessful card payment (Expired Card) does NOT mark bill as paid
            // ==========================================
            var failedPayRes = await paymentService.ProcessCardPaymentAsync(new ProcessCardPaymentRequestDto
            {
                AccountNumber = "NWSDB-1004",
                BillNumber = "INV-2026-0804",
                Amount = 1275.00m,
                CardNumber = "4111111111111111", // Expired card (01/22)
                CardHolderName = "Dilani Jayawardena",
                ExpiryDate = "01/22",
                CVV = "111"
            });
            Assert.False(failedPayRes.Success);
            var bill4 = await nwsdbContext.Bills.FirstAsync(b => b.BillNumber == "INV-2026-0804");
            Assert.Equal("Pending", bill4.Status);
            Assert.Equal(0.00m, bill4.PaidAmount);

            // ==========================================
            // TEST 5: Insufficient balance card payment has error with "Rs."
            // ==========================================
            var insuffPayRes = await paymentService.ProcessCardPaymentAsync(new ProcessCardPaymentRequestDto
            {
                AccountNumber = "NWSDB-1005",
                BillNumber = "INV-2026-0805",
                Amount = 2150.00m,
                CardNumber = "4000123456789010", // Balance is only 1125.00
                CardHolderName = "Saman Silva",
                ExpiryDate = "11/26",
                CVV = "789"
            });
            Assert.False(insuffPayRes.Success);
            Assert.Contains("Rs.", insuffPayRes.Message);
            Assert.DoesNotContain("$", insuffPayRes.Message);

            // ==========================================
            // FINAL RESET: Restore database state so ALL 5 accounts are UNPAID and ready for testing!
            // ==========================================
            DbInitializer.ResetTestData(nwsdbContext);
            BankDbInitializer.ResetTestData(bankContext);

            // Final Assertions on all 5 accounts
            var expectedFinalAmounts = new Dictionary<string, decimal>
            {
                { "NWSDB-1001", 3680.00m },
                { "NWSDB-1002", 3170.00m },
                { "NWSDB-1003", 375.00m },
                { "NWSDB-1004", 1275.00m },
                { "NWSDB-1005", 2150.00m }
            };

            foreach (var (acc, amt) in expectedFinalAmounts)
            {
                var current = await billService.GetCurrentBillAsync(acc);
                Assert.NotNull(current);
                Assert.Equal("Pending", current.Status);
                Assert.Equal(amt, current.BillAmount);
                Assert.Equal(0.00m, current.PaidAmount);
                Assert.Equal(amt, current.BillAmount - current.PaidAmount);
            }
        }

        private class LocalDirectBankApiService : IBankApiService
        {
            private readonly IBankService _bankService;

            public LocalDirectBankApiService(IBankService bankService)
            {
                _bankService = bankService;
            }

            public async Task<BankPaymentResponseDto> ProcessPaymentAsync(NWSDB.Api.DTOs.BankPaymentRequestDto request)
            {
                var bankDto = new Bank.Api.DTOs.BankPaymentRequestDto
                {
                    CardNumber = request.CardNumber,
                    CardHolderName = request.CardHolderName,
                    ExpiryDate = request.CardExpiry,
                    CVV = request.CardCvc,
                    Amount = request.Amount
                };

                var res = await _bankService.ProcessPaymentAsync(bankDto);
                return new BankPaymentResponseDto
                {
                    Success = res.Success,
                    Message = res.Message,
                    ReferenceNumber = res.ReferenceNumber,
                    RemainingBalance = res.RemainingBalance,
                    Timestamp = res.Timestamp
                };
            }
        }
    }
}