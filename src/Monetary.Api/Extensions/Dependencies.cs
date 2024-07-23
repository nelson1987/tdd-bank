using Monetary.Api.Controllers;

namespace Monetary.Api.Extensions;

public static class Dependencies
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwagger();
        return services;
    }

    public static WebApplication UseCore(this WebApplication app)
    {
        app.UseSwaggerApp();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}

public static class SwaggerServices
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        return services.AddEndpointsApiExplorer().AddSwaggerGen();
    }

    public static WebApplication UseSwaggerApp(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        return app;
    }
}