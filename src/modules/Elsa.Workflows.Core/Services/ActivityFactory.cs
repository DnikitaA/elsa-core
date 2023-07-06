using System.Text.Json;
using Elsa.Extensions;
using Elsa.Workflows.Core.Contracts;
using Elsa.Workflows.Core.Models;
using Humanizer;

namespace Elsa.Workflows.Core.Services;

/// <inheritdoc />
public class ActivityFactory : IActivityFactory
{
    /// <inheritdoc />
    public IActivity Create(Type type, ActivityConstructorContext context)
    {
        // Backwards compatibility for older JSON schemas.
        var canStartWorkflow = GetBoolean(context.Element, "canStartWorkflow");
        var runAsynchronously = GetBoolean(context.Element, "runAsynchronously");
        var activity = (IActivity)context.Element.Deserialize(type, context.SerializerOptions)!;

        activity.SetCanStartWorkflow(canStartWorkflow);
        activity.SetRunAsynchronously(runAsynchronously);

        return activity;
    }

    private static bool GetBoolean(JsonElement element, string propertyName)
    {
        var propertyNames = new[] { propertyName.Camelize(), propertyName.Pascalize() };

        foreach (var name in propertyNames)
        {
            if (element.TryGetProperty("customProperties", out var customPropertyElement))
            {
                if (customPropertyElement.TryGetProperty(name, out var canStartWorkflowElement))
                    return canStartWorkflowElement.GetBoolean();
            }

            if (element.TryGetProperty(propertyName.Camelize(), out var property) && property.GetBoolean())
                return true;
        }

        return false;
    }
}