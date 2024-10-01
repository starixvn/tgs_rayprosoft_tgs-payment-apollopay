namespace Tgs.Payment.ApolloPay;
public record ApolloPayClientDepositResponse(
    decimal Amount,
    Guid FormId,
    string ThirdPartyOrderNo,
    Uri RedirectUrl);