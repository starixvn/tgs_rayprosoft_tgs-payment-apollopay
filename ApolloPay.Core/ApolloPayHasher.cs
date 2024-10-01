using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Tgs.Payment.ApolloPay;

public class ApolloPayHasher(ILogger<ApolloPayHasher> logger)
{
    public string ComputeHash(string source)
    {
        logger.LogInformation("Compute Hash Source:{source}", source);

        var data = MD5.HashData(Encoding.UTF8.GetBytes(source));

        return Convert.ToHexString(data).ToLower();
    }
}