using Microsoft.AspNetCore.Mvc;

namespace Common.Extensions;

public static class ControllerExt
{
    public static string? ControllerAction<T>(this IUrlHelper urlHelper, string name, object? arg)
        where T : ControllerBase
    {
        var contentType = typeof(T);
        var method = contentType.GetMethod(name);
        if (method == null)
        {
            return null;
        }

        var controller = contentType.Name.Replace("Controller", string.Empty);
        var action = urlHelper.Action(name, controller, arg);
        return action;
    }
}