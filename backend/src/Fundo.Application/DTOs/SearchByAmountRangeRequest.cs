namespace Fundo.Application.DTOs;

public record SearchByAmountRangeRequest(decimal MinAmount, decimal MaxAmount);