using Tgs.Payment.ApolloPay.Client;
using Tgs.Payment.Clients;

namespace Tgs.Payment.ApolloPay;
public record WithdrawForm(
      Guid Id,
      string? UserId,
      UserType UserType,
      Guid BrandId,
      Guid? BrandPaymentId,
      decimal Amount,
      PaymentFlowState Status,
      Guid BankInfoId,
      string? BankInfo,
      ApolloPayWithdrawApplyInfo? ApplyInfo,
      string? PaymentReturn = null);