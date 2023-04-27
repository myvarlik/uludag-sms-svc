using EasyNetQ;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using uludag_sms_svc;
using uludag_sms_svc.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SMSStoreDatabaseSettings>(builder.Configuration.GetSection("SMSStoreDatabase"));

// Add services to the container.
builder.Services.AddSingleton<SMSService>();

string rabbitmqConnectionString = "host=10.245.195.44:5672;virtualhost=/;username=myvarlik;password=celeron504";
var bus = RabbitHutch.CreateBus(rabbitmqConnectionString);

// Kuyruk oluþtur
var queue = bus.Advanced.QueueDeclare("sms-gonder");
// Kuyruða abone ol
bus.Advanced.Consume(queue, (body, properties, info) =>
{
    var message = Encoding.UTF8.GetString(body.Span);
    SMSModel mail = System.Text.Json.JsonSerializer.Deserialize<SMSModel>(message);
    SMSService.TekliGonder(mail);
    return Task.CompletedTask;
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/apirun", () => "Api Runs");

app.MapPost("/list", ResponseModel ([FromBody] ListModel listModel, SMSService smsService) => smsService.Get(listModel));
app.MapGet("/send", ResponseModel (SMSService smsService) => smsService.Gonder());
app.MapDelete("/", ResponseModel ([FromQuery] string id, SMSService smsService) => smsService.Remove(id));
app.MapPost("/", ResponseModel ([FromBody] SMSModel smsModel, SMSService smsService) => smsService.Create(smsModel));

app.Run();