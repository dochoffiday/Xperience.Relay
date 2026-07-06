using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Xperience.Relay.Hosting;

/// <summary>
/// Sets the maximum request body size for relay endpoints, overriding the server-wide default.
/// Configured via <see cref="RelayHostingOptions.MaxRequestBodySizeBytes"/>.
/// </summary>
internal sealed class RelayRequestBodySizeFilter(long? maxBytes) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var feature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (feature is { IsReadOnly: false })
        {
            feature.MaxRequestBodySize = maxBytes;
        }

        return await next(context);
    }
}
