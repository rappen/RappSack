using Azure.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Rappen.XRM.RappSack;
using System.Diagnostics;

namespace Rappen.XRM.RappSackDV;

public class RappSackDVCore : RappSackCore
{
    public string OrganizationUrl { get; }
    public ServiceClient Client { get; }

    public RappSackDVCore(string environmentUrl, TokenCredential credential, IMemoryCache cache, ILogger log, bool useUniqueInstance)
        : base(CreateClient(environmentUrl, credential, cache, log, useUniqueInstance, out var created), new RappSackDVTracerCore(log))
    {
        OrganizationUrl = environmentUrl;
        Client = created;
    }

    private static ServiceClient CreateClient(string environmentUrl, TokenCredential credential, IMemoryCache cache, ILogger log, bool useUniqueInstance, out ServiceClient created)
    {
        async Task<string> TokenProviderAsync(string _)
        {
            var key = $"dv-token::{environmentUrl}";
            var token = await cache.GetOrCreateAsync(key, async ce =>
            {
                ce.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(50);
                var scope = $"{environmentUrl}/.default";
                var at = await credential.GetTokenAsync(new TokenRequestContext([scope]), CancellationToken.None);
                return at;
            });
            return token.Token;
        }

        created = new ServiceClient(new Uri(environmentUrl), TokenProviderAsync, useUniqueInstance, log);
        return created;
    }

    // Optional helper to run shell on Windows Kudu only
    public void Cmd(string args, string? folder = null)
    {
        Trace($"Cmd: {args}");
        if (!string.IsNullOrEmpty(folder))
        {
            args = $"/c cd /d {folder} && {args}";
            Trace($"     in folder: {folder}");
        }

        var process = new Process
        {
            StartInfo =
            {
                FileName = "cmd.exe",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        process.Start();
        process.WaitForExit();
        var result = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        Trace(result);
        if (!string.IsNullOrEmpty(error))
        {
            Trace($"Error:{Environment.NewLine}{error}");
        }
        Trace("Cmd: Called");
    }
}