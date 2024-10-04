using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.ApolloPay.Client;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.ApolloPay.MessageQueue;

internal class WithdrawProcessor(
    ApolloPayClient apolloPayClient,
    IOptions<ApolloPayOptions> optionsAccessor,
    IPaymentClient paymentClient)
    : IMessageProcessor<WithdrawRequest, WithdrawResponse>
{
    private readonly ApolloPayOptions _options = optionsAccessor.Value;

    public async ValueTask<WithdrawResponse> HandleAsync(
        string subject,
        WithdrawRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<ApolloPayWithdrawSettings>(data.ApplyInfo, ApolloPayUtility.SerializerOptions)!;

        var order = new WithdrawOrder(
            settings.FormId,
            decimal.Parse(data.Amount) * settings.ExchangeRate,
            new Uri($"{_options.CallbackBaseUrl}/withdraw/callback"),
            settings.ChannelType,
            settings.BankCode,
            settings.BankCardNo,
            settings.AccountName);

        var withdrawResponse = await apolloPayClient
            .WithdrawAsync(settings.MerchantNo, settings.ApiKey, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new ApolloPayWithdrawApplyInfo(
            settings,
            new ApolloPayWithdrawResponse(withdrawResponse.FormId, withdrawResponse.ThirdPartyOrderNo));

        await paymentClient
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                settings.FormId,
                withdrawResponse.ThirdPartyOrderNo,
                cancellationToken)
            .ConfigureAwait(false);

        return new WithdrawResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, ApolloPayUtility.SerializerOptions)
        };
    }
}