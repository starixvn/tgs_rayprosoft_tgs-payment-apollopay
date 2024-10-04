using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.ApolloPay.ApplicationServices;
using Tgs.Payment.ApolloPay.Client;
using Tgs.Payment.ApolloPay.ViewModels;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApolloPay.Controllers;

[Route("[controller]")]
[ApiController]
public class DepositController(ILogger<DepositController> logger) : ControllerBase
{
    [Consumes(MediaTypeNames.Multipart.FormData)]
    [HttpPost("callback")]
    public async Task<string> DepositCallbackAsync(
       [FromServices] IGetService<GetDepositFormQuery, DepositForm?> getService,
       [FromServices] IPaymentServiceClient paymentServiceClient,
       [FromServices] IPaymentClient paymentClient,
       [FromServices] ApolloPayHasher hasher,
       [FromForm] CallbackDepositViewModel source)
    {
        var formId = Guid.Parse(source.TradeNo);

        var depositForm = await getService
            .GetAsync(new(formId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (depositForm == null)
        {
            logger.LogWarning("Can't found deposit form({id}).", formId);
            return "fail";
        }

        var apiKey = depositForm.ApplyInfo!.Settings.ApiKey;

        var sign = source.Sign;
        var hash = hasher.ComputeHash($"{source.TradeNo}{source.TopupAmount}{apiKey}");

        logger.LogInformation("Compute hash: {hash}, sign: {sign}", hash, sign);

        if (sign != hash)
        {
            logger.LogWarning("Sign value invalid. input: {sign}, compute: {hash}", sign, hash);
            return "fail";
        }

        var exchangeRate = depositForm.ApplyInfo.Settings.ExchangeRate;

        var paymentReturn = new ApolloPayDepositReturn()
        {
            OrderAmount = decimal.Parse(source.TopupAmount),
            FormId = formId,
            Status = source.TradeStatus.ToString(),
            Sign = source.Sign,
            AfterExchangeOrderAmount = decimal.Parse(source.TopupAmount) / exchangeRate,
        };

        var fsm = PaymentStateMachineFactory.CreateDepositStateMachine(depositForm.Status);

        switch (paymentReturn.Status)
        {
            case "1"://成功
                if (fsm.CanFire(PaymentTrigger.PaymentDepositCompleted))
                {
                    await fsm.FireAsync(PaymentTrigger.PaymentDepositCompleted).ConfigureAwait(false);

                    await paymentServiceClient.SubmitDepositFormStatusAsync(
                        depositForm.Id,
                        paymentReturn: JsonSerializer.Serialize(paymentReturn, ApolloPayUtility.SerializerOptions),
                        cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

                    await paymentClient.ApproveDepositAsync(
                        depositForm.Id,
                        depositForm.BrandId,
                        paymentReturn.AfterExchangeOrderAmount,
                        HttpContext.RequestAborted).ConfigureAwait(false);
                }

                break;

            default:
                break;
        }

        return "SUCCESS";
    }
}