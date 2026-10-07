using Microsoft.EntityFrameworkCore;
using NWSDB.Api.Models;

namespace NWSDB.Api.Data
{
    public static class DbInitializer
    {
        public static void Initialize(NwsdbDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Customers.Any())
            {
                return; // DB already seeded
            }

            // 1. Seed 5 Realistic Customers
            var customers = new Customer[]
            {
                new Customer
                {
                    AccountNumber = "NWSDB-1001",
                    Name = "Kavindu Perera",
                    Address = "142/B, Galle Road, Colombo 03",
                    Phone = "+94 77 123 4567",
                    Email = "kavindu.p@gmail.com"
                },
                new Customer
                {
                    AccountNumber = "NWSDB-1002",
                    Name = "Nimali Fernando",
                    Address = "55, Kandy Road, Kiribathgoda",
                    Phone = "+94 71 987 6543",
                    Email = "nimali.f@yahoo.com"
                },
                new Customer
                {
                    AccountNumber = "NWSDB-1003",
                    Name = "Saman Silva",
                    Address = "88/1, High Level Road, Maharagama",
                    Phone = "+94 75 555 1212",
                    Email = "saman.silva@outlook.com"
                },
                new Customer
                {
                    AccountNumber = "NWSDB-1004",
                    Name = "Dilani Jayawardena",
                    Address = "12, Negombo Road, Ja-Ela",
                    Phone = "+94 72 333 4455",
                    Email = "dilani.j@gmail.com"
                },
                new Customer
                {
                    AccountNumber = "NWSDB-1005",
                    Name = "Ruwan Gunasekara",
                    Address = "204, Galle Road, Dehiwala",
                    Phone = "+94 76 888 9900",
                    Email = "ruwan.g@gmail.com"
                }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();

            // 2. Seed Bills with current unpaid bills for ALL accounts
            var bills = new Bill[]
            {
                // Customer 1: NWSDB-1001 (Kavindu Perera) -> Rs. 3,680.00
                new Bill
                {
                    CustomerId = customers[0].Id,
                    BillNumber = "INV-2026-0801",
                    BillingMonth = "August 2026",
                    UnitsUsed = 38,
                    BillAmount = 3680.00m,
                    PaidAmount = 0.00m,
                    DueDate = DateTime.UtcNow.AddDays(15),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Bill
                {
                    CustomerId = customers[0].Id,
                    BillNumber = "INV-2026-0701",
                    BillingMonth = "July 2026",
                    UnitsUsed = 40,
                    BillAmount = 3850.00m,
                    PaidAmount = 3850.00m,
                    DueDate = DateTime.UtcNow.AddDays(-15),
                    Status = "Paid",
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                },

                // Customer 2: NWSDB-1002 (Nimali Fernando) -> Rs. 3,170.00
                new Bill
                {
                    CustomerId = customers[1].Id,
                    BillNumber = "INV-2026-0802",
                    BillingMonth = "August 2026",
                    UnitsUsed = 32,
                    BillAmount = 3170.00m,
                    PaidAmount = 0.00m,
                    DueDate = DateTime.UtcNow.AddDays(15),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Bill
                {
                    CustomerId = customers[1].Id,
                    BillNumber = "INV-2026-0702",
                    BillingMonth = "July 2026",
                    UnitsUsed = 29,
                    BillAmount = 2915.00m,
                    PaidAmount = 2915.00m,
                    DueDate = DateTime.UtcNow.AddDays(-15),
                    Status = "Paid",
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                },

                // Customer 3: NWSDB-1003 (Saman Silva) -> Rs. 375.00
                new Bill
                {
                    CustomerId = customers[2].Id,
                    BillNumber = "INV-2026-0803",
                    BillingMonth = "August 2026",
                    UnitsUsed = 5,
                    BillAmount = 375.00m,
                    PaidAmount = 0.00m,
                    DueDate = DateTime.UtcNow.AddDays(10),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },

                // Customer 4: NWSDB-1004 (Dilani Jayawardena) -> Rs. 1,275.00
                new Bill
                {
                    CustomerId = customers[3].Id,
                    BillNumber = "INV-2026-0804",
                    BillingMonth = "August 2026",
                    UnitsUsed = 15,
                    BillAmount = 1275.00m,
                    PaidAmount = 0.00m,
                    DueDate = DateTime.UtcNow.AddDays(20),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },

                // Customer 5: NWSDB-1005 (Ruwan Gunasekara) -> Rs. 2,150.00
                new Bill
                {
                    CustomerId = customers[4].Id,
                    BillNumber = "INV-2026-0805",
                    BillingMonth = "August 2026",
                    UnitsUsed = 22,
                    BillAmount = 2150.00m,
                    PaidAmount = 0.00m,
                    DueDate = DateTime.UtcNow.AddDays(12),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Bill
                {
                    CustomerId = customers[4].Id,
                    BillNumber = "INV-2026-0705",
                    BillingMonth = "July 2026",
                    UnitsUsed = 20,
                    BillAmount = 2060.00m,
                    PaidAmount = 2060.00m,
                    DueDate = DateTime.UtcNow.AddDays(-20),
                    Status = "Paid",
                    CreatedAt = DateTime.UtcNow.AddDays(-50)
                }
            };

            context.Bills.AddRange(bills);
            context.SaveChanges();

            // 3. Seed Historical Payments for July bills (August test bills remain unpaid)
            var payments = new Payment[]
            {
                new Payment
                {
                    CustomerId = customers[0].Id,
                    BillId = bills[1].Id, // INV-2026-0701
                    ReceiptNumber = "REC-2026-0701",
                    Amount = 3850.00m,
                    PaymentMethod = "Card",
                    TransactionId = "TXN-DEMO771029",
                    PaymentDate = DateTime.UtcNow.AddDays(-20),
                    Status = "Success"
                },
                new Payment
                {
                    CustomerId = customers[1].Id,
                    BillId = bills[3].Id, // INV-2026-0702
                    ReceiptNumber = "REC-2026-0702",
                    Amount = 2915.00m,
                    PaymentMethod = "Card",
                    TransactionId = "TXN-DEMO882190",
                    PaymentDate = DateTime.UtcNow.AddDays(-18),
                    Status = "Success"
                },
                new Payment
                {
                    CustomerId = customers[4].Id,
                    BillId = bills[5].Id, // INV-2026-0705
                    ReceiptNumber = "REC-2026-0705",
                    Amount = 2060.00m,
                    PaymentMethod = "Card",
                    TransactionId = "TXN-DEMO990112",
                    PaymentDate = DateTime.UtcNow.AddDays(-22),
                    Status = "Success"
                }
            };

            context.Payments.AddRange(payments);
            context.SaveChanges();
        }

        /// <summary>
        /// Resets the current test cycle bills to Pending with remaining balance equal to AmountDue.
        /// Removes previous test payments/collections for current August bills while preserving customer accounts and historical records.
        /// </summary>
        public static void ResetTestData(NwsdbDbContext context)
        {
            var augustBillNumbers = new[]
            {
                "INV-2026-0801",
                "INV-2026-0802",
                "INV-2026-0803",
                "INV-2026-0804",
                "INV-2026-0805"
            };

            var augustBills = context.Bills
                .Where(b => augustBillNumbers.Contains(b.BillNumber))
                .ToList();

            var augustBillIds = augustBills.Select(b => b.Id).ToList();

            // 1. Remove payments for the current August bills
            var augustPayments = context.Payments
                .Where(p => augustBillIds.Contains(p.BillId))
                .ToList();
            if (augustPayments.Any())
            {
                context.Payments.RemoveRange(augustPayments);
            }

            // 2. Remove third party collections for the current August bills
            var augustCollections = context.ThirdPartyCollections
                .Where(c => augustBillIds.Contains(c.BillId))
                .ToList();
            if (augustCollections.Any())
            {
                context.ThirdPartyCollections.RemoveRange(augustCollections);
            }

            // 3. Reset Bill amounts, PaidAmount to 0.00, and Status to Pending
            var targetAmounts = new Dictionary<string, (decimal amount, int units)>
            {
                { "INV-2026-0801", (3680.00m, 38) }, // NWSDB-1001 -> Rs. 3,680.00
                { "INV-2026-0802", (3170.00m, 32) }, // NWSDB-1002 -> Rs. 3,170.00
                { "INV-2026-0803", (375.00m, 5) },   // NWSDB-1003 -> Rs. 375.00
                { "INV-2026-0804", (1275.00m, 15) }, // NWSDB-1004 -> Rs. 1,275.00
                { "INV-2026-0805", (2150.00m, 22) }  // NWSDB-1005 -> Rs. 2,150.00
            };

            foreach (var bill in augustBills)
            {
                if (targetAmounts.TryGetValue(bill.BillNumber, out var config))
                {
                    bill.BillAmount = config.amount;
                    bill.UnitsUsed = config.units;
                }
                bill.PaidAmount = 0.00m;
                bill.Status = "Pending";
            }

            // Ensure any missing August bill for seeded customers is created
            var customers = context.Customers.ToList();
            var customerMap = new Dictionary<string, (string billNo, decimal amount, int units)>
            {
                { "NWSDB-1001", ("INV-2026-0801", 3680.00m, 38) },
                { "NWSDB-1002", ("INV-2026-0802", 3170.00m, 32) },
                { "NWSDB-1003", ("INV-2026-0803", 375.00m, 5) },
                { "NWSDB-1004", ("INV-2026-0804", 1275.00m, 15) },
                { "NWSDB-1005", ("INV-2026-0805", 2150.00m, 22) }
            };

            foreach (var cust in customers)
            {
                if (customerMap.TryGetValue(cust.AccountNumber, out var mapping))
                {
                    var existing = augustBills.FirstOrDefault(b => b.CustomerId == cust.Id && b.BillNumber == mapping.billNo);
                    if (existing == null)
                    {
                        var newBill = new Bill
                        {
                            CustomerId = cust.Id,
                            BillNumber = mapping.billNo,
                            BillingMonth = "August 2026",
                            UnitsUsed = mapping.units,
                            BillAmount = mapping.amount,
                            PaidAmount = 0.00m,
                            DueDate = DateTime.UtcNow.AddDays(15),
                            Status = "Pending",
                            CreatedAt = DateTime.UtcNow.AddDays(-10)
                        };
                        context.Bills.Add(newBill);
                    }
                }
            }

            context.SaveChanges();
        }
    }
}
