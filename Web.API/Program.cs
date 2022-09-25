using Auth.Application.DTOs;
using Auth.Identity.Services;
using Auth.Infra.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterServices(builder.Configuration);

builder.Services.AddSwagger();

builder.Services.AddAuth(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();

app.UseAuthorization();

app.ApplyMigrations();

app.MapPost("/register", async (IdentityService identityService, UserRegisterRequest user) =>
{
    return await identityService.RegisterUser(user);
})
.AllowAnonymous()
.WithName("Register");

app.MapPost("/login", async (IdentityService identityService, UserLoginRequest user) =>
{
    return await identityService.Login(user);
})
.AllowAnonymous()
.WithName("Login");

app.MapGet("/teste", () =>
{
    return 1;
})
.WithName("Teste");

app.Run();

