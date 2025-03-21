using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;
using Wingman.Api.Core.Middlewares;
using Wingman.Api.Core.Services;
using Wingman.Api.Core.Services.Interfaces;
using Wingman.Api.Features.Aircrafts.Repositories;
using Wingman.Api.Features.Aircrafts.Repositories.Interfaces;
using Wingman.Api.Features.Aircrafts.Services;
using Wingman.Api.Features.Aircrafts.Services.Interfaces;
using Wingman.Api.Features.Auth.Repositories;
using Wingman.Api.Features.Auth.Repositories.Interfaces;
using Wingman.Api.Features.Auth.Services;
using Wingman.Api.Features.Auth.Services.Interfaces;
using Wingman.Api.Features.Flights.Repositories;
using Wingman.Api.Features.Flights.Repositories.Interfaces;
using Wingman.Api.Features.Flights.Services;
using Wingman.Api.Features.Flights.Services.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers(config =>
{
    AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddFluentValidationAutoValidation();
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

#region Core
builder.Services.AddSingleton<IDbConnectionService, DbConnectionService>();
#endregion

#region Auth
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>(); // TODO(serafa.leo): Confirm if repositories should be scoped or singletons
#endregion

#region Flights
builder.Services.AddScoped<IFlightsService, FlightsService>();
builder.Services.AddScoped<IFlightsRepository, FlightsRepository>();
#endregion

#region Aircrafts
builder.Services.AddScoped<IAircraftsService, AircraftsService>();
builder.Services.AddScoped<IAircraftsRepository, AircraftsRepository>();
#endregion

#endregion

WebApplication app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
