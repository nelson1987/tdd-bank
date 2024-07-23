using Monetary.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCore();

var app = builder.Build();
app.UseCore();
app.Run();