using Tgs.Payment.ApolloPay.Client;
using Tgs.Payment.Clients;

namespace Tgs.Payment.ApolloPay;
public record DepositForm(
    Guid Id,
    string PlayerCode,
    Guid BrandId,
    Guid BrandPaymentId,
    decimal Amount,
    PaymentFlowState Status,
    ApolloPayDepositApplyInfo? ApplyInfo,
    string? PaymentReturn = null);