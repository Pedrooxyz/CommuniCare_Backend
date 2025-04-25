using CommuniCare;
using CommuniCare.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);


const string DevClient = "DevClient";

builder.Services.AddCors(options =>
{
    options.AddPolicy(DevClient, p =>
    {
        p.WithOrigins("http://localhost:3000")
         .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
         .WithHeaders("content-type", "authorization")
         .AllowCredentials();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});


// ? Configurar licen�a da QuestPDF antes de adicionar outros servi�os
var questPdfLicense = builder.Configuration["QuestPDF:LicenseKey"];
if (!string.IsNullOrEmpty(questPdfLicense))
{
    QuestPDF.Settings.License = (QuestPDF.Infrastructure.LicenseType)Enum.Parse(typeof(QuestPDF.Infrastructure.LicenseType), questPdfLicense);
}
else
{
    // Caso n�o tenha chave de licen�a, use a licen�a comunit�ria
    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
}

// 1. Adicionar configura��o do JWT
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);

var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

// 2. Configurar autentica��o JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// 3. Servi�os essenciais
builder.Services.AddSingleton<EmailService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CommuniCare API", Version = "v1" });

    // Configurar o bot�o Authorize no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Introduz o token abaixo:",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});


builder.Services.AddDbContext<CommuniCareContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConStr")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommuniCareContext>();

    // Adicionar TipoContacto "email" e "numTelemovel" se não existirem
    if (!db.TipoContactos.Any())
        db.TipoContactos.AddRange(
            new TipoContacto { DescContacto = "email" },
            new TipoContacto { DescContacto = "numTelemovel" }
        );

    // Adicionar TipoUtilizador "utilizador" e "administrador" se não existirem
    if (!db.TipoUtilizadors.Any())
        db.TipoUtilizadors.AddRange(
            new TipoUtilizador { DescTU = "utilizador" },
            new TipoUtilizador { DescTU = "administrador" }
        );

    // Adicionar o código postal padrão se não existir
    var cp = db.Cps.FirstOrDefault(c => c.CPostal == "0000-000");
    if (cp is null)
        db.Cps.Add(new Cp { CPostal = "0000-000", Localidade = "000000" });

    db.SaveChanges();

    // Adicionar morada padrão se não existir
    var defaultAddress = db.Morada.FirstOrDefault(m => m.Rua == "A definir");
    if (defaultAddress is null)
    {
        defaultAddress = new Morada
        {
            Rua = "A definir",
            NumPorta = null,
            CPostal = "0000-000"
        };
        db.Morada.Add(defaultAddress);
        db.SaveChanges();
    }

    // Criar utilizador de tipo "utilizador" (TipoUtilizadorId = 1) com UtilizadorId = 1
    var userTipoId = db.TipoUtilizadors.First(t => t.DescTU == "utilizador").TipoUtilizadorId;
    var normalUser = db.Utilizadores.FirstOrDefault(u => u.UtilizadorId == 1);

    if (normalUser is null)
    {
        normalUser = new Utilizador
        {
            UtilizadorId = 1,  // Definir o ID explicitamente
            NomeUtilizador = "utilizador",
            Password = BCrypt.Net.BCrypt.HashPassword("string"), // Senha encriptada
            NumCares = 0,
            TipoUtilizadorId = userTipoId,
            MoradaId = defaultAddress.MoradaId
        };
        db.Utilizadores.Add(normalUser);
        db.SaveChanges();

        // Adicionar contacto "utilizador@teste.pt" para o utilizador comum
        var emailTipoId = db.TipoContactos.First(t => t.DescContacto == "email").TipoContactoId;
        db.Contactos.Add(new Contacto
        {
            NumContacto = "utilizador@teste.pt",
            UtilizadorId = normalUser.UtilizadorId,
            TipoContactoId = emailTipoId
        });
        db.SaveChanges();
    }

    // Criar utilizador de tipo "administrador" (TipoUtilizadorId = 2) com UtilizadorId = 2
    var adminTipoId = db.TipoUtilizadors.First(t => t.DescTU == "administrador").TipoUtilizadorId;
    var adminUser = db.Utilizadores.FirstOrDefault(u => u.UtilizadorId == 2);

    if (adminUser is null)
    {
        adminUser = new Utilizador
        {
            UtilizadorId = 2,  // Definir o ID explicitamente
            NomeUtilizador = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("string"), // Senha encriptada
            NumCares = 0,
            TipoUtilizadorId = adminTipoId,
            MoradaId = defaultAddress.MoradaId
        };
        db.Utilizadores.Add(adminUser);
        db.SaveChanges();

        // Adicionar contacto "admin@admin.com" para o utilizador administrador
        var emailTipoId = db.TipoContactos.First(t => t.DescContacto == "email").TipoContactoId;
        db.Contactos.Add(new Contacto
        {
            NumContacto = "admin@teste.pt",
            UtilizadorId = adminUser.UtilizadorId,
            TipoContactoId = emailTipoId
        });
        db.SaveChanges();
    }
}


// 4. Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors(DevClient);

app.UseAuthentication(); // <--- Muito importante: antes do Authorization
app.UseAuthorization();



app.MapControllers();
app.Run();
