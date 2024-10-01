namespace Tgs.Payment.ApolloPay.Client;
public record ApolloPayDepositSettings(
    string MerchantNo,
    string ApiKey,
    Guid FormId,
    string ChannelType,
    string? BankCode);
