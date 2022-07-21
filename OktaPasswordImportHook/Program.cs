// Program
// Copyright © 2022 Joel A Mussman. All rights reserved.
//

using OktaPasswordImportHook.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Local services. The pasword validator used depends on what is specified in the configuration. The configuration
// cascades: if AD is specified it will be used, followed by LDAP.

string activeDirectory = builder.Configuration["ActiveDirectory"];

if (activeDirectory != null && activeDirectory.ToLower() == "true") {

    builder.Services.AddSingleton<IPrincipalContextProxyBuilder, PrincipalContextDomainProxyBuilder>();
    builder.Services.AddSingleton<IPasswordValidator, AdPasswordValidatorService>();

} else {

    string ldapServer = builder.Configuration["Ldap:Server"];

    if (ldapServer != null) {

        builder.Services.AddSingleton<ILdapBuilder, LdapBuilder>();
        builder.Services.AddSingleton<IPasswordValidator, LdapPasswordValidatorService>();
    }
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();