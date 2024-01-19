using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TestTemplate9.Api.Helpers
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelName = bindingContext.ModelName;
            var elementType = bindingContext.ModelMetadata.ModelType.GetGenericArguments()[0];
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                var emptyArray = System.Array.CreateInstance(elementType, 0);
                bindingContext.Result = ModelBindingResult.Success(emptyArray);
                return Task.CompletedTask;
            }
            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
            var modelValue = valueProviderResult.FirstValue;
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
            if (string.IsNullOrEmpty(modelValue))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }
            var converter = TypeDescriptor.GetConverter(elementType);
            object[] convertedValues = null;
            try
            {
                convertedValues = modelValue
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(mv => converter.ConvertFrom(mv.Trim()))
                    .ToArray();
            }
            catch (Exception /*InvalidPropertyMappingException*/ ex)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                bindingContext.ModelState.Root.ValidationState = ModelValidationState.Invalid;
                bindingContext.ModelState.AddModelError(string.Empty, ex.Message);
                return Task.CompletedTask;
            }
            var valuesArray = System.Array.CreateInstance(elementType, convertedValues.Length);
            convertedValues.CopyTo(valuesArray, 0);
            bindingContext.Result = ModelBindingResult.Success(valuesArray);
            return Task.CompletedTask;
        }
    }
}
