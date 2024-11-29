using ServerSample.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/Report", (ReportDTO request) =>
{
    return HttpResponseDTO<bool>.Success(true,"has update.");
});

app.MapPost("/Verification", (VerifyDTO request) =>
{
    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "packages", "packet_20241125233523804_1.0.0.1.zip");
    var packet = new FileInfo(filePath);
    var result = new List<VerificationResultDTO>
    {
        new VerificationResultDTO
        {
            RecordId = 1,
            Name = "packet_20241125233523804_1.0.0.1",
            Hash = "6c1ef824df2443b95e93c51d91de1c77447884602da2275a182a610a9429a835",
            ReleaseDate = DateTime.Now,
            Url = "http://localhost:5000/packages/packet_20241125233523804_1.0.0.1.zip",
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