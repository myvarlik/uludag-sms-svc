using MesajPaneli.Business;
using MesajPaneli.Models;
using MesajPaneli.Models.JsonPostModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using uludag_sms_svc;
using uludag_sms_svc.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SMSStoreDatabaseSettings>(
    builder.Configuration.GetSection("SMSStoreDatabase"));

// Add services to the container.
builder.Services.AddSingleton<SMSService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/apirun", () => "Api Run");

app.MapPost("/list", async Task<List<SMSModel>> ([FromBody] ListModel listModel, SMSService smsService) => await smsService.GetAsync(listModel));

app.MapGet("/send", (SMSService smsService) => smsService.Gonder());

app.MapPost("/", async ([FromBody] SMSModel smsModel, SMSService smsService) =>
{
    await smsService.CreateAsync(smsModel);
});

app.MapPut("/", async ([FromBody] SMSModel smsModel, SMSService smsService) =>
{
    await smsService.UpdateAsync(smsModel.id, smsModel);
});

app.Run();