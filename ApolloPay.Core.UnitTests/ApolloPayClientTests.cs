using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.ApolloPay;

namespace ApolloPay.Core.UnitTests;

public class ApolloPayClientTests
{
    [Fact]
    public async Task 餘額查詢()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new ApolloPayHasher(NullLogger<ApolloPayHasher>.Instance);

        var sut = new ApolloPayClient(httpClient, signer, TimeProvider.System, NullLogger<ApolloPayClient>.Instance);

        var merchantNo = "merNo";
        var apiKey = "apiKey";

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/apollopaysApi/inquiry/getMerBalance")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {"Success":1,"Message":"","Balance":0,"Freeze":0}
                """));

        var actual = await sut.QueryBalanceAsync(merchantNo, apiKey);

        Assert.Equal(0, actual);
    }

    [Fact]
    public async Task 代收_QRIS()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new ApolloPayHasher(NullLogger<ApolloPayHasher>.Instance);

        var sut = new ApolloPayClient(httpClient, signer, TimeProvider.System, NullLogger<ApolloPayClient>.Instance);

        var formId = Guid.Parse("921b6b72-5791-4311-aa17-2dc3d67d0311");
        var merchantNo = "merNo";
        var apiKey = "apiKey";

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

        var order = new DepositOrder(
           100000m,
           formId,
           new Uri("https://2vvcthng-7024.asse.devtunnels.ms/deposit/callback"),
           "QRIS",
           null);

        var actual = await sut.DepositAsync(merchantNo, apiKey, order);

        Assert.Equal("202410011311008811750968", actual.ThirdPartyOrderNo);
        Assert.Equal(new Uri("https://cc.inhepay.info/polarID121w9e3b7i/depositID/csr?oid=202410011311008811750968"), actual.RedirectUrl);
    }

    [Fact]
    public async Task 代付()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new ApolloPayHasher(NullLogger<ApolloPayHasher>.Instance);

        var sut = new ApolloPayClient(httpClient, signer, TimeProvider.System, NullLogger<ApolloPayClient>.Instance);

        var formId = Guid.Parse("e7c2a363-a929-4626-8d7a-d9d3771c8187");
        var merchantNo = "merNo";
        var apiKey = "apiKey";

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/apollopaysApi/payout/createOrder")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {"Success":1,"Message":"Done","oid":"20241001161715629369233"}
                """));

        var order = new WithdrawOrder(
           formId,
           100000m,
           new Uri("https://2vvcthng-7024.asse.devtunnels.ms/withdraw/callback"),
           "Payout",
           "BCA_ID",
           "11111111111111",
           "testAccountName");

        var actual = await sut.WithdrawAsync(merchantNo, apiKey, order);

        Assert.Equal(100000m, actual.Amount);
        Assert.Equal(formId, actual.FormId);
        Assert.Equal("20241001161715629369233", actual.ThirdPartyOrderNo);
    }
}