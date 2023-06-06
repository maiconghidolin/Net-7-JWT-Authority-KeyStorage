using Infra.EF;
using Infra.Repositories;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Core.Services;
using Core.Validation;
using Common.Enrichers;
using Common.Extensions;
using Serilog;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

{
    var host = builder.Host;

    host.UseSerilog((context, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration));

    var services = builder.Services;

    services.AddControllers();

    services.AddHttpContextAccessor();

    services.AddDbContext<ApiDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

    services.AddMemoryCache();

    services.AddValidatorsFromAssemblyContaining<UserValidator>();

    services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ITokenService, TokenService>();

    services.AddEndpointsApiExplorer();

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Auth", Version = "v1" });
    });

    services.AddJwksManager()
            .UseJwtValidation()
            .PersistKeysToDatabaseStore<ApiDbContext>();

}

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
});

app.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = LogEnricher.EnrichFromRequest);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseExceptionMiddleware();

app.UseJwksDiscovery();

app.MapControllers();

app.Run();