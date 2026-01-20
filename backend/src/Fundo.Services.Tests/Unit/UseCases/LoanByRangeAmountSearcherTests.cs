using Fundo.Domain.Repositories;
using Fundo.Application.UseCases.Search;
using Moq;
using Xunit;
using FluentAssertions;
using Fundo.Domain.Entities;
using Fundo.Services.Tests.Shared.ObjectMothers;

namespace Fundo.Services.Tests.Unit.UseCases;

public class LoanByRangeSearcherTests
{
  private readonly Mock<ILoanRepository> _loanRepository;
  private readonly LoanByRangeAmountSearcher _sut;

  public LoanByRangeSearcherTests()
  {
    _loanRepository = new Mock<ILoanRepository>();
    _sut = new LoanByRangeAmountSearcher(_loanRepository.Object);
  }

  [Fact]
  public async Task ExecuteAsync_WhenLoansExist_ReturnsMappedResponses()
  {
    // Arrange
    const decimal minAmount = 100m;
    const decimal maxAmount = 500m;

    var loans = new List<Loan>
        {
            LoanMother.ActiveLoan(150m, "Ana"),
            LoanMother.ActiveLoan(300m, "Bruno"),
            LoanMother.ActiveLoan(450m, "Carla")
        };

    _loanRepository
        .Setup(x => x.GetByAmountRangeAsync(minAmount, maxAmount, It.IsAny<CancellationToken>()))
        .ReturnsAsync(loans);

    var request = LoanRequestMother.CreateSearchByAmountRangeRequest(minAmount, maxAmount);

    // Act
    var response = (await _sut.ExecuteAsync(request, CancellationToken.None)).ToList();

    // Assert
    response.Should().HaveCount(3);
    response.Should().AllSatisfy(item => item.Status.Should().Be("active"));
    response.Select(x => x.Id).Should().BeEquivalentTo(loans.Select(x => x.Id));
    response.Select(x => x.Amount).Should().BeEquivalentTo(loans.Select(x => x.Amount));
    response.Select(x => x.ApplicantName).Should().BeEquivalentTo(loans.Select(x => x.ApplicantName));

    _loanRepository.Verify(
        x => x.GetByAmountRangeAsync(minAmount, maxAmount, It.IsAny<CancellationToken>()),
        Times.Once);
  }

  [Fact]
  public async Task ExecuteAsync_WhenNoLoansInRange_ReturnsEmptyList()
  {
    // Arrange
    _loanRepository
        .Setup(x => x.GetByAmountRangeAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<Loan>());

    var request = LoanRequestMother.CreateSearchByAmountRangeRequest(100m, 200m);

    // Act
    var response = await _sut.ExecuteAsync(request, CancellationToken.None);

    // Assert
    response.Should().BeEmpty();
  }

  [Fact]
  public async Task ExecuteAsync_WhenSingleLoanInRange_ReturnsSingleMappedResponse()
  {
    // Arrange
    var loan = LoanMother.ActiveLoan(500m, "Diana");
    _loanRepository
        .Setup(x => x.GetByAmountRangeAsync(400m, 600m, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<Loan> { loan });

    var request = LoanRequestMother.CreateSearchByAmountRangeRequest(400m, 600m);

    // Act
    var response = (await _sut.ExecuteAsync(request, CancellationToken.None)).ToList();

    // Assert
    response.Should().ContainSingle();
    response.First().Id.Should().Be(loan.Id);
    response.First().Amount.Should().Be(loan.Amount);
    response.First().ApplicantName.Should().Be(loan.ApplicantName);
  }
}