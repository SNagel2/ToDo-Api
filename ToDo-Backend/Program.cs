using ToDo_Backend.Interfaces;
using ToDo_Backend.Repositories;
using ToDo_Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Services
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register the in-memory repository as a singleton so state survives the
// lifetime of the process (across multiple requests).
builder.Services.AddSingleton<IToDoRepository, InMemoryToDoRepository>();
builder.Services.AddScoped<IToDoService, ToDoService>();

// ---------------------------------------------------------------------------
// CORS – allow any origin for demo purposes
// ---------------------------------------------------------------------------
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();