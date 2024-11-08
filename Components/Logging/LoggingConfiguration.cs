using Microsoft.AspNetCore.SignalR;
using Serilog;
using Serilog.Sinks.AspNetCore.App.SignalR.Extensions;
using System.Security.Policy;

namespace ForestChurches.Components.Logging
{
    public class LoggingConfiguration
    {
        internal static void ConfigureLogging(IServiceCollection services)
        {
            services.AddSerilog(
                (services, loggingConfiguration) =>
                    loggingConfiguration
                        .MinimumLevel.Information()
                        .WriteTo.SignalR<LoggingHub>(
                            services,
                            (context, message, logEvent) =>
                                context.Clients.All.SendAsync("RecieveEvent", message, logEvent)
                            )
                        .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
                        .WriteTo.SQLite(AppDomain.CurrentDomain.BaseDirectory + @"/logs.db", "system_logs", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
                );
        }
    }
}