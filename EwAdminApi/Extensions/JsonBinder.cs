using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EwAdminApi.Extensions;

public class JsonBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var json = bindingContext.HttpContext.Request.Body;

        using var reader = new StreamReader(json);
        var jsonString = await reader.ReadToEndAsync();
        var jsonObject = JsonDocument.Parse(jsonString, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        });

        var targetType = bindingContext.ModelType;
        var model = Activator.CreateInstance(targetType);
        var propertyDict = new Dictionary<string, object?>();

        foreach (var prop in targetType.GetProperties())
        {
            var jsonPropertyName = JsonNamingPolicy.CamelCase.ConvertName(prop.Name).ToLower();

            foreach (var jsonProperty in jsonObject.RootElement.EnumerateObject())
            {
                if (jsonProperty.Name.ToLower() != jsonPropertyName) continue;
                var value = JsonSerializer.Deserialize(jsonProperty.Value.GetRawText(), prop.PropertyType);
                prop.SetValue(model, value);
                propertyDict[jsonPropertyName] = value;
                break;
            }
        }

        bindingContext.HttpContext.Items["NormalizedJsonProperties"] = propertyDict;
        bindingContext.Result = ModelBindingResult.Success(model);
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class JsonBinderAttribute : ModelBinderAttribute
{
    public JsonBinderAttribute() : base(typeof(JsonBinder)) { }
}