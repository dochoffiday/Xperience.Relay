using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Modules;
using Microsoft.Extensions.DependencyInjection;
using Xperience.Relay.Kentico.Security;

[assembly: AssemblyDiscoverable]
[assembly: RegisterModule(typeof(RelayModule))]

namespace Xperience.Relay.Kentico.Security;

/// <summary>Runs once at application start to make sure the API key table exists.</summary>
internal sealed class RelayModule() : Module("Xperience.Relay")
{
    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        try
        {
            RelayApiKeyInstaller.Install(parameters.Services.GetRequiredService<IInfoProvider<ResourceInfo>>());
        }
        catch (Exception ex)
        {
            // A failed install shouldn't take the whole site down; relay requests will just be rejected.
            parameters.Services.GetService<IEventLogService>()?.LogException("Xperience.Relay", "INSTALL", ex);
        }
    }
}
