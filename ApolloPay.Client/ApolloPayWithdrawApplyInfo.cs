namespace Tgs.Payment.ApolloPay.Client;
public record ApolloPayWithdrawApplyInfo(
    ApolloPayWithdrawSettings Settings,
    ApolloPayWithdrawResponse Response);