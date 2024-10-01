namespace Tgs.Payment.ApolloPay;
public record DepositOrder(
    decimal Amount,
    Guid FormId,
    Uri CallbackUrl,
    string ChannelType,
    string? BankCode);