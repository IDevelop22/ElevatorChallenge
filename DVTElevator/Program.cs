using BusinessLogic.Models;
using BusinessLogic.Models.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DVTElevator
{
    internal class Program
    {
         static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("Application Starting...");

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ICartSchedule, CartSchedule>();
                    services.AddSingleton<IScheduler, Scheduler>();
                })
                .UseSerilog()
                .Build();

            var schedule = ActivatorUtilities.CreateInstance<CartSchedule>(host.Services);
            var scheduler = ActivatorUtilities.CreateInstance<Scheduler>(host.Services);

            ICollection<Cart> carts = new List<Cart>();
            for (int i = 0; i < 3; i++) { 
                carts.Add(new Cart(i + 1, 0,scheduler.GetSchedule()));
                
            }
            //Initialize Building
            scheduler.Run(carts);


            SendMultipleElevatorRequest(scheduler,Log.Logger);
            var add = Console.ReadLine();
            while (add.ToString()!= "q") {
               

                add = Console.ReadLine();
            };
        }

        static async Task SendMultipleElevatorRequest(Scheduler scheduler,ILogger logger) {
            
            for (int i = 0; i < 5; i++)
            {
             
                await Task.Delay(1000*i);
           
                var cartInstruction = new CartFloorInstructions()
                {
                   // CartId = 2,
                    DestinationFloorNum = i + 10 - (2 * i),
                    LoadingFloorNum = i + 11,
                    FloorInstructionStatus = FloorInstructionStatus.Requested,
                    Passengers = 5+i,

                };

                scheduler.RequestCart(cartInstruction.LoadingFloorNum,cartInstruction.DestinationFloorNum,cartInstruction.Passengers);
                logger.Information("Added Cart Instruction {@ins}", cartInstruction);
            }
            
            

        }
        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }



    }
}