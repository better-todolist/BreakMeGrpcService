using BreakMeGrpcService;
using BreakMeGrpcService.Local;
using BreakMeGrpcService.Services;
using Vanara.PInvoke;

FileManager.init();
//var windows = User32.GetForegroundWindow();
//var v = new WindowsInfo(BreakMe.ObserveMode.TitleName, windows);

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();


var app = builder.Build();
app.UseHttpLogging();

// Configure the HTTP request pipeline.
var grpcBuilder = app.MapGrpcService<BreakMeRpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


app.Run();
