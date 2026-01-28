using System.Text.Json.Serialization;

namespace KimmyEsthi.Services;

public class Service
{
    public int Id { get; set; }
    public required string ServiceName { get; set; }
    public string? PromotionName { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ServiceType ServiceType { get; set; }
    public required string CardTitle { get; set; }
    public required string CardContent { get; set; }
    public required string CardImgSrc { get; set; }
    public string? CardOverlayContent { get; set; }
    public string[]? PackageItems { get; set; }
    public string[]? Tags { get; set; }
    public bool? NotBookable { get; set; }
    public string? Price { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum ServiceType
{
    Facial,
    Package,
    AddOn,
}
