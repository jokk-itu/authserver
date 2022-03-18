using AuthorizationServer;
using AuthorizationServer.Extensions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

//CONFIGURE SERVICE CONTAINER
var services = builder.Services;

var authConfiguration = builder.Configuration.GetSection("Identity").Get<AuthenticationConfiguration>();
services.AddSingleton(authConfiguration);
services.AddDataProtection();

services.AddAuth(authConfiguration);
services.AddSwagger();
services.AddApi();
services.AddDatastore(builder.Configuration);

var app = builder.Build();


//CONFIGURE PIPELINE
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var groupName in provider.ApiVersionDescriptions.Select(v => v.GroupName))
        options.SwaggerEndpoint(
            $"/swagger/{groupName}/swagger.json",
            groupName.ToUpper());
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//MAIN METHOD
app.Run();