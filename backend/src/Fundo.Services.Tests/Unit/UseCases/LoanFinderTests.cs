using FluentAssertions;
using Fundo.Application.UseCases.Find;
using Fundo.Domain.Entities;
using Fundo.Domain.Repositories;
using Fundo.Services.Tests.Shared.ObjectMothers;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases;

public class LoanFinderTests
{
    [Fact]
    public async Task ExecuteAsync_WhenLoanIsMissing_ReturnsNull()
    {
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        var useCase = new LoanFinder(loanRepository.Object);

        var response = await useCase.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        response.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoanExists_ReturnsMappedResponse()
    {
        var loan = LoanMother.ActiveLoan(500m, "Carlos");
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repo => repo.GetByIdAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var useCase = new LoanFinder(loanRepository.Object);

        var response = await useCase.ExecuteAsync(loan.Id, CancellationToken.None);

        response.Should().NotBeNull();
        response!.Id.Should().Be(loan.Id);
        response.Amount.Should().Be(loan.Amount);
        response.CurrentBalance.Should().Be(loan.CurrentBalance);
        response.ApplicantName.Should().Be(loan.ApplicantName);
        response.Status.Should().Be("active");
        response.CreatedAt.Should().Be(loan.CreatedAt);
        response.UpdatedAt.Should().Be(loan.UpdatedAt);
    }
}
