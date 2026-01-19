using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CK.Taghelpers.ViewComponents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace CK.TagHelpers.Tests.Views;

public abstract class RazorViewTestBase
{
    private static readonly Lazy<IServiceProvider> ServiceProvider = new(BuildServiceProvider);

    protected async Task<string> RenderViewAsync<TModel>(string viewPath, TModel model)
    {
        using var scope = ServiceProvider.Value.CreateScope();
        var services = scope.ServiceProvider;

        var httpContext = new DefaultHttpContext { RequestServices = services };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        var viewEngine = services.GetRequiredService<IRazorViewEngine>();
        var tempDataProvider = services.GetRequiredService<ITempDataProvider>();
        var metadataProvider = services.GetRequiredService<IModelMetadataProvider>();

        var viewResult = FindView(viewEngine, actionContext, viewPath, "Components/DynamicEditor/Default");

        var viewData = new ViewDataDictionary<TModel>(metadataProvider, new ModelStateDictionary())
        {
            Model = model
        };

        var tempData = new TempDataDictionary(httpContext, tempDataProvider);
        await using var writer = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            viewResult.View!,
            viewData,
            tempData,
            writer,
            new HtmlHelperOptions());

        await viewResult.View!.RenderAsync(viewContext);
        return writer.ToString();
    }

    private static ViewEngineResult FindView(
        IRazorViewEngine viewEngine,
        ActionContext actionContext,
        string viewPath,
        string viewName)
    {
        var getViewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: true);
        if (getViewResult.Success)
        {
            return getViewResult;
        }

        var findViewResult = viewEngine.FindView(actionContext, viewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult;
        }

        var searched = string.Join(
            Environment.NewLine,
            (getViewResult.SearchedLocations ?? Array.Empty<string>())
            .Concat(findViewResult.SearchedLocations ?? Array.Empty<string>()));

        throw new InvalidOperationException(
            $"View '{viewPath}' not found. Searched locations:{Environment.NewLine}{searched}");
    }

    private static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        var environment = CreateHostEnvironment();

        services.AddSingleton<IWebHostEnvironment>(environment);
        services.AddSingleton<IHostEnvironment>(environment);
        services.AddLogging();
        services.AddSingleton<DiagnosticListener>(_ => new DiagnosticListener("Microsoft.AspNetCore"));
        services.AddSingleton<DiagnosticSource>(sp => sp.GetRequiredService<DiagnosticListener>());

        services
            .AddMvc()
            .AddApplicationPart(typeof(DynamicEditorViewComponent).Assembly);

        services.AddSingleton<ITempDataProvider, InMemoryTempDataProvider>();

        return services.BuildServiceProvider();
    }

    private static IWebHostEnvironment CreateHostEnvironment()
    {
        var contentRoot = Directory.GetCurrentDirectory();
        var webRoot = Path.Combine(contentRoot, "wwwroot");
        IFileProvider webRootProvider = Directory.Exists(webRoot)
            ? new PhysicalFileProvider(webRoot)
            : new NullFileProvider();

        return new TestWebHostEnvironment
        {
            ApplicationName = "CK.TagHelpers.Tests",
            EnvironmentName = Environments.Development,
            ContentRootPath = contentRoot,
            ContentRootFileProvider = new PhysicalFileProvider(contentRoot),
            WebRootPath = webRoot,
            WebRootFileProvider = webRootProvider
        };
    }

    private sealed class InMemoryTempDataProvider : ITempDataProvider
    {
        private readonly Dictionary<string, object?> _data = new();

        public IDictionary<string, object?> LoadTempData(HttpContext context)
        {
            return new Dictionary<string, object?>(_data);
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
            _data.Clear();
            foreach (var entry in values)
            {
                _data[entry.Key] = entry.Value;
            }
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = string.Empty;
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
