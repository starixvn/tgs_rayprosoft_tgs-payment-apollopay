using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Payment.ApolloPay;

namespace ApolloPay.Core.UnitTests;

public class HasherTests
{
    [Fact]
    public void 餘額查詢()
    {
        var merNo = "merNo";
        var datetime = "20241001015126";
        var apiKey = "apiKey";

        var sut = new ApolloPayHasher(NullLogger<ApolloPayHasher>.Instance);

        var actual = sut.ComputeHash($"{merNo}{datetime}{apiKey}");

        var expected = "a63c4a9537e80f1f95944346e4db828a";

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 代收回調()
    {
        var tradeNo = "921b6b72-5791-4311-aa17-2dc3d67d0311";
        var topupAmount = "100000";
        var apiKey = "apiKey";

        var sut = new ApolloPayHasher(NullLogger<ApolloPayHasher>.Instance);

        var actual = sut.ComputeHash($"{tradeNo}{topupAmount}{apiKey}");

        var expected = "640c911af70c3af376d5b1d04e20c1e7";

        Assert.Equal(expected, actual);
    }
}