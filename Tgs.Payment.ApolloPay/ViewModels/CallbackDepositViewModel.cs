using Microsoft.AspNetCore.Mvc;

namespace Tgs.Payment.ApolloPay.ViewModels;

public class CallbackDepositViewModel
{
    [FromForm(Name = "tradeNo")]
    public required string TradeNo { get; init; }

    [FromForm(Name = "topupAmount")]
    public required string TopupAmount { get; init; }

    [FromForm(Name = "tradeStatus")]
    public int TradeStatus { get; init; }

    [FromForm(Name = "message")]
    public required string Message { get; init; }

    [FromForm(Name = "sign")]
    public required string Sign { get; init; }
}
