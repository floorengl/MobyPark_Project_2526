using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using MobyPark_api.Enums;

namespace MobyPark_api.tests
{
    public class IntegrationTestPaymentRepository : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public IntegrationTestPaymentRepository(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task SeedAsync(AppDbContext db, Payment p)
        {
            db.Payments.Add(p);
            await db.SaveChangesAsync();
        }

        private static Payment MakePayment(DateTime createdAt, decimal amount)
        {
            var txId = Guid.NewGuid();
            return new Payment
            {
                Id = Guid.NewGuid(),
                Amount = amount,
                CreatedAt = createdAt,
                //Status = PaymentStatus.Completed,
                Hash = Guid.NewGuid().ToString("N"),
                TransactionId = txId,
                TransactionData = new TransactionData
                {
                    TransactionId = txId,
                    Amount = amount,
                    Date = createdAt,
                    Method = "Card",
                    Issuer = "VISA",
                    Bank = "ING"
                }
            };
        }

        [Fact]
        public async Task GetByIdWithTransactionAsync_ShouldIncludeTransaction()
        {
            await _fixture.ResetDB();
            using var db = _fixture.CreateContext();

            var repo = new PaymentRepository(db);

            var p = MakePayment(DateTime.UtcNow, 10m);
            await SeedAsync(db, p);

            var found = await repo.GetByIdWithTransactionAsync(p.Id);

            Assert.NotNull(found);
            Assert.NotNull(found!.TransactionData);
            Assert.Equal(p.TransactionId, found.TransactionData.TransactionId);
        }

        [Fact]
        public async Task GetBetweenAsync_ShouldReturnOnlyInRange()
        {
            await _fixture.ResetDB();
            using var db = _fixture.CreateContext();
            var repo = new PaymentRepository(db);

            var t1 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var t2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            var t3 = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);

            await SeedAsync(db, MakePayment(t1, 1m));
            await SeedAsync(db, MakePayment(t2, 2m));
            await SeedAsync(db, MakePayment(t3, 3m));

            var list = await repo.GetBetweenAsync(t1, t2);

            Assert.Equal(2, list.Count);
            Assert.DoesNotContain(list, x => x.CreatedAt == t3);

            // also ensures Include worked
            Assert.All(list, x => Assert.NotNull(x.TransactionData));
        }

        [Fact]
        public async Task GetAllWithTransactionAsync_ShouldBeOrderedDescByCreatedAt()
        {
            await _fixture.ResetDB();
            using var db = _fixture.CreateContext();
            var repo = new PaymentRepository(db);

            var older = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var newer = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);

            await SeedAsync(db, MakePayment(older, 1m));
            await SeedAsync(db, MakePayment(newer, 2m));

            var list = await repo.GetAllWithTransactionAsync();

            Assert.Equal(2, list.Count);
            Assert.Equal(newer, list[0].CreatedAt);
            Assert.Equal(older, list[1].CreatedAt);
            Assert.All(list, x => Assert.NotNull(x.TransactionData));
        }
    }
}
