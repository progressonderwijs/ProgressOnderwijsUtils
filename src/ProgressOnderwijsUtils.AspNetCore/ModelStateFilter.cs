using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ProgressOnderwijsUtils.AspNetCore;

public sealed class ModelStateFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context) { }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid) {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}
