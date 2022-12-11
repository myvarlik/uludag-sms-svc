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

app.MapGet("/apirun", () => "Api Runs");

app.MapPost("/list", ResponseModel ([FromBody] ListModel listModel, SMSService smsService) => smsService.Get(listModel));
app.MapGet("/send", (SMSService smsService) => smsService.Gonder());
app.MapDelete("/", ([FromQuery] string id, SMSService smsService) => smsService.Remove(id));
app.MapPost("/", ([FromBody] SMSModel smsModel, SMSService smsService) => smsService.Create(smsModel));

app.Run();