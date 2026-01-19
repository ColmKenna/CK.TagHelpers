using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Routing;

namespace CK.TagHelpers.Tests.ViewComponents;

public abstract class ViewComponentTestBase
{
    protected void SetupViewComponentContext(ViewComponent viewComponent)
    {
        var httpContext = new DefaultHttpContext();
        var viewContext = new ViewContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData()
        };

        var viewComponentContext = new ViewComponentContext
        {
            ViewContext = viewContext
        };

        viewComponent.ViewComponentContext = viewComponentContext;
    }

    protected void SetupHttpContextItems(ViewComponent viewComponent, Dictionary<object, object?> items)
    {
        foreach (var item in items)
        {
            viewComponent.HttpContext.Items[item.Key] = item.Value;
        }
    }

    protected void SetupRouteData(ViewComponent viewComponent, Dictionary<string, object?> routeValues)
    {
        foreach (var value in routeValues)
        {
            viewComponent.RouteData.Values[value.Key] = value.Value;
        }
    }

    protected static void AssertViewResultWithModel<TModel>(
        IViewComponentResult result,
        string? expectedViewName = null)
    {
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);

        if (expectedViewName is not null)
        {
            Assert.Equal(expectedViewName, viewResult.ViewName);
        }

        Assert.NotNull(viewResult.ViewData?.Model);
        Assert.IsType<TModel>(viewResult.ViewData.Model);
    }

    protected static TModel GetViewModel<TModel>(IViewComponentResult result)
    {
        var viewResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.NotNull(viewResult.ViewData?.Model);
        return Assert.IsType<TModel>(viewResult.ViewData.Model);
    }
}
