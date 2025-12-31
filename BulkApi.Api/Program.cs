using BulkApi.Application.Receipts;
using BulkApi.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers / Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR（Application側のAssemblyをスキャン）
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateReceiptCommand).Assembly));

// EF Core MySQL（Build前に登録する）
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseMySql(cs, new MySqlServerVersion(new Version(8, 0, 36)),
        mySql => mySql.EnableRetryOnFailure());
});

// HealthChecks（これが無いと MapHealthChecks が動かない）
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapControllers();

app.Run();
