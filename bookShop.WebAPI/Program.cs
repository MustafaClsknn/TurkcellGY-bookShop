using bookShop.Business.Abstract;
using bookShop.Business.Concrete;
using bookShop.Business.MapperProfile;
using bookShop.DataAccess.Abstract;
using bookShop.DataAccess.Concrete.EntityFramework;
using bookShop.DataAccess.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();//.AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IWriterService, WriterService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookRepository, EFBookRepository>();
builder.Services.AddScoped<IWriterRepository, EFWriterRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IPublisherRepository, EFPublisherRepository>();
builder.Services.AddScoped<IUserRepository, EFUserRepository>();
var connectionString = builder.Configuration.GetConnectionString("db");
builder.Services.AddDbContext<bookShopDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = "Data Source=MCLSKN ;Initial Catalog=DistributedCacheDb;Integrated Security=True;";
    options.SchemaName = "dbo";
    options.TableName = "TestCache";

});

builder.Services.AddAutoMapper(typeof(MapProfile));

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Burasý çok ama çok gizli bir ifade"));
var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.SaveToken = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateActor = true,
            ValidateIssuer = true,
            ValidateAudience = true,

            ValidIssuer = "turkcell.com.tr",
            ValidAudience = "turkcell.com",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key
        };
    });

var app = builder.Build();
app.Lifetime.ApplicationStarted.Register( async () =>
{
    var scope = app.Services.CreateScope();
    var _categoryService = scope.ServiceProvider.GetService<ICategoryService>();
    var _publisherService = scope.ServiceProvider.GetService<IPublisherService>();
    var _writerService = scope.ServiceProvider.GetService<IWriterService>();
    var _cache = scope.ServiceProvider.GetService<IDistributedCache>();
    var categories = await _categoryService.GetAllEntitiesAsyncDto();
    var publishers = await _publisherService.GetAllEntitiesAsyncDto();
    var writers = await _writerService.GetAllEntitiesAsyncDto();
    var json = JsonConvert.SerializeObject(categories);
    var json2 = JsonConvert.SerializeObject(publishers);
    var json3 = JsonConvert.SerializeObject(writers);
    var option = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddDays(1));
    _cache.SetString("categories", json, option);
    _cache.SetString("publishers", json2, option);
    _cache.SetString("writers", json3, option);

});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

