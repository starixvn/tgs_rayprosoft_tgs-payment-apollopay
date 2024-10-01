namespace Tgs.Payment.ApolloPay.Client;
public record ApolloPayWithdrawSettings(
    string MerchantNo,
    string ApiKey,
    Guid FormId,
    string ChannelCode,
    string BankCode,
    string BankCardNo,
    string AccountName);