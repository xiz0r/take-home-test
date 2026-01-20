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
    private readonly Mock<ILoanRepository> _loanRepository;
    private readonly LoanListFinder _sut;

    public LoanListFinderTests()
    {
        _loanRepository = new Mock<ILoanRepository>();
        _sut = new LoanListFinder(_loanRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoansExist_ReturnsMappedResponses()
    {
        // Arrange
        var loans = new List<Loan>
        {
            LoanMother.ActiveLoan(120m, "Laura"),
            LoanMother.ActiveLoan(320m, "Miguel")
        };
        _loanRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(loans);

        // Act
        var response = (await _sut.ExecuteAsync(CancellationToken.None)).ToList();

        // Assert
        response.Should().HaveCount(2);
        response.Should().AllSatisfy(item => item.Status.Should().Be("active"));
        response.Select(x => x.Id).Should().BeEquivalentTo(loans.Select(x => x.Id));
        response.Select(x => x.Amount).Should().BeEquivalentTo(loans.Select(x => x.Amount));
        response.Select(x => x.ApplicantName).Should().BeEquivalentTo(loans.Select(x => x.ApplicantName));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoLoansExist_ReturnsEmptyList()
    {
        // Arrange
        _loanRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Loan>());

        // Act
        var response = await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSingleLoanExists_ReturnsSingleMappedResponse()
    {
        // Arrange
        var loan = LoanMother.ActiveLoan(500m, "Carlos");
        _loanRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Loan> { loan });

        // Act
        var response = (await _sut.ExecuteAsync(CancellationToken.None)).ToList();

        // Assert
        response.Should().ContainSingle();
        response.First().Id.Should().Be(loan.Id);
        response.First().Amount.Should().Be(loan.Amount);
        response.First().ApplicantName.Should().Be(loan.ApplicantName);
    }
}
