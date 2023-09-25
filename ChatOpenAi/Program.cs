using ChatOpenAi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<Setting>(builder.Configuration.GetSection(nameof(Setting)));
builder.Services.AddSingleton<Setting>();
string urlCors = builder.Configuration.GetSection("UrlCors").Get<string>() ?? string.Empty;
var arrurlCors = urlCors.Split(',');
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyPolicy",
        policy =>
        {
            policy.WithOrigins(arrurlCors).AllowAnyHeader().AllowAnyMethod();
        });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

string ResponseHeaders = builder.Configuration.GetSection("Headers").Get<string>() ?? string.Empty;
var arrResponseHeaders = ResponseHeaders.Split(',');

app.UseCors("MyPolicy");
app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    foreach (var item in arrResponseHeaders)
    {
        context.Response.Headers.Remove(item);
    }
    await next(context);
});


app.UseAuthorization();

app.MapControllers();

app.Run();