namespace Tgs.Payment.ApolloPay;
public record WithdrawOrder(
    Guid FormId,
    decimal Amount,
    Uri CallbackUrl,
    string ChannelType,
    string BankCode,
    string BankCardNo,
    string AccountName);