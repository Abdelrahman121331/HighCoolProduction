using ERP.Application.Common.Exceptions;
using ERP.Application.MasterData.Items;
using ERP.Application.MasterData.UomConversions;
using FluentValidation;
using FluentValidation.Results;

namespace ERP.Api.Endpoints;

public static class ItemMasterDataEndpoints
{
    public static IEndpointRouteBuilder MapItemMasterDataEndpoints(this IEndpointRouteBuilder app)
    {
        var items = app.MapGroup("/api/items");
        items.MapGet("/", ListItemsAsync);
        items.MapGet("/{id:guid}", GetItemAsync);
        items.MapPost("/", CreateItemAsync);
        items.MapPut("/{id:guid}", UpdateItemAsync);
        items.MapPost("/{id:guid}/deactivate", DeactivateItemAsync);

        var conversions = app.MapGroup("/api/uom-conversions");
        conversions.MapGet("/", ListUomConversionsAsync);
        conversions.MapGet("/{id:guid}", GetUomConversionAsync);
        conversions.MapPost("/", CreateUomConversionAsync);
        conversions.MapPut("/{id:guid}", UpdateUomConversionAsync);
        conversions.MapPost("/{id:guid}/deactivate", DeactivateUomConversionAsync);

        return app;
    }

    private static async Task<IResult> ListItemsAsync(
        string? search,
        bool? isActive,
        IItemService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ListAsync(new ItemListQuery(search, isActive), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetItemAsync(Guid id, IItemService service, CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> CreateItemAsync(
        UpsertItemRequest request,
        IValidator<UpsertItemRequest> validator,
        IItemService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        return await HandleValidatedCreateAsync(
            request,
            validator,
            () => service.CreateAsync(request, GetActor(context), cancellationToken));
    }

    private static async Task<IResult> UpdateItemAsync(
        Guid id,
        UpsertItemRequest request,
        IValidator<UpsertItemRequest> validator,
        IItemService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        return await HandleValidatedUpdateAsync(
            request,
            validator,
            () => service.UpdateAsync(id, request, GetActor(context), cancellationToken));
    }

    private static async Task<IResult> DeactivateItemAsync(
        Guid id,
        IItemService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.DeactivateAsync(id, GetActor(context), cancellationToken);
        return result ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> ListUomConversionsAsync(
        bool? isActive,
        string? search,
        IUomConversionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ListAsync(
            new UomConversionListQuery(isActive, search),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetUomConversionAsync(
        Guid id,
        IUomConversionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> CreateUomConversionAsync(
        UpsertUomConversionRequest request,
        IValidator<UpsertUomConversionRequest> validator,
        IUomConversionService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        return await HandleValidatedCreateAsync(
            request,
            validator,
            () => service.CreateAsync(request, GetActor(context), cancellationToken));
    }

    private static async Task<IResult> UpdateUomConversionAsync(
        Guid id,
        UpsertUomConversionRequest request,
        IValidator<UpsertUomConversionRequest> validator,
        IUomConversionService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        return await HandleValidatedUpdateAsync(
            request,
            validator,
            () => service.UpdateAsync(id, request, GetActor(context), cancellationToken));
    }

    private static async Task<IResult> DeactivateUomConversionAsync(
        Guid id,
        IUomConversionService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var result = await service.DeactivateAsync(id, GetActor(context), cancellationToken);
        return result ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> HandleValidatedCreateAsync<TRequest, TResult>(
        TRequest request,
        IValidator<TRequest> validator,
        Func<Task<TResult>> handler)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(ToErrors(validationResult));
        }

        try
        {
            var result = await handler();
            return Results.Created(string.Empty, result);
        }
        catch (DuplicateEntityException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
    }

    private static async Task<IResult> HandleValidatedUpdateAsync<TRequest, TResult>(
        TRequest request,
        IValidator<TRequest> validator,
        Func<Task<TResult?>> handler)
        where TResult : class
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(ToErrors(validationResult));
        }

        try
        {
            var result = await handler();
            return result is null ? Results.NotFound() : Results.Ok(result);
        }
        catch (DuplicateEntityException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { message = exception.Message });
        }
    }

    private static string GetActor(HttpContext context)
    {
        return context.User.Identity?.Name ?? "system";
    }

    private static Dictionary<string, string[]> ToErrors(ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
    }
}
