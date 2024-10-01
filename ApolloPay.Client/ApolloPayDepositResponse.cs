namespace Tgs.Payment.ApolloPay.Client;
public record ApolloPayDepositResponse(
    decimal Amount,
    Guid FormId,
    Uri RedirectUrl,
    string ThirdPartyOrderNo);