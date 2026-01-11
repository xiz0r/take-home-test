using FluentAssertions;
using Fundo.Application.UseCases.Create;
using Fundo.Domain.Entities;
using Fundo.Domain.Repositories;
using Fundo.Services.Tests.Shared.ObjectMothers;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases;

public class LoanCreatorTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCreateLoanAndReturnResponse()
    {
        var request = LoanRequestMother.CreateLoanRequest(250m, "Maria");
        var loanRepository = new Mock<ILoanRepository>();
        Loan? createdLoan = null;

        loanRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()))
            .Callback<Loan, CancellationToken>((loan, _) => createdLoan = loan)
            .ReturnsAsync((Loan loan, CancellationToken _) => loan);

        var useCase = new LoanCreator(loanRepository.Object);

        var response = await useCase.ExecuteAsync(request, CancellationToken.None);

        createdLoan.Should().NotBeNull();
        response.Id.Should().Be(createdLoan!.Id);
        response.Amount.Should().Be(request.Amount);
        response.CurrentBalance.Should().Be(createdLoan.CurrentBalance);
        response.ApplicantName.Should().Be(request.ApplicantName);
        response.Status.Should().Be("active");
        response.CreatedAt.Should().Be(createdLoan.CreatedAt);
        response.UpdatedAt.Should().Be(createdLoan.UpdatedAt);

        loanRepository.Verify(
            repo => repo.AddAsync(createdLoan, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
