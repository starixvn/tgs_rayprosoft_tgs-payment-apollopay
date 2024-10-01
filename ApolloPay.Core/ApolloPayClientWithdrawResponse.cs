namespace Tgs.Payment.ApolloPay;
public record ApolloPayClientWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo,
    decimal Amount);