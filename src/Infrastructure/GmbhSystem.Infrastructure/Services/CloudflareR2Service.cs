using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace GmbhSystem.Infrastructure.Services;

public interface IMediaService
{
    Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken);
    Task<(Stream Stream, string ContentType)> GetFileAsync(string fileName, CancellationToken cancellationToken);
}

public class CloudflareR2Service : IMediaService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public CloudflareR2Service(IConfiguration configuration)
    {
        var serviceUrl = configuration["CloudflareR2:ServiceUrl"]
                         ?? throw new InvalidOperationException("CloudflareR2:ServiceUrl is missing in appsettings.json");
        var accessKey = configuration["CloudflareR2:AccessKey"]
                        ?? throw new InvalidOperationException("CloudflareR2:AccessKey is missing in appsettings.json");
        var secretKey = configuration["CloudflareR2:SecretKey"]
                        ?? throw new InvalidOperationException("CloudflareR2:SecretKey is missing in appsettings.json");
        _bucketName = configuration["CloudflareR2:BucketName"]
                      ?? throw new InvalidOperationException("CloudflareR2:BucketName is missing in appsettings.json");
        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
    
            // R2 အတွက် Path Style ကို မဖြစ်မနေ true ထားရပါမည်
            ForcePathStyle = true,
    
            // RegionEndpoint အစား R2 ၏ Standard ဖြစ်သော AuthenticationRegion = "auto" ကို ပြောင်းသုံးပါ
            AuthenticationRegion = "auto",
    
            // Network Connection နှေးကွေးပါက Timeout မဖြစ်စေရန် အချိန်တိုးပေးခြင်း
            Timeout = TimeSpan.FromSeconds(60),
            // ReadWriteTimeout = TimeSpan.FromSeconds(120)
        };

        var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
        _s3Client = new 
            AmazonS3Client(credentials, config);
    }

    // 1. WRITE (Upload to Cloudflare R2)
    // 1. WRITE (Upload to Cloudflare R2)
    public async Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        using var stream = file.OpenReadStream();

        // Stream position အစ (0) တွင် မရှိပါက Reset ပြန်လုပ်ပေးခြင်း
        if (stream.CanSeek && stream.Position != 0)
        {
            stream.Position = 0;
        }

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = uniqueFileName,
            InputStream = stream,
            ContentType = file.ContentType,
            DisablePayloadSigning = true,
            
            // [အရေးကြီးဆုံး ပြင်ဆင်ချက်များ]
            // 1. AWS SDK ၏ Chunked Encoding ကို ပိတ်လိုက်ပါ (R2 က Chunking ကို ဖြတ်ချတတ်လို့ပါ)
            // UseChunkedEncoding = false,
            
            // 2. Stream ကို လုံခြုံစွာ အလိုအလျောက် ပိတ်ပေးရန်
            AutoCloseStream = true
        };

        // 3. R2 Server က Stream အရှည်ကို မသိဘဲ Connection ဖြတ်မချအောင် File Length ကို အတိအကျ ကြေညာပေးခြင်း
        putRequest.Headers.ContentLength = file.Length;

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        return uniqueFileName;
    }

    // 2. READ (Download / Stream from Cloudflare R2)
    public async Task<(Stream Stream, string ContentType)> GetFileAsync(string fileName,
        CancellationToken cancellationToken)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName
        };

        var response = await _s3Client.GetObjectAsync(getRequest, cancellationToken);
        return (response.ResponseStream, response.Headers.ContentType);
    }
}