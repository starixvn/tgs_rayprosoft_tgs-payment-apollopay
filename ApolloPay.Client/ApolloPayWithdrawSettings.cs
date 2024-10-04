using System.ComponentModel;

namespace Tgs.Payment.ApolloPay.Client;
public record ApolloPayWithdrawSettings(
    string MerchantNo,
    string ApiKey,
    Guid FormId,
    string ChannelType,
    string BankCode,
    string BankCardNo,
    string AccountName,
    [property: Description("匯率轉換")]
    decimal ExchangeRate);