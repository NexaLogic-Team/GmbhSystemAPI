using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace GmbhSystem.Infrastructure.Services;

public interface IMediaService
{
    Task<string> UploadFileAsync(string filePath, string bucketName);
    Task<Stream> GetFileAsync(string bucketName, string key);
    Task<string> GeneratePresignedUrlAsync(string bucketName, string key);
    Task UploadContentAsync(string bucketName, string key, string content);
    Task<string> GetContentAsync(string bucketName, string key);
}

public class CloudflareR2Service : IMediaService
{
    private readonly IAmazonS3 _s3Client;

    public CloudflareR2Service(IConfiguration configuration)
    {
        var accessKey = configuration["CloudflareR2:AccessKey"]!;
        var secretKey = configuration["CloudflareR2:SecretKey"]!;
        var accountId = configuration["CloudflareR2:AccountId"]!;

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        _s3Client = new AmazonS3Client(credentials, new AmazonS3Config
        {
            ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
            ForcePathStyle = true,
            AuthenticationRegion = "auto",
        });
    }

    public async Task<string> UploadFileAsync(string filePath, string bucketName)
    {
        var request = new PutObjectRequest
        {
            FilePath = filePath,
            BucketName = bucketName,
            Key = Path.GetFileName(filePath),
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true,
            UseChunkEncoding = false
        };

        var response = await _s3Client.PutObjectAsync(request);
        return response.ETag;
    }

    public async Task<Stream> GetFileAsync(string bucketName, string key)
    {
        var response = await _s3Client.GetObjectAsync(bucketName, key);
        return response.ResponseStream;
    }

    public async Task<string> GeneratePresignedUrlAsync(string bucketName, string key)
    {
        var presign = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.Now.AddDays(7),
        };

        return await Task.FromResult(_s3Client.GetPreSignedURL(presign));
    }
    
    public async Task UploadContentAsync(string bucketName, string key, string content)
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var request = new PutObjectRequest
        {
            InputStream = stream,
            BucketName = bucketName,
            Key = key,
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true,
            UseChunkEncoding = false
        };

        await _s3Client.PutObjectAsync(request);
    }

    public async Task<string> GetContentAsync(string bucketName, string key)
    {
        try
        {
            var response = await _s3Client.GetObjectAsync(bucketName, key);
            using var reader = new StreamReader(response.ResponseStream);
            return await reader.ReadToEndAsync();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null!;
        }
    }
}