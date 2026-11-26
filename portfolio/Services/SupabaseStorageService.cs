
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;


namespace portfolio.Services;

public class SupabaseStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _secretKey;
    private readonly string _bucket;

    public SupabaseStorageService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;

        _supabaseUrl = configuration["Supabase:Url"]
            ?? throw new InvalidOperationException(
                "Supabase URL is missing.");

        _secretKey = configuration["Supabase:SecretKey"]
            ?? throw new InvalidOperationException(
                "Supabase secret key is missing.");

        _bucket = configuration["Supabase:Bucket"]
            ?? throw new InvalidOperationException(
                "Supabase bucket is missing.");
    }

    public async Task<(string ObjectPath, string PublicUrl)> UploadAsync(
        long projectId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        // Generate a unique filename.
        string extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        string objectPath =
            $"projects/{projectId}/{Guid.NewGuid():N}{extension}";

        // Encode the bucket and file path for the URL.
        string bucket = Uri.EscapeDataString(_bucket);

        string encodedPath = string.Join(
            "/",
            objectPath.Split('/').Select(Uri.EscapeDataString));

        string uploadUrl =
            $"{_supabaseUrl.TrimEnd('/')}/storage/v1/object/{bucket}/{encodedPath}";

        using var request = new HttpRequestMessage(
            HttpMethod.Post, uploadUrl);

        // Authenticate using the server-side secret key.
        request.Headers.Add("apikey", _secretKey);

        // Attach the actual image file.
        var content = new StreamContent(file.OpenReadStream());

        content.Headers.ContentType =
            new MediaTypeHeaderValue(file.ContentType);

        request.Content = content;

        using var response = await _httpClient.SendAsync(
            request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(
                cancellationToken);

            throw new HttpRequestException(
                $"Image upload failed: {error}");
        }

        // Construct the public image URL.
        string publicUrl =
            $"{_supabaseUrl.TrimEnd('/')}/storage/v1/object/public/{bucket}/{encodedPath}";

        return (objectPath, publicUrl);




    }


    public async Task DeleteAsync(string publicUrl)
    {
        string publicPrefix =
            $"{_supabaseUrl.TrimEnd('/')}/storage/v1/object/public/{Uri.EscapeDataString(_bucket)}/";

        // Make sure the URL belongs to our configured bucket.
        if (!publicUrl.StartsWith(
            publicPrefix,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The image URL does not belong to the configured storage bucket.");
        }

        string objectPath = publicUrl.Substring(publicPrefix.Length);

        if (string.IsNullOrWhiteSpace(objectPath))
        {
            throw new InvalidOperationException("Invalid image path.");
        }

        string bucket = Uri.EscapeDataString(_bucket);

        string encodedPath = string.Join(
            "/",
            objectPath.Split('/').Select(segment =>
                Uri.EscapeDataString(Uri.UnescapeDataString(segment))));

        string deleteUrl =
            $"{_supabaseUrl.TrimEnd('/')}/storage/v1/object/{bucket}/{encodedPath}";

        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            deleteUrl);

        request.Headers.Add("apikey", _secretKey);

        using var response = await _httpClient.SendAsync(request);

        // A missing file is treated as already deleted.
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return;
        }

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Image deletion failed: {error}");
        }
    }

}
