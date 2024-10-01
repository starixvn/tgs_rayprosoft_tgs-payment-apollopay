namespace Tgs.Payment.ApolloPay.Client;
public record ApolloPayWithdrawReturn
{
    public decimal OrderAmount { get; init; }

    public Guid FormId { get; init; }

    public string Status { get; init; } = default!;

    public string Sign { get; init; } = default!;
}
