using Api.Configs;
using Api.Mapper;
using Api.Middlewares;
using Api.Services;
using AspNetCoreRateLimit;
using DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Api;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var authSection = builder.Configuration.GetSection(AuthConfig.Position);
        var authConfig = authSection.Get<AuthConfig>();

        builder.Services.Configure<AuthConfig>(authSection);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = "������� ����� ������������",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme,
                        },
                        Scheme = "oauth2",
                        Name = JwtBearerDefaults.AuthenticationScheme,
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                },
            });

            c.SwaggerDoc("Auth", new OpenApiInfo { Title = "Auth" });
            c.SwaggerDoc("Api", new OpenApiInfo { Title = "Api" });
        });

        builder.Services.AddOptions();
        builder.Services.AddMemoryCache();
        builder.Services.Configure<ClientRateLimitOptions>(builder.Configuration.GetSection("ClientRateLimiting"));
        builder.Services.Configure<ClientRateLimitPolicies>(
            builder.Configuration.GetSection("ClientRateLimitPolicies"));
        builder.Services.AddInMemoryRateLimiting();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly);
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<LinkGeneratorService>();
        builder.Services.AddTransient<AttachService>();
        builder.Services.AddScoped<PostService>();

        builder.Services.AddAuthentication(o => { o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authConfig?.Issuer,
                    ValidateAudience = true,
                    ValidAudience = authConfig?.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = authConfig?.SymmetricSecurityKey(),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("ValidAccessToken", p =>
            {
                p.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                p.RequireAuthenticatedUser();
            });

        builder.Services.AddDbContext<DataContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql"));
        });

        var app = builder.Build();

        using (var serviceScope = ((IApplicationBuilder)app).ApplicationServices.GetService<IServiceScopeFactory>()
               ?.CreateScope())
        {
            if (serviceScope != null)
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<DataContext>();
                context.Database.Migrate();
            }
        }

        //if (app.Environment.IsDevelopment())
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("Api/swagger.json", "Api");
            c.SwaggerEndpoint("Auth/swagger.json", "Auth");
        });

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseClientRateLimiting();

        app.UseAuthorization();
        app.UseTokenValidator();
        app.UseGlobalErrorWrapper();
        app.MapControllers();

        app.Run();
    }
}