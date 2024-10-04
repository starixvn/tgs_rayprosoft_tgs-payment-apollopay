using System.ComponentModel;

namespace Tgs.Payment.ApolloPay.Client;
public record ApolloPayDepositSettings(
    string MerchantNo,
    string ApiKey,
    Guid FormId,
    string ChannelType,
    string? BankCode,
    [property: Description("匯率轉換")]
    decimal ExchangeRate);