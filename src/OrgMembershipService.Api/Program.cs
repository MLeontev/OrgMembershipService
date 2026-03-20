using OrgMembershipService.Api.Extensions;
using OrgMembershipService.Api.Middlewares;
using OrgMembershipService.Application;
using OrgMembershipService.Infrastructure;
using System.Reflection;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OrgMembershipService API",
        Version = "v1",
        Description = "API для управления пользователями, членством в организациях и проверкой прав доступа"
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT access токен без префикса Bearer"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document), []
        }
    });

    var apiXmlPath = Path.Combine(
        AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    
    if (File.Exists(apiXmlPath))
        options.IncludeXmlComments(apiXmlPath, includeControllerXmlComments: true);

    var applicationXmlPath = Path.Combine(
        AppContext.BaseDirectory,
        $"{typeof(OrgMembershipService.Application.DependencyInjection).Assembly.GetName().Name}.xml");
    
    if (File.Exists(applicationXmlPath))
        options.IncludeXmlComments(applicationXmlPath);
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddUseCases();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

var swaggerBasePath = "/api/users";

app.UseSwagger(options =>
{
    options.PreSerializeFilters.Add((swaggerDocument, httpRequest) =>
    {
        swaggerDocument.Servers =
        [
            new OpenApiServer
            {
                Url = swaggerBasePath
            }
        ];
    });
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("./v1/swagger.json", "OrgMembershipService API v1");
});

app.MapOpenApi();

await app.ApplyMigrations();
await app.SeedDatabase();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
