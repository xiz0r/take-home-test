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
    private readonly Mock<ILoanRepository> _loanRepository;
    private readonly LoanFinder _sut;

    public LoanFinderTests()
    {
        _loanRepository = new Mock<ILoanRepository>();
        _sut = new LoanFinder(_loanRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoanIsMissing_ReturnsNull()
    {
        // Arrange
        _loanRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        // Act
        var response = await _sut.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoanExists_ReturnsMappedResponse()
    {
        // Arrange
        var loan = LoanMother.ActiveLoan(500m, "Carlos");
        _loanRepository
            .Setup(x => x.GetByIdAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        // Act
        var response = await _sut.ExecuteAsync(loan.Id, CancellationToken.None);

        // Assert
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
