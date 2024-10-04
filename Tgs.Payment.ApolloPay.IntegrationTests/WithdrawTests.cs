using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.ApolloPay.Client;
using Tgs.Payment.Clients;
using Xunit.Abstractions;

namespace Tgs.Payment.ApolloPay.IntegrationTests;

public class WithdrawTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task 代付回調_訂單狀態失敗()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = ApolloPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("e7c2a363-a929-4626-8d7a-d9d3771c8187");
        var brandId = Guid.NewGuid();
        var merchantNo = "merchantNo";
        var apiKey = "apiKey";

        var settings = new ApolloPayWithdrawSettings(
            merchantNo,
            apiKey,
            formId,
            "Payout",
            "BCA_ID",
            "11111111111111",
            "testAccountName",
            1m);
        var withdrawResponse = new ApolloPayWithdrawResponse(formId, "20241001161715629369233");

        var applyInfo = new ApolloPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, ApolloPayUtility.SerializerOptions)
            });

        var request = new Dictionary<string, string>
            {
                { "tradeNo", "e7c2a363-a929-4626-8d7a-d9d3771c8187" },
                { "orderAmount", "100000" },
                { "tradeStatus", "-1" },
                { "message", "" },
                { "sign", "d802a83cfe6e9c4f5362ae8f112462e8" }
            };

        var content = new MultipartFormDataContent();
        foreach (var keyValuePair in request)
        {
            content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
        }

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();
      
        Assert.Equal("SUCCESS", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .RejectWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 代付回調_訂單狀態成功()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = ApolloPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("e7c2a363-a929-4626-8d7a-d9d3771c8187");
        var brandId = Guid.NewGuid();
        var merchantNo = "merchantNo";
        var apiKey = "apiKey";

        var settings = new ApolloPayWithdrawSettings(
            merchantNo,
            apiKey,
            formId,
            "Payout",
            "BCA_ID",
            "11111111111111",
            "testAccountName",
            1m);
        var withdrawResponse = new ApolloPayWithdrawResponse(formId, "20241001161715629369233");

        var applyInfo = new ApolloPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, ApolloPayUtility.SerializerOptions)
            });

        var request = new Dictionary<string, string>
        {
            { "tradeNo", "e7c2a363-a929-4626-8d7a-d9d3771c8187" },
            { "orderAmount", "100000" },
            { "tradeStatus", "1" },
            { "message", "" },            
            { "sign", "d802a83cfe6e9c4f5362ae8f112462e8" }
        };

        var content = new MultipartFormDataContent();
        foreach (var keyValuePair in request)
        {
            content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
        }

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();        

        Assert.Equal("SUCCESS", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .PaidWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }
}