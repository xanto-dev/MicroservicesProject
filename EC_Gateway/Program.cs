using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ajout de la configuration Ocelot
var routes = "Routes";

builder.Configuration.AddOcelotWithSwaggerSupport(options =>
{
    options.Folder = routes;
});
 
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);
 
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddControllers();


var app = builder.Build();

// Configurer le http pipeline pour la documentation Swagger
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// Configuration de l'interface Swagger unifiéee
app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs";
});

// Activation du middleware Ocelot
app.UseOcelot().Wait();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();