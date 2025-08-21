using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();



app.MapGet("", async ctx =>
{
    var appOptions = ctx.RequestServices
        .GetRequiredService<IApplicationOptions>();
    await ctx.Response.WriteAsync(appOptions.Identifier);
});

app.MapGet("users/{userId}", async (GetUser request, IQueryDispatcher dispatcher) =>
{
    var result = await dispatcher.DispatchAsync(request);
    return result.ToApiResult();

}).RequireAuthorization("Admin");

app.MapGet("me", async (IQueryDispatcher dispatcher) =>
{
    var result = await dispatcher.DispatchAsync(new GetCurrentUser());
    return result.ToApiResult();
}).RequireAuthorization();

app.MapPost("sign-in", async (SignInPassword cmd, ICommandDispatcher dispatcher) =>
{
    var result = await dispatcher.DispatchAsync(cmd);
    return result.ToApiResult();
});

app.MapPost("create-user", async (CreateUser cmd, ICommandDispatcher dispatcher) =>
{
    var result = await dispatcher.DispatchAsync(cmd);
    return result.ToApiResult(value => Results.Created("identity/me", (object?)value));
});

app.MapPost("access-token/revoke", async (RevokeAccessToken cmd, ICommandDispatcher dispatcher) =>
{
    var result = await dispatcher.DispatchAsync(cmd);
    return result.ToApiResult(value => Results.NoContent());
});

app.MapPost("refresh-tokens/use", async (UseRefreshToken cmd, ICommandDispatcher dispatcher) =>
{
    var result = await dispatcher.DispatchAsync(cmd);
    return result.ToApiResult(value => Results.NoContent());
});

app.MapPost("refresh-tokens/revoke",
    async (RevokeRefreshToken cmd, ICommandDispatcher dispatcher) =>
    {
        var result = await dispatcher.DispatchAsync(cmd);
        return result.ToApiResult(value => Results.NoContent());
    });
public sealed record GetUser(Guid UserId);

public sealed record SignInPassword(string Email, string Password);

public sealed record CreateUser(string Email, string Password);

public sealed record RevokeAccessToken(string AccessToken);

public sealed record UseRefreshToken( string RefreshToken);

public sealed record RevokeRefreshToken(string RefreshToken);

public sealed record GetCurrentUser();
public enum ResultStatus
{
    Success,
    NotFound,
    UnAuthenticated,
    Unauthorized,
    Failure
}

public sealed class Result<T>
{
    public bool IsFailure => Status != ResultStatus.Success;
    public ResultStatus Status { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }

    public static Result<T> Ok(T value) => new() { Status = ResultStatus.Success, Value = value };
    public static Result<T> NotFound(string? error = null) => new() { Status = ResultStatus.NotFound, Error = error };
    public static Result<T> Unauthorized(string? error = null) => new() { Status = ResultStatus.Unauthorized, Error = error };
    public static Result<T> Unauthenticated(string? error = null) => new() { Status = ResultStatus.UnAuthenticated, Error = error };
    public static Result<T> Failure(string? error = null) => new() { Status = ResultStatus.Failure, Error = error };
}


public static class Extensions
{
    /// <summary>
    /// Maps a Result<T> to IResult. Success mapping is extendable via a custom function.
    /// </summary>
    public static IResult ToApiResult<T>(
        this Result<T> result,
        Func<T?, IResult>? successMapper = null)
    {
        switch (result.Status)
        {
            case ResultStatus.Success:
                if (successMapper != null)
                    return successMapper(result.Value);

                // Default mapping
                if (result.Value is null)
                    return Results.NoContent(); // default for empty success
                return Results.Ok(result.Value);

            case ResultStatus.NotFound:
                return Results.NotFound(result.Error);

            case ResultStatus.Unauthorized:
                return Results.Unauthorized();

            case ResultStatus.Failure:
                return Results.BadRequest(result.Error);

            default:
                return Results.Problem("Unknown error", statusCode: 500);
        }
    }
}
public interface IQueryDispatcher
{
    Task<Result> DispatchAsync<T>(T request);
}

public interface ICommandDispatcher
{
    Task<Result> DispatchAsync<T>(T request);
}
 
public interface IApplicationOptions
{
    public string Name { get;  }
    public string Version { get;  }
    public string Description { get;  }
    public string Identifier { get;  }
}
