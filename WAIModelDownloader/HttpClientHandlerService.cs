using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class HttpClientHandlerService
{
    private readonly HttpClientHandler _handler;
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public HttpClientHandlerService()
    {
        _cookieContainer = new CookieContainer();
        _handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            UseCookies = true,
            UseDefaultCredentials = false
        };

        _client = new HttpClient(_handler);
    }

    public HttpClient HttpClient => _client; // Expose HttpClient through a property

    public async Task<bool> AuthenticateAsync(string loginLink)
    {
        try
        {
            Log("Authenticating with login link: " + loginLink);
            var response = await _client.GetAsync(loginLink);
            LogResponse(response);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError("Error during authentication", ex);
            return false;
        }
    }

    public async Task<string> GetHtmlAsync(string url)
    {
        try
        {
            Log("Fetching HTML content from: " + url);
            var response = await _client.GetAsync(url);
            LogResponse(response);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            LogError("Error fetching HTML content", ex);
            throw;
        }
    }

    public async Task DownloadFileAsync(string url, string outputPath)
    {
        try
        {
            Log("Downloading file from: " + url + " to: " + outputPath);
            var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            LogResponse(response);
            response.EnsureSuccessStatusCode();

            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                await response.Content.CopyToAsync(fs);
            }
        }
        catch (Exception ex)
        {
            LogError("Error downloading file", ex);
            throw;
        }
    }

    private void Log(string message)
    {
        // Replace with your preferred logging method
        Console.WriteLine(message);
    }

    private void LogResponse(HttpResponseMessage response)
    {
        Log("Response: " + response.StatusCode + " - " + response.ReasonPhrase);
        Log("Headers: " + response.Headers);
        if (response.Content != null)
        {
            Log("Content Headers: " + response.Content.Headers);
        }
    }

    private void LogError(string message, Exception ex)
    {
        Log(message + ": " + ex.Message);
    }
}
