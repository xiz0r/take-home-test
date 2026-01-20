using FluentAssertions;
using Fundo.Application.UseCases.Payment;
using Fundo.Domain.Entities;
using Fundo.Domain.Repositories;
using Fundo.Services.Tests.Shared.ObjectMothers;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases;

public class LoanPaymentMakerTests
{
    private readonly Mock<ILoanRepository> _loanRepository;
    private readonly LoanPaymentMaker _sut;

    public LoanPaymentMakerTests()
    {
        _loanRepository = new Mock<ILoanRepository>();
        _loanRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new LoanPaymentMaker(_loanRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoanMissing_ThrowsKeyNotFoundException()
    {
        // Arrange
        _loanRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        // Act
        var action = () => _sut.ExecuteAsync(
            Guid.NewGuid(),
            LoanRequestMother.MakePaymentRequest(50m),
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>();

        _loanRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentIsValid_UpdatesLoanAndReturnsResponse()
    {
        // Arrange
        var loan = LoanMother.ActiveLoan(200m, "Andrea");
        SetupLoanExists(loan);

        // Act
        var response = await _sut.ExecuteAsync(
            loan.Id,
            LoanRequestMother.MakePaymentRequest(50m),
            CancellationToken.None);

        // Assert
        response.Id.Should().Be(loan.Id);
        response.CurrentBalance.Should().Be(150m);
        response.Status.Should().Be("active");
        response.UpdatedAt.Should().NotBeNull();

        _loanRepository.Verify(
            x => x.UpdateAsync(loan, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentClearsBalance_UpdatesLoanAsPaid()
    {
        // Arrange
        var loan = LoanMother.ActiveLoan(75m, "Sofia");
        SetupLoanExists(loan);

        // Act
        var response = await _sut.ExecuteAsync(
            loan.Id,
            LoanRequestMother.MakePaymentRequest(75m),
            CancellationToken.None);

        // Assert
        response.CurrentBalance.Should().Be(0m);
        response.Status.Should().Be("paid");
        response.UpdatedAt.Should().NotBeNull();

        _loanRepository.Verify(
            x => x.UpdateAsync(loan, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task ExecuteAsync_WhenPaymentAmountIsInvalid_ThrowsArgumentException(decimal amount)
    {
        // Arrange
        var loan = LoanMother.ActiveLoan(100m, "Diego");
        SetupLoanExists(loan);

        // Act
        var action = () => _sut.ExecuteAsync(
            loan.Id,
            LoanRequestMother.MakePaymentRequest(amount),
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();

        _loanRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentExceedsBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var loan = LoanMother.ActiveLoan(100m, "Diego");
        SetupLoanExists(loan);

        // Act
        var action = () => _sut.ExecuteAsync(
            loan.Id,
            LoanRequestMother.MakePaymentRequest(200m),
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();

        _loanRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoanIsAlreadyPaid_ThrowsInvalidOperationException()
    {
        // Arrange
        var loan = LoanMother.PaidLoan(100m, "Mara");
        SetupLoanExists(loan);

        // Act
        var action = () => _sut.ExecuteAsync(
            loan.Id,
            LoanRequestMother.MakePaymentRequest(10m),
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();

        _loanRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupLoanExists(Loan loan)
    {
        _loanRepository
            .Setup(x => x.GetByIdAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);
    }
}
