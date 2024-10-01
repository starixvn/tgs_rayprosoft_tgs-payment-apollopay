using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.ApolloPay;

public class ApolloPayClient(
    HttpClient httpClient,
    ApolloPayHasher hasher,
    TimeProvider timeProvider,
    ILogger<ApolloPayClient> logger)
{
    public async ValueTask<ApolloPayClientDepositResponse> DepositAsync(
        string merchantNo,
        string apiKey,
        DepositOrder order,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["merNo"] = merchantNo,
            ["tradeNo"] = order.FormId.ToString(),
            ["cType"] = order.ChannelType,
            ["orderAmount"] = order.Amount.ToString("#.##"),
            ["notifyUrl"] = order.CallbackUrl.AbsoluteUri,
        };

        if (order.BankCode != null)
            dict["bankCode"] = order.BankCode;

        dict["sign"] = hasher.ComputeHash($"{dict["merNo"]}{dict["tradeNo"]}{dict["orderAmount"]}{apiKey}");

        var content = new FormUrlEncodedContent(dict);

        var response = await httpClient
            .PostAsync("/apollopaysApi/pay/createOrder", content, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("ApolloPayClient Deposit response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Deposit response can't read.");

        if (node["Success"]!.GetValue<int>() != 1)
            throw new ApiFailException(node["Message"]!.ToString());

        return new ApolloPayClientDepositResponse(
            Amount: decimal.Parse(node["Params"]!["orig_money"]!.GetValue<string>()),
            FormId: order.FormId,
            ThirdPartyOrderNo: node["oid"]!.GetValue<string>(),
            RedirectUrl: new Uri(node["PayPage"]!.GetValue<string>()));
    }

    public async ValueTask<ApolloPayClientWithdrawResponse> WithdrawAsync(
        string merchantNo,
        string apiKey,
        WithdrawOrder order,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["merNo"] = merchantNo,
            ["tradeNo"] = order.FormId.ToString(),
            ["cType"] = order.ChannelCode,
            ["bankCode"] = order.BankCode,
            ["bankCardNo"] = order.BankCardNo,
            ["orderAmount"] = order.Amount.ToString("#.##"),
            ["accountName"] = order.AccountName,
            ["notifyUrl"] = order.CallbackUrl.AbsoluteUri,
        };

        dict["sign"] = hasher.ComputeHash($"{dict["merNo"]}{dict["tradeNo"]}{dict["bankCode"]}{dict["orderAmount"]}{apiKey}");

        var content = new FormUrlEncodedContent(dict);

        var response = await httpClient
            .PostAsync("/apollopaysApi/payout/createOrder", content, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("ApolloPayClient Withdraw response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Withdraw response can't read.");

        if (node["Success"]!.GetValue<int>() != 1)
            throw new ApiFailException(node["Message"]!.ToString());

        return new ApolloPayClientWithdrawResponse(
            FormId: order.FormId,
            ThirdPartyOrderNo: node["oid"]!.GetValue<string>(),
            Amount: order.Amount);
    }

    public async ValueTask<decimal> QueryBalanceAsync(
        string merchantNo,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var datetime = timeProvider.GetUtcNow().ToString("yyyyMMddHHmmss");
        var dict = new Dictionary<string, string?>
        {
            ["merNo"] = merchantNo,
            ["datetime"] = datetime
        };

        dict["sign"] = hasher.ComputeHash($"{merchantNo}{datetime}{apiKey}");

        var content = new FormUrlEncodedContent(dict);

        var response = await httpClient
            .PostAsync($"/apollopaysApi/inquiry/getMerBalance", content, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("ApolloPayClient QueryBalance response Body:{body}", body);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Balance response can't read.");

        if (node["Success"]!.GetValue<int>() != 1)
            throw new ApiFailException(node["Message"]!.GetValue<string>());

        return node["Balance"]!.GetValue<decimal>();
    }
}