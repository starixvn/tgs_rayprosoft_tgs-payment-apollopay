using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NATS.Client.Core;
using Tgs.Payment.Clients;
using Tgs.Payment.ApolloPay;
using Tgs.Payment.ApolloPay.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPaymentClient()
    .AddKeyedSingleton("Nats",
    (sp, _) => new NatsConnection(NatsOpts.Default with
    {
        Url = "nats://localhost:4222"
    }))
    .AddPhoenixToolkitsMessageQueue()
    .AddNatsMessageQueue(natsConfig => natsConfig
        .ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats")))
    .AddNatsGlobPatternExchange("*");

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();

var merchantNo = "merchantNo";
var apiKey = "apiKey";

//代收QRIS

var orderNo = Guid.NewGuid();
var amount = 100_000m;

var depositsettings = new ApolloPayDepositSettings(
    merchantNo,
    apiKey,
    orderNo,
    "QRIS",
    null);

var depositreturn = await paymentClient.DepositAsync(
    "tgs.payment.apollopay.deposit",
    amount,
    JsonSerializer.Serialize(depositsettings));
logger.LogInformation($"deposit return: {depositreturn}");

//代付
/*
var orderNo = Guid.NewGuid();
var withdrawSettings = new ApolloPayWithdrawSettings(
    merchantNo,
    apiKey,
    orderNo,
    "Payout",
    "BNI_ID",
    "222222222222",
    "testAccountName");

var withdrawReturn = await paymentClient.WithdrawAsync(
    "tgs.payment.apollopay.withdraw",
    100_000M,
    JsonSerializer.Serialize(withdrawSettings));
logger.LogInformation($"Withdraw Return: {withdrawReturn}");*/

//代收訂單查詢
//var queryFormId = Guid.Parse("b667ff5c-b142-4460-9a52-bab2bf4617e4");

//var httpClient = new HttpClient
//{
//    BaseAddress = new Uri("https://openapi.fukuipay.ooo")
//};
//var client = new ApolloPayClient(
//    httpClient,
//    new ApolloPayHasher(NullLogger<ApolloPayHasher>.Instance),
//    TimeProvider.System,
//    NullLogger<ApolloPayClient>.Instance);
//var depositQuery = await client.QueryDepositFormAsync(merchantId, merchantKey, queryFormId);

//logger.LogInformation("{depositQuery}", depositQuery);

//餘額
/*
var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://gate.apollopays.com")
};
var client = new ApolloPayClient(
    httpClient,
    new ApolloPayHasher(NullLogger<ApolloPayHasher>.Instance),
    TimeProvider.System,
    NullLogger<ApolloPayClient>.Instance);
var balanceQuery = await client.QueryBalanceAsync(merchantNo, apiKey);

logger.LogInformation("Balance:{balanceQuery}", balanceQuery);*/