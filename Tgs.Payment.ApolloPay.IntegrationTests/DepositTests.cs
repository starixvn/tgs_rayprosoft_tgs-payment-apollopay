using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.ApolloPay.Client;
using Tgs.Payment.Clients;
using Xunit.Abstractions;

namespace Tgs.Payment.ApolloPay.IntegrationTests;

public class DepositTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task 代收回調_訂單狀態成功()
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

        var formId = Guid.Parse("921b6b72-5791-4311-aa17-2dc3d67d0311");
        var brandId = Guid.NewGuid();
        var apiKey = "apiKey";

        var request = new Dictionary<string, string>
        {
            { "tradeNo", "921b6b72-5791-4311-aa17-2dc3d67d0311" },
            { "topupAmount", "100000" },
            { "tradeStatus", "1" },
            { "message", "成功" },
            { "sign", "640c911af70c3af376d5b1d04e20c1e7" }
        };

        var content = new MultipartFormDataContent();
        foreach (var keyValuePair in request)
        {
            content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
        }

        var applyInfo = new ApolloPayDepositApplyInfo(
            new ApolloPayDepositSettings(
                "merchantNo",
                apiKey,
                formId,
                "QRIS",
                null,
                1m),
             new ApolloPayDepositResponse(
                100000m,
                formId,
                new Uri("https://cc.inhepay.info/polarID121w9e3b7i/depositID/csr?oid=202410011311008811750968"),
                "202410011311008811750968"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 100000m,
                ThirdPartyOrderNo = "202410011311008811750968",
                ApplyInfo = JsonSerializer.Serialize(applyInfo, ApolloPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("SUCCESS", actual);

        var paymentReturn = new ApolloPayDepositReturn()
        {
            Status = "1",
            FormId = formId,
            OrderAmount = decimal.Parse("100000"),
            Sign = "640c911af70c3af376d5b1d04e20c1e7",
        };

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .ApproveDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Is(paymentReturn.OrderAmount),
                Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task 代收回調_訂單狀態失敗()
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

        var formId = Guid.Parse("921b6b72-5791-4311-aa17-2dc3d67d0311");
        var brandId = Guid.NewGuid();
        var apiKey = "apiKey";

        var request = new Dictionary<string, string>
        {
            { "tradeNo", "921b6b72-5791-4311-aa17-2dc3d67d0311" },
            { "topupAmount", "100000" },
            { "tradeStatus", "-2" },
            { "message", "成功" },
            { "sign", "640c911af70c3af376d5b1d04e20c1e7" }
        };

        var content = new MultipartFormDataContent();
        foreach (var keyValuePair in request)
        {
            content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
        }

        var applyInfo = new ApolloPayDepositApplyInfo(
            new ApolloPayDepositSettings(
                "merchantNo",
                apiKey,
                formId,
                "QRIS",
                null,
                1m),
             new ApolloPayDepositResponse(
                100000m,
                formId,
                new Uri("https://cc.inhepay.info/polarID121w9e3b7i/depositID/csr?oid=202410011311008811750968"),
                "202410011311008811750968"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 100000m,
                ThirdPartyOrderNo = "202410011311008811750968",
                ApplyInfo = JsonSerializer.Serialize(applyInfo, ApolloPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("SUCCESS", actual);

        var paymentReturn = new ApolloPayDepositReturn()
        {
            Status = "-2",
            FormId = formId,
            OrderAmount = decimal.Parse("100000"),
            Sign = "640c911af70c3af376d5b1d04e20c1e7",
        };

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .RejectDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),                
                Arg.Any<CancellationToken>());
    }
}