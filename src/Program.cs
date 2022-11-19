using Microsoft.AspNetCore.Builder;
using TodoApi.Models;
using Microsoft.EntityFrameworkCore;
using TodoApi.Services;
using ILogger = TodoApi.Services.ILogger; // We need this as .NET also has a built in interface called ILogger

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Registers TodoContext DBContext into the container with TodoContext as key.
// We don't use an interface type as key here (it would not have any benefit).
builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));

builder.Services.AddControllers();


#region Register our custom services

// #3.1 REGISTER MAPPINGS
// We can register mappings three different ways
// * AddSingleton: returns the same instance for all resolutions
// * AddTransient: returns a different instance for different resolutions
// * AddScoped: returns the same instance for the same scope
//   (one web api request is served within the context of the same scope)

// Registers an ILogger->Logger mapping, as singleton.
// Later at resolution, we will get a Logger instace when we ask for an ILogger implementation
builder.Services.AddSingleton<ILogger, Logger>();

// Registers an INotificationService->NotificationService mapping, as transient.
// Later at resolution, we will get a NotificationService instace when we ask for an INotificationService implementation
builder.Services.AddTransient<INotificationService, NotificationService>();

// Registers an IContactRepository->ContactRepository mapping, as scoped.
// Later at resolution, we will get a ContactRepository instace when we ask for an IContactRepository implementation
builder.Services.AddScoped<IContactRepository, ContactRepository>();

/*
EMailSender will need to be instantiated by the container when resolving IEMailSender, and the constructor
parameters must be specified appropriately. The logger parameter is completely "OK", and the container can
resolve it based on the ILogger-> Logger container mapping registration. However, there is no way to find out
the value of the smtpAddress parameter. To solve this problem, ASP.NET Core proposes an "options" mechanism
for the framework, which allows us to retrieve the value from some configuration. Covering the "options" topic
would be a far-reaching thread for us, so for simplification we applied another approach. The AddSingleton
(and other Add ... operations) have an overload in which we can specify a lambda expression. This lambda is
called by the container later at the resolve step (that is, when we ask the container for an IEMailSender
implementation) for each instance. With the help of this lambda we manually create the EMailSender object,
so we have the chance to provide the necessary constructor parameters. In fact, the container is really
"helpful" with us: it provides an IServiceCollection object as the lambda parameter for us (in this example
it's called sp), and based on container registrations we can conveniently resolve types with the help of
the already covered GetRequiredService and GetService calls.
*/
builder.Services.AddSingleton<IEMailSender, EMailSender>(sp => new EMailSender(sp.GetRequiredService<ILogger>(), "smtp.myserver.com"));

#endregion


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
