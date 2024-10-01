namespace Tgs.Payment.ApolloPay.Client;
public record ApolloPayWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo);
