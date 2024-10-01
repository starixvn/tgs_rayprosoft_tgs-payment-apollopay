using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.ApolloPay;
using Tgs.Payment.ApolloPay.Client;
using Tgs.Payment.ApolloPay.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;

namespace ApolloPay.Core.UnitTests;

public class WithdrawProcessorTests
{
    [Fact]
    public async Task 代付()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");

        var fakePayClient = new ApolloPayClient(
            httpClient,
            new ApolloPayHasher(NullLogger<ApolloPayHasher>.Instance),
            TimeProvider.System,
            NullLogger<ApolloPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var options = new ApolloPayOptions
        {
            CallbackBaseUrl = new Uri("https://localhost"),
            PaymentApiUrl = new Uri("https://localhost"),
            PaymentServiceBaseUrl = new Uri("http://localhost:5541")
        };

        var sut = new WithdrawProcessor(
            fakePayClient,
            new OptionsWrapper<ApolloPayOptions>(options),
            fakePaymentClient);

        var formId = Guid.Parse("e7c2a363-a929-4626-8d7a-d9d3771c8187");
        var merchantId = "merchantId";
        var merchantKey = "merchantKey";

        var applySettings = new ApolloPayWithdrawSettings(
            merchantId,
            merchantKey,
            formId,
            "Payout",
            "BCA_ID",
            "11111111111111",
            "testAccountName");

        var request = new WithdrawRequest
        {
            Amount = "100",
            ApplyInfo = JsonSerializer.Serialize(applySettings, ApolloPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/apollopaysApi/payout/createOrder")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {"Success":1,"Message":"Done","oid":"20241001161715629369233"}
                """));

        // Act
        var actual = await sut.HandleAsync(
            "test",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<ApolloPayWithdrawApplyInfo>(actual.Result, ApolloPayUtility.SerializerOptions);

        // Assert
        _ = fakePaymentClient.Received(1)
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                Arg.Is(applySettings.FormId),
                Arg.Is("20241001161715629369233"));
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
    }
}