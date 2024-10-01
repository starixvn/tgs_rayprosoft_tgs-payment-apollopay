using System.Text.Json.Serialization;

namespace Tgs.Payment.ApolloPay;
public record BrandPaymentMerchantInfo(
    [property: JsonPropertyName("merchantNo")] string MerchantNo,
    [property: JsonPropertyName("apiKey")] string ApiKey);