using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MobyPark_api.Enums;

namespace MobyPark_api.tests
{
    public class TestPaymentRepository : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public TestPaymentRepository(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        private static Payment MakePayment(DateTime createdAt, decimal amount)
        {
            var txId = Guid.NewGuid();
            return new Payment
            {
                Id = Guid.NewGuid(),
                Amount = amount,
                CreatedAt = createdAt,
                Status = PaymentStatus.Complete,
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
        public async Task GetByIdTransactionAsync_ShouldIncludeTransaction()
        {
            // Arrange.
            await _fixture.ResetDB();
            using var db = _fixture.CreateContext();
            var repo = new PaymentRepository(db);
            var p = MakePayment(DateTime.UtcNow, 10m);
            db.Payments.Add(p);
            await db.SaveChangesAsync();
            // Act.
            var found = await repo.GetByIdPaymentAsync(p.Id);
            // Assert.
            Assert.NotNull(found);
            Assert.NotNull(found!.TransactionData);
            Assert.Equal(p.TransactionId, found.TransactionData.TransactionId);
        }

        [Fact]
        public async Task GetBetweenAsync_ShouldReturnOnlyInRange()
        {
            // Arrange.
            await _fixture.ResetDB();
            using var db = _fixture.CreateContext();
            var repo = new PaymentRepository(db);

            var t1 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var t2 = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            var t3 = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);
            db.Payments.AddRange(
                MakePayment(t1, 1m),
                MakePayment(t2, 2m),
                MakePayment(t3, 3m)
            );
            await db.SaveChangesAsync();
            // Act.
            var list = await repo.GetBetweenAsync(t1, t2);
            // Assert.
            Assert.Equal(2, list.Count);
            Assert.DoesNotContain(list, x => x.CreatedAt == t3);
            Assert.All(list, x => Assert.NotNull(x.TransactionData));
        }

        [Fact]
        public async Task GetAllPaymentsAsync_ShouldBeOrderedDescByCreatedAt()
        {
            // Arrange.
            await _fixture.ResetDB();
            using var db = _fixture.CreateContext();
            var repo = new PaymentRepository(db);
            var older = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var newer = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            db.Payments.AddRange(
                MakePayment(older, 1m),
                MakePayment(newer, 2m)
            );
            await db.SaveChangesAsync();
            // Act.
            var list = await repo.GetAllPaymentsAsync();
            // Assert.
            Assert.Equal(2, list.Count);
            Assert.Equal(newer, list[0].CreatedAt);
            Assert.Equal(older, list[1].CreatedAt);
            Assert.All(list, x => Assert.NotNull(x.TransactionData));
        }
    }
}
