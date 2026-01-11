using FluentAssertions;
using Fundo.Application.UseCases.Find;
using Fundo.Domain.Entities;
using Fundo.Domain.Repositories;
using Fundo.Services.Tests.Shared.ObjectMothers;
using Moq;
using Xunit;

namespace Fundo.Services.Tests.Unit.UseCases;

public class LoanListFinderTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsMappedResponses()
    {
        var loans = new List<Loan>
        {
            LoanMother.ActiveLoan(120m, "Laura"),
            LoanMother.ActiveLoan(320m, "Miguel")
        };
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(loans);

        var useCase = new LoanListFinder(loanRepository.Object);

        var response = (await useCase.ExecuteAsync(CancellationToken.None)).ToList();

        response.Should().HaveCount(2);
        response.Select(item => item.Id).Should().Equal(loans.Select(loan => loan.Id));
        response.Select(item => item.Amount).Should().Equal(loans.Select(loan => loan.Amount));
        response.Select(item => item.ApplicantName).Should().Equal(loans.Select(loan => loan.ApplicantName));
        response.Select(item => item.Status).Should().Equal(loans.Select(_ => "active"));
    }
}
