using Fundo.Application.DTOs;
using Fundo.Domain.Entities;

namespace Fundo.Application.UseCases.Shared;

internal static class LoanResponseMapper
{
    public static LoanResponse Map(Loan loan)
    {
        return new LoanResponse(
            loan.Id,
            loan.Amount,
            loan.CurrentBalance,
            loan.ApplicantName,
            loan.Status.ToString().ToLower(),
            loan.CreatedAt,
            loan.UpdatedAt
        );
    }
}
