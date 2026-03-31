//using MMLib.SwaggerForOcelot.DependencyInjection;
//using Ocelot.DependencyInjection;
//using Ocelot.Middleware;

//var builder = WebApplication.CreateBuilder(args);

//var routes = "Routes";

//builder.Configuration.AddOcelotWithSwaggerSupport(options =>
//{
//    options.Folder = routes;
//});

//builder.Services.AddOcelot(builder.Configuration);
//builder.Services
//// Add services to the container.


//builder.Services.AddControllers();
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);


var routes = "Routes";

builder.Configuration.AddOcelotWithSwaggerSupport(options =>
{
    options.Folder = routes;
});

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);
// Add services to the container.
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// Configuration de l'interface Swagger unifiée
// Cela permet de voir la doc de tous les microservices sur la passerelle
app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs";
});

// Activation du middleware Ocelot
// L'utilisation de .Wait() est recommandée par votre cours pour s'assurer qu'Ocelot est prêt
app.UseOcelot().Wait();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();