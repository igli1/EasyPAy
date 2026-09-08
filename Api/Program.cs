using Application.Dtos;
using Application.Interfaces;
using Application.Services;
using Infrastructure;
using Infrastructure.Excel;
using Infrastructure.Matching;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Reporting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EasyPayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Repositories
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ITechnicianRepository, TechnicianRepository>();
builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();

builder.Services.AddScoped<IExcelReader<RawPersonRow>, ExcelPersonReader>();

builder.Services.AddScoped<IImportReportWriter, CsvImportReportWriter>();
builder.Services.AddScoped<INameMatcher, JaroWinklerNameMatcher>();
builder.Services.AddScoped<IExcelReader<RawWorkOrderRow>, ExcelWorkOrderReader>();
builder.Services.AddScoped<WorkOrderImportService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();