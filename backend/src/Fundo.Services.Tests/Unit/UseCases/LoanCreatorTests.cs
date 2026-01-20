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
    private readonly Mock<ILoanRepository> _loanRepository;
    private readonly LoanCreator _sut;

    public LoanCreatorTests()
    {
        _loanRepository = new Mock<ILoanRepository>();
        _loanRepository
            .Setup(x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan loan, CancellationToken _) => loan);

        _sut = new LoanCreator(_loanRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesLoanSuccessfully()
    {
        // Arrange
        var request = LoanRequestMother.CreateLoanRequest(250m, "Maria");

        // Act
        var response = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Amount.Should().Be(request.Amount);
        response.CurrentBalance.Should().Be(request.Amount);
        response.ApplicantName.Should().Be(request.ApplicantName);
        response.Status.Should().Be("active");

        _loanRepository.Verify(
            x => x.AddAsync(
                It.Is<Loan>(l =>
                    l.ApplicantName == request.ApplicantName &&
                    l.CurrentBalance == request.Amount),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task ExecuteAsync_WithInvalidAmount_ThrowsArgumentException(decimal amount)
    {
        // Arrange
        var request = LoanRequestMother.CreateLoanRequest(amount, "Maria");

        // Act
        var action = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();

        _loanRepository.Verify(
            x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithInvalidApplicantName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var request = LoanRequestMother.CreateLoanRequest(250m, name!);

        // Act
        var action = () => _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();

        _loanRepository.Verify(
            x => x.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
