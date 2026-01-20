using FluentAssertions;
using Fundo.Application.UseCases.Payment;
using Fundo.Domain.Entities;
using Fundo.Domain.Enums;
using Fundo.Domain.Repositories;
using Fundo.Services.Tests.Shared.ObjectMothers;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases;

public class LoanPaymentMakerTests
{
    [Fact]
    public async Task ExecuteAsync_WhenLoanMissing_Throws()
    {
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        var useCase = new LoanPaymentMaker(loanRepository.Object);

        var action = () => useCase.ExecuteAsync(
            Guid.NewGuid(),
            LoanRequestMother.MakePaymentRequest(50m),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
        loanRepository.Verify(
            repo => repo.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentIsValid_UpdatesLoanAndReturnsResponse()
    {
        var loan = LoanMother.ActiveLoan(200m, "Andrea");
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repo => repo.GetByIdAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);
        loanRepository
            .Setup(repo => repo.UpdateAsync(loan, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new LoanPaymentMaker(loanRepository.Object);

        var response = await useCase.ExecuteAsync(
            loan.Id,
            LoanRequestMother.MakePaymentRequest(50m),
            CancellationToken.None);

        response.Id.Should().Be(loan.Id);
        response.CurrentBalance.Should().Be(150m);
        response.Status.Should().Be("active");
        response.UpdatedAt.Should().NotBeNull();

        loanRepository.Verify(
            repo => repo.UpdateAsync(loan, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentClearsBalance_UpdatesLoanAsPaid()
    {
        var loan = LoanMother.ActiveLoan(75m, "Sofia");
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repo => repo.GetByIdAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);
        loanRepository
            .Setup(repo => repo.UpdateAsync(loan, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new LoanPaymentMaker(loanRepository.Object);

        var response = await useCase.ExecuteAsync(
            loan.Id,
            LoanRequestMother.MakePaymentRequest(75m),
            CancellationToken.None);

        response.CurrentBalance.Should().Be(0m);
        response.Status.Should().Be("paid");
        response.UpdatedAt.Should().NotBeNull();

        loanRepository.Verify(
            repo => repo.UpdateAsync(loan, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentAmountIsZero_Throws()
    {
        var loan = LoanMother.ActiveLoan(100m, "Diego");
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repo => repo.GetByIdAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var useCase = new LoanPaymentMaker(loanRepository.Object);

        var action = () => useCase.ExecuteAsync(
            loan.Id,
            LoanRequestMother.MakePaymentRequest(0m),
            CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
        loanRepository.Verify(
            repo => repo.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPaymentExceedsBalance_Throws()
    {
        var loan = LoanMother.ActiveLoan(100m, "Diego");
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repo => repo.GetByIdAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var useCase = new LoanPaymentMaker(loanRepository.Object);

        var action = () => useCase.ExecuteAsync(
            loan.Id,
            LoanRequestMother.MakePaymentRequest(200m),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
        loanRepository.Verify(
            repo => repo.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoanIsAlreadyPaid_Throws()
    {
        var loan = LoanMother.ActiveLoan(100m, "Mara");
        SetPrivateProperty(loan, "Status", LoanStatus.Paid);
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repo => repo.GetByIdAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var useCase = new LoanPaymentMaker(loanRepository.Object);

        var action = () => useCase.ExecuteAsync(
            loan.Id,
            LoanRequestMother.MakePaymentRequest(10m),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
        loanRepository.Verify(
            repo => repo.UpdateAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static void SetPrivateProperty<T>(Loan loan, string propertyName, T value)
    {
        var property = typeof(Loan).GetProperty(
            propertyName,
            System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.NonPublic);

        property.Should().NotBeNull();
        property!.SetValue(loan, value);
    }
}
