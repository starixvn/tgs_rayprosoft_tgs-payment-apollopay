using Microsoft.AspNetCore.Mvc;

namespace Tgs.Payment.ApolloPay.ViewModels;

public class CallbackWithdrawViewModel
{
    /// <summary>
    /// 商户订单号
    /// </summary>
    [FromForm(Name = "tradeNo")]
    public required string TradeNo { get; init; }
    /// <summary>
    /// 代付金額
    /// </summary>
    [FromForm(Name = "orderAmount")]
    public required string OrderAmount { get; init; }
    /// <summary>
    /// 支付状态
    /// </summary>
    [FromForm(Name = "tradeStatus")]
    public int TradeStatus { get; init; }
    /// <summary>
    /// 狀態信息
    /// </summary>
    [FromForm(Name = "message")]
    public string? Message { get; init; }
    
    [FromForm(Name = "sign")]
    public required string Sign { get; init; }
}
