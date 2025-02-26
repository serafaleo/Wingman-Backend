using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
using Wingman.Api.Core.Middlewares;
using Wingman.Api.Core.Services;
using Wingman.Api.Core.Services.Interfaces;
using Wingman.Api.Features.Auth.Repositories;
using Wingman.Api.Features.Auth.Repositories.Interfaces;
using Wingman.Api.Features.Auth.Services;
using Wingman.Api.Features.Auth.Services.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(config =>
{
    AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// NOTE(serafa.leo): This assumes all our validators are in the same assembly as Program.cs
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
        ValidAudience = builder.Configuration["Jwt:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
    };
});

builder.Services.AddAuthorization();

#region Add Services

builder.Services.AddSingleton<IDbConnectionService, DbConnectionService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IUsersService, UsersService>();

#endregion

#region Add Repositories

builder.Services.AddScoped<IUsersRepository, UsersRepository>(); // TODO(serafa.leo): Confirm if repositories should be scoped or singletons

#endregion

WebApplication app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<ValidationMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
