using CRUDRestApi.DataBase;
using CRUDRestApi.DataBase.Interfaces;
using CRUDRestApi.Models;
using CRUDRestApi.Service.Interfaces;
using CRUDRestApi.Service;
using NLog;
using NLog.Web;
using System;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);


    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddScoped<IDataBaseOperations, DataBaseOperations>();
    builder.Services.AddScoped<IUserService,UserSerivce>();
    builder.Services.AddScoped<IUserValidatiorService, UserValidationService>();
    builder.Services.AddScoped<DataAccessException, DataAccessException>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
