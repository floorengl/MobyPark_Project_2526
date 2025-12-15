using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MobyPark_api.Enums;


public class PaymentServiceTests
{
    private static AddPaymentDto PaymentAddDto(string? hash = null) => new AddPaymentDto
    {
        Amount = 10m,
        CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        Status = PaymentStatus.Complete,
        Hash = hash,
        Transaction = new TransactionDataDto
        {
            Amount = 10m,
            Date = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Method = "Card",
            Issuer = "VISA",
            Bank = "ING"
        }
    };

    private static UpdatePaymentDto PaymentUpdateDto() => new UpdatePaymentDto
    {
        Status = PaymentStatus.Complete,
        Transaction = new TransactionDataDto
        {
            Amount = 999m,
            Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Method = "Cash",
            Issuer = "NEW",
            Bank = "NEWBANK"
        }
    };

    private static Payment MakePayment(Guid id, decimal amount = 10m)
    {
        var txId = Guid.NewGuid();
        return new Payment
        {
            Id = id,
            Amount = amount,
            CreatedAt = DateTime.UtcNow,
            Status = PaymentStatus.Complete,
            Hash = "originalhash",
            TransactionId = txId,
            TransactionData = new TransactionData
            {
                TransactionId = txId,
                Amount = amount,
                Date = DateTime.UtcNow,
                Method = "Card",
                Issuer = "VISA",
                Bank = "ING"
            }
        };
    }


    [Fact]
    public async Task GetPaymentsBetweenAsync_ReturnsRepositoryResult()
    {
        // Arrange.
        var repo = new Mock<IPaymentRepository>();
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 1, 2);
        var expected = new List<Payment>
        {
            MakePayment(Guid.NewGuid(), 1m),
            MakePayment(Guid.NewGuid(), 2m)
        };
        repo.Setup(r => r.GetBetweenAsync(start, end, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var sut = new PaymentService(repo.Object);
        // Act.
        var actual = await sut.GetPaymentsBetweenAsync(start, end);
        // Assert.
        Assert.Same(expected, actual);
        repo.Verify(r => r.GetBetweenAsync(start, end, It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task AddRefundAsync_WhenOriginalNotFound_Throws()
    {
        // Arrange.
        var repo = new Mock<IPaymentRepository>();
        repo.Setup(r => r.GetByIdPaymentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);
        var sut = new PaymentService(repo.Object);
        // Act / Assert.
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.AddRefundAsync(new RefundPaymentDto
            {
                OriginalPaymentId = Guid.NewGuid(),
                Status = PaymentStatus.Complete
            }));
    }


    [Fact]
    public async Task GetPaymentByIdAsync_ReturnsRepositoryResult()
    {
        // Arrange.
        var repo = new Mock<IPaymentRepository>();
        var id = Guid.NewGuid();
        var expected = MakePayment(id, 10m);
        repo.Setup(r => r.GetByIdPaymentAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var sut = new PaymentService(repo.Object);
        // Act.
        var actual = await sut.GetPaymentByIdAsync(id);
        // Assert.
        Assert.Same(expected, actual);
        repo.Verify(r => r.GetByIdPaymentAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task UpdatePaymentAsync_WhenPaymentNotFound_ReturnsNull()
    {
        // Arrange.
        var repo = new Mock<IPaymentRepository>();
        repo.Setup(r => r.GetByIdPaymentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var sut = new PaymentService(repo.Object);
        // Act.
        var result = await sut.UpdatePaymentAsync(Guid.NewGuid(), PaymentUpdateDto());
        // Assert.
        Assert.Null(result);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task UpdatePaymentAsync_SavesChanges()
    {
        // Arrange.
        var repo = new Mock<IPaymentRepository>();
        var id = Guid.NewGuid();
        var entity = MakePayment(id, 10m);
        repo.Setup(r => r.GetByIdPaymentAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var sut = new PaymentService(repo.Object);
        var dto = PaymentUpdateDto();
        dto.Status = PaymentStatus.Complete;
        // Act.
        var result = await sut.UpdatePaymentAsync(id, dto);
        // Assert.
        Assert.NotNull(result);
        Assert.Equal(dto.Status, entity.Status);
        Assert.Equal(dto.Transaction.Amount, entity.TransactionData.Amount);
        Assert.Equal(dto.Transaction.Date, entity.TransactionData.Date);
        Assert.Equal(dto.Transaction.Method, entity.TransactionData.Method);
        Assert.Equal(dto.Transaction.Issuer, entity.TransactionData.Issuer);
        Assert.Equal(dto.Transaction.Bank, entity.TransactionData.Bank);
        repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task GetPaymentsAsync_ReturnsRepositoryResult()
    {
        // Arrange.
        var repo = new Mock<IPaymentRepository>();
        var expected = new List<Payment>
        {
            MakePayment(Guid.NewGuid(), 1m),
            MakePayment(Guid.NewGuid(), 2m)
        };
        repo.Setup(r => r.GetAllPaymentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var sut = new PaymentService(repo.Object);
        // Act.
        var actual = await sut.GetPaymentsAsync();
        // Assert.
        Assert.Same(expected, actual);
        repo.Verify(r => r.GetAllPaymentsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
