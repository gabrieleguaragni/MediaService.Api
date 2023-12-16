using FluentValidation;
using MediaService.Business.Abstractions.Kafka;
using MediaService.Business.Abstractions.Services;
using MediaService.Business.Exceptions;
using MediaService.Business.Kafka;
using MediaService.Business.Services;
using MediaService.Validators;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient("AuthApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AuthApiClient:BaseAddress"]!); 
});

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT API", Version = "v1" });

    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});


// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageService, ImageService>();

// Register validators
builder.Services.AddScoped<IValidator<IFormFile>, ImageValidator>();

// Kafka
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddHostedService<KafkaAvatarConsumerService>();
builder.Services.AddHostedService<KafkaPostConsumerService>();

builder.Services.AddCors(options =>
{
    // Default policy 
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin();
        }
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = $"http://{httpReq.Host.Value}/media-api" }
            });
    });
    app.UseSwaggerUI();
}

// Global error handler
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();