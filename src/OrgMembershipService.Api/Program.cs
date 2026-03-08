using OrgMembershipService.Api.Extensions;
using OrgMembershipService.Application;
using OrgMembershipService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddUseCases();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapOpenApi();

await app.ApplyMigrations();
await app.SeedDatabase();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();