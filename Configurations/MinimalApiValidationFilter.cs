using System.ComponentModel.DataAnnotations;

namespace CaManagement.Api.Configurations;

/// <summary>Data-annotation validation for minimal API JSON bodies (<see cref="IEndpointFilter"/>).</summary>
public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var arg = context.Arguments.OfType<T>().FirstOrDefault();
        if (arg is null)
            return Results.BadRequest();

        var vc = new ValidationContext(arg);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(arg, vc, results, validateAllProperties: true))
        {
            var errors = results
                .Where(r => r.MemberNames.Any())
                .SelectMany(r => r.MemberNames.Select(m => new KeyValuePair<string, string[]>(m, [r.ErrorMessage ?? "Invalid."])))
                .GroupBy(p => p.Key)
                .ToDictionary(g => g.Key, g => g.SelectMany(x => x.Value).ToArray());

            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}
