using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Payment.ApolloPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.ApolloPay.MessageQueue;
internal class DepositProcessor(
    ApolloPayClient apolloPayClient,
    IOptions<ApolloPayOptions> optionsAccessor,
    ILogger<DepositProcessor> logger)
    : IMessageProcessor<DepositRequest, DepositResponse>
{
    private readonly ApolloPayOptions _options = optionsAccessor.Value;

    public async ValueTask<DepositResponse> HandleAsync(
        string subject,
        DepositRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("data applyInfo:{applyInfo}", data.ApplyInfo);

        var settings = JsonSerializer.Deserialize<ApolloPayDepositSettings>(
            data.ApplyInfo,
            ApolloPayUtility.SerializerOptions)!;

        var order = new DepositOrder(
            decimal.Parse(data.Amount) * settings.ExchangeRate,
            settings.FormId,
            new Uri($"{_options.CallbackBaseUrl}/deposit/callback"),
            settings.ChannelType,
            settings.BankCode);

        var depositResponse = await apolloPayClient
            .DepositAsync(settings.MerchantNo, settings.ApiKey, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new ApolloPayDepositApplyInfo(
            settings,
            new ApolloPayDepositResponse(
                depositResponse.Amount,
                depositResponse.FormId,
                depositResponse.RedirectUrl,
                depositResponse.ThirdPartyOrderNo));

        return new DepositResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, ApolloPayUtility.SerializerOptions)
        };
    }
}
