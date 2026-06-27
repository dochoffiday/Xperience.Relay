using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Xperience.Relay.Hosting;

/// <summary>
/// Rejects requests that don't present the configured API key in <see cref="RelayHostingOptions.ApiKeyHeaderName"/>.
/// Compares using a fixed-time check so response timing can't be used to brute-force the key.
/// </summary>
public class RelayApiKeyEndpointFilter : IEndpointFilter
{
    private readonly RelayHostingOptions _options;

    public RelayApiKeyEndpointFilter(IOptions<RelayHostingOptions> options)
    {
        _options = options.Value;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            throw new InvalidOperationException("RelayHostingOptions.ApiKey must be configured before relay endpoints can be mapped.");
        }

        var providedKey = context.HttpContext.Request.Headers[_options.ApiKeyHeaderName].ToString();
        if (!IsMatch(providedKey, _options.ApiKey))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }

    private static bool IsMatch(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        // Hash both sides first so the comparison is fixed-length regardless of the provided key's
        // length -- FixedTimeEquals alone still leaks length via early-exit-free but differently
        // sized inputs in some implementations, so equalizing length removes that signal too.
        var providedHash = SHA256.HashData(providedBytes);
        var expectedHash = SHA256.HashData(expectedBytes);
        return CryptographicOperations.FixedTimeEquals(providedHash, expectedHash);
    }
}
