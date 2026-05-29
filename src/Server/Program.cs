using ServerSample.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string packageName = "patch_20260529221936";

app.MapPost("/Upgrade/Report", (ReportDTO request) =>
{
    return HttpResponseDTO<bool>.Success(true,"has update.");
});

app.MapPost("/Upgrade/Verification", (VerifyDTO request, IWebHostEnvironment env) =>
{
    // Use WebRootPath to correctly locate wwwroot in both dev (project dir) and publish scenarios.
    var filePath = Path.Combine(env.WebRootPath, "packages", $"{packageName}.zip");
    var packet = new FileInfo(filePath);
    if (!packet.Exists)
    {
        return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.InnerException(
            $"Package file not found: {filePath}");
    }

    // Only return patches whose AppType matches the request.
    // Client convention: Client=1, Upgrade=2 (matches GeneralUpdate.Core.AppType enum).
    // This patch is a main application update (AppType=Client).
    const int patchAppType = 2; // Client
    if (request.AppType != null && request.AppType != patchAppType)
    {
        return HttpResponseDTO<IEnumerable<VerificationResultDTO>>.Success(
            Enumerable.Empty<VerificationResultDTO>(), "No matching update for this app type.");
    }

    var result = new List<VerificationResultDTO>
    {
        new VerificationResultDTO
        {
            RecordId = 1,
            Name = packageName,
            Hash = "0ad66de07179921e7ab5d2f38d39e92ca73969136d42536e0381b9f86082b4e5",
            ReleaseDate = DateTime.Now,
            Url = $"http://localhost:5000/packages/{packageName}.zip",
            Version = "1.0.0.1",
            AppType = patchAppType,
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