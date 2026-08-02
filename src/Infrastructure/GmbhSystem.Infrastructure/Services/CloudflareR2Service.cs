using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace GmbhSystem.Infrastructure.Services;

public interface IMediaService
{
    Task<string> UploadFileAsync(Stream stream, string fileName, string bucketName, string contentType);
    Task<Stream> GetFileAsync(string bucketName, string key);
    Task<string> GeneratePresignedUrlAsync(string bucketName, string key);
    Task UploadContentAsync(string bucketName, string key, string content);
    Task<string?> GetContentAsync(string bucketName, string key);
    Task DeleteFileAsync(string bucketName, string key);
}

public class CloudflareR2Service : IMediaService
{
    private readonly IAmazonS3 _s3Client;

    public CloudflareR2Service(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task<string> UploadFileAsync(Stream stream, string fileName, string bucketName, string contentType)
    {
        var request = new PutObjectRequest
        {
            InputStream = stream,
            BucketName = bucketName,
            Key = fileName, // မူရင်း File Name ကို Key အဖြစ် သုံးမည်
            ContentType = contentType,
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
            Expires = DateTime.UtcNow.AddDays(7) // UTC သုံးထားသည်
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

    public async Task<string?> GetContentAsync(string bucketName, string key)
    {
        try
        {
            var response = await _s3Client.GetObjectAsync(bucketName, key);
            using var reader = new StreamReader(response.ResponseStream);
            return await reader.ReadToEndAsync();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
    
    public async Task DeleteFileAsync(string bucketName, string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        try
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest);
        }
        catch (Exception ex)
        {
            // Log Error according to your logging setup
            Console.WriteLine($"[R2-DELETE-ERROR] Failed to delete key '{key}': {ex.Message}");
        }
    }
}