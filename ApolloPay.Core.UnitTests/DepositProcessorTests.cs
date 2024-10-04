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
public class DepositProcessorTests
{
    [Fact]
    public async Task 儲值要求處理()
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
        var formId = Guid.Parse("b667ff5c-b142-4460-9a52-bab2bf4617e4");
        var merchantNo = "merchantNo";
        var apiKey = "apiKey";

        var sut = new DepositProcessor(
            fakePayClient,
            new OptionsWrapper<ApolloPayOptions>(options),
            NullLogger<DepositProcessor>.Instance);

        var applySettings = new ApolloPayDepositSettings(
            merchantNo,
            apiKey,
            formId,
            "QRIS",
            null);

        var request = new DepositRequest
        {
            Amount = "100000",
            ApplyInfo = JsonSerializer.Serialize(applySettings, ApolloPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/apollopaysApi/pay/createOrder")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "Success": 1,
                    "Message": "",
                    "oid": "202410011311008811750968",
                    "PayPage": "https:\/\/cc.inhepay.info\/polarID121w9e3b7i\/depositID\/csr?oid=202410011311008811750968",
                    "Params": {
                        "bankAccount": "240508005000000",
                        "bankCode": "QRIS_P",
                        "bankName": "QRIS",
                        "branchName": "-",
                        "bankAccountName": "ILHAM TOFANDHI",
                        "noteNo": "b0a4de0eb4964b7eb014d0830",
                        "orig_money": "100000",
                        "money": "100000",
                        "pay_page_type": "NewQRIS_P",
                        "qrcode_url": "https:\/\/cc.inhepay.info\/polarID121w9e3b7i\/service\/getQrcode?qr_text=00020101021226650013ID.PAYDIA.WWW011893600818024050800502152405080050000000303UKE51440014ID.CO.QRIS.WWW0215ID10243233228250303UKE52045969530336054061000005802ID5911  THEBAG.ME6009TANGERANG61051511462550125b0a4de0eb4964b7eb014d083007152405080050000000803api630424FA",
                        "phone_no": "85213243590"
                    }
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "test",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<ApolloPayDepositApplyInfo>(actual.Result, ApolloPayUtility.SerializerOptions);

        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
        Assert.Equal(
            new ApolloPayDepositResponse(
                100000m,
                applySettings.FormId,
                new Uri("https://cc.inhepay.info/polarID121w9e3b7i/depositID/csr?oid=202410011311008811750968"),
                "202410011311008811750968"),
            applyInfo.Response);
    }
}
