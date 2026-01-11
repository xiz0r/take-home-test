using Fundo.Application.DTOs;
using Fundo.Application.UseCases.Create;
using Fundo.Application.UseCases.Find;
using Fundo.Application.UseCases.Payment;
using Microsoft.AspNetCore.Mvc;

namespace Fundo.Applications.WebApi.Controllers;

/// <summary>
/// Controller for managing loans and payments
/// </summary>
[ApiController]
[Route("loans")]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly LoanCreator loanCreator;
    private readonly LoanFinder loanFinder;
    private readonly LoanListFinder loanListFinder;
    private readonly LoanPaymentMaker loanPaymentMaker;

    public LoansController(
        LoanCreator loanCreator,
        LoanFinder loanFinder,
        LoanListFinder loanListFinder,
        LoanPaymentMaker loanPaymentMaker)
    {
        this.loanCreator = loanCreator;
        this.loanFinder = loanFinder;
        this.loanListFinder = loanListFinder;
        this.loanPaymentMaker = loanPaymentMaker;
    }

    /// <summary>
    /// Creates a new loan
    /// </summary>
    /// <param name="request">Loan creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created loan</returns>
    /// <response code="201">Returns the newly created loan</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanResponse>> CreateLoan(
        [FromBody] CreateLoanRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var loan = await this.loanCreator.ExecuteAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetLoanById), new { id = loan.Id }, loan);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets a loan by its ID
    /// </summary>
    /// <param name="id">The loan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The loan details</returns>
    /// <response code="200">Returns the loan</response>
    /// <response code="404">If the loan is not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> GetLoanById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var loan = await this.loanFinder.ExecuteAsync(id, cancellationToken);
        
        if (loan is null)
            return NotFound(new { error = $"Loan with id {id} not found" });

        return Ok(loan);
    }

    /// <summary>
    /// Gets all loans
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all loans</returns>
    /// <response code="200">Returns the list of loans</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LoanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LoanResponse>>> GetAllLoans(
        CancellationToken cancellationToken)
    {
        var loans = await this.loanListFinder.ExecuteAsync(cancellationToken);
        return Ok(loans);
    }

    /// <summary>
    /// Makes a payment on a loan
    /// </summary>
    /// <param name="id">The loan ID</param>
    /// <param name="request">Payment details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated loan</returns>
    /// <response code="200">Returns the updated loan</response>
    /// <response code="400">If the payment is invalid</response>
    /// <response code="404">If the loan is not found</response>
    [HttpPost("{id:guid}/payment")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> MakePayment(
        Guid id,
        [FromBody] MakePaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var loan = await this.loanPaymentMaker.ExecuteAsync(id, request, cancellationToken);
            return Ok(loan);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
