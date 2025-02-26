using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Wingman.Api.Core.DTOs;
using Wingman.Api.Core.Helpers.ExtensionMethods;

namespace Wingman.Api.Core.Middlewares;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly JsonSerializerOptions _jsonOptions;

    public ValidationMiddleware(RequestDelegate next, IOptions<JsonOptions> jsonOptions)
    {
        _next = next;
        _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.ContentType != null && context.Request.ContentType.Contains("application/json"))
        {
            context.Request.EnableBuffering();

            using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                string body = await reader.ReadToEndAsync();

                if (body.IsNotNullOrEmpty())
                {
                    context.Request.Body.Position = 0;

                    Endpoint? endpoint = context.GetEndpoint();

                    if (endpoint is not null)
                    {
                        ControllerActionDescriptor? actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

                        if (actionDescriptor is not null)
                        {
                            List<ValidationResult> failedValidationResults = [];

                            foreach (ParameterDescriptor parameter in actionDescriptor.Parameters)
                            {
                                Type modelType = parameter.ParameterType;
                                Type validatorType = typeof(IValidator<>).MakeGenericType(modelType);

                                IValidator? validatorObj = context.RequestServices.GetService(validatorType) as IValidator;

                                if (validatorObj is not null)
                                {
                                    // NOTE(serafa.leo): Here we have a model with an actual validator for it.

                                    object? model = JsonSerializer.Deserialize(body, modelType, _jsonOptions);

                                    if (model is not null)
                                    {
                                        ValidationResult validationResult = await validatorObj.ValidateAsync(new ValidationContext<object>(model));

                                        if (!validationResult.IsValid)
                                        {
                                            failedValidationResults.Add(validationResult);
                                        }
                                    }
                                }
                            }

                            if (failedValidationResults.IsNotNullOrEmpty())
                            {
                                ApiResponseDto<object> response = new()
                                {
                                    StatusCode = StatusCodes.Status400BadRequest,
                                    Message = "Validation failed.",
                                    Errors = failedValidationResults.SelectMany(r => r.Errors).Select(e => e.ErrorMessage).ToList()
                                };

                                context.Response.StatusCode = response.StatusCode;
                                await context.Response.WriteAsJsonAsync(response);
                                return;
                            }
                        }
                    }
                }
            }
        }

        await _next(context);
    }
}
