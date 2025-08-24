using System.Reflection;
using DistLib;
using DistLib.WebApi;
using DistLib.Requests.MediaR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PFinance.Services.Identity.Application;
using PFinance.Services.Identity.Application.UserAccount;
using PFinance.Services.Identity.Application.UserAccount.Commands.CreateUser;
using PFinance.Services.Identity.Application.UserAccount.Commands.RevokeAccessToken;
using PFinance.Services.Identity.Application.UserAccount.Commands.RevokeRefreshToken;
using PFinance.Services.Identity.Application.UserAccount.Commands.SignInPassword;
using PFinance.Services.Identity.Application.UserAccount.Commands.UseRefreshToken;
using PFinance.Services.Identity.Common.Enums;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
Assembly[] requestAssemblies = [ApplicationAssembly.Assembly];

builder.Services.AddDistLib(builder.Configuration)
    .AddInMemoryDomainEventDispatcher(requestAssemblies)
    .UseMediaRRequestPipeline(requestAssemblies);
    

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
        .GetRequiredService<IOptions<ApplicationOptions>>();
    await ctx.Response.WriteAsync($"{appOptions.Value.Name} - {appOptions.Value.Version}");
});

app.MapGet("users/{userId}", async (Guid userId, IQueryDispatcher dispatcher) =>
{
    var result = await dispatcher.DispatchAsync(new GetUserById(userId));
    return result.ToApiResult<UserDto>();

}).RequireAuthorization("Admin");

app.MapGet("me", async (IQueryDispatcher dispatcher) =>
{
    var result = await dispatcher.DispatchAsync(new GetCurrentUser());
    return result.ToApiResult<UserDto>();
}).RequireAuthorization();

app.MapPost("sign-in", async ([FromBody] SignInPassword cmd, ICommandDispatcher dispatcher,
    CancellationToken ct) =>
{
    var result = await dispatcher.DispatchAsync<SignInPassword, Result<AuthDto>>(cmd, ct);
    return result.ToApiResult<AuthDto>(Results.Ok);
});

app.MapPost("create-user", async ([FromBody] CreateUser cmd, ICommandDispatcher dispatcher,
    CancellationToken ct) =>
{
    var userId = Guid.NewGuid();
    var result = await dispatcher.DispatchAsync<CreateUser, Result>(
        cmd.Bind(u => u.UserId, userId), ct);
    return result.ToApiResult(() => Results.Created("identity/me", (object?)userId));
});

app.MapPost("access-token/revoke", async ([FromBody] RevokeAccessToken cmd, ICommandDispatcher dispatcher, 
    CancellationToken ct) =>
{
    var result = await dispatcher.DispatchAsync<RevokeAccessToken, Result>(cmd, ct);
    return result.ToApiResult(Results.NoContent);
});

app.MapPost("refresh-tokens/use", async ([FromBody] UseRefreshToken cmd, ICommandDispatcher dispatcher, 
    CancellationToken ct) =>
{
    var result = await dispatcher.DispatchAsync<UseRefreshToken, Result<AuthDto>>(cmd, ct);
    return result.ToApiResult(Results.Ok);
});

app.MapPost("refresh-tokens/revoke",
    async ([FromBody] RevokeRefreshToken cmd, ICommandDispatcher dispatcher, 
        CancellationToken ct) =>
    {
        var result = await dispatcher.DispatchAsync<RevokeRefreshToken, Result>(cmd, ct);
        return result.ToApiResult(Results.NoContent);
    });

app.Run();


public sealed record GetUserById(Guid UserId) : IQuery<Result<UserDto>>;

public sealed record GetCurrentUser() : IQuery<Result<UserDto>>;


