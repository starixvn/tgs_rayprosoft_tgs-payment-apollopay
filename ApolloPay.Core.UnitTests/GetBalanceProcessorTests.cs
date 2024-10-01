using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.ApolloPay;
using Tgs.Payment.ApolloPay.MessageQueue;
using Tgs.Payment.Protos;

namespace ApolloPay.Core.UnitTests;

public class GetBalanceProcessorTests
{
    [Fact]
    public async Task GetBalanceProcessor_取得商戶餘額()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");

        var fakeTimeProvider = Substitute.For<TimeProvider>();

        var fakePayClient = new ApolloPayClient(
            httpClient,
            new ApolloPayHasher(NullLogger<ApolloPayHasher>.Instance),
            fakeTimeProvider,
            NullLogger<ApolloPayClient>.Instance);

        var sut = new GetBalanceProcessor(fakePayClient);

        var request = new PaymentBalanceRequest
        {
            BrandPaymentSettings = """
                {
                    "merchantNo": "merchantNo",
                    "apiKey": "apiKey"
                }
                """
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/apollopaysApi/inquiry/getMerBalance")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {"Success":1,"Message":"","Balance":0,"Freeze":0}
                """));

        // Act
        var actual = await sut.HandleAsync("test", request, null);

        // Assert
        Assert.Equal(0M, actual.Balance.ToDecimal());
    }
}