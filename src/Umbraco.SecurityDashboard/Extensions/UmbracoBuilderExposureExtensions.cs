using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.SecurityDashboard.Services.Exposure;

namespace Umbraco.SecurityDashboard.Extensions;

public static class UmbracoBuilderExposureExtensions
{
    public static IUmbracoBuilder AddExposureCheck<T>(this IUmbracoBuilder builder)
        where T : class, IExposureCheck
    {
        builder.Services.AddSingleton<IExposureCheck, T>();
        return builder;
    }
}
