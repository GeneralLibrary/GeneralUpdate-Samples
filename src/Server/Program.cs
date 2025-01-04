using ServerSample.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string packageName = "packet_20250102230201638_1.0.0.1";

app.MapPost("/Upgrade/Report", (ReportDTO request) =>
{
    return HttpResponseDTO<bool>.Success(true,"has update.");
});

app.MapPost("/Upgrade/Verification", (VerifyDTO request) =>
{
    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "packages", $"{packageName}.zip");
    var packet = new FileInfo(filePath);
    var result = new List<VerificationResultDTO>
    {
        new VerificationResultDTO
        {
            RecordId = 1,
            Name = packageName,
            Hash = "ad1a85a9169ca0083ab54ba390e085c56b9059efc3ca8aa1ec9ed857683cc4b1",
            ReleaseDate = DateTime.Now,
            Url = $"http://localhost:5000/packages/{packageName}.zip",
            Version = "1.0.0.1",
            AppType = 1,
            Platform = 1,
            ProductId = "2d974e2a-31e6-4887-9bb1-b4689e98c77a",
            IsForcibly = false,
            Format = ".zip",
            Size = packet.Length,
            IsFreeze = false
        }
    };
    return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Success(result,"has update.");
});

app.UseStaticFiles();
app.Run();