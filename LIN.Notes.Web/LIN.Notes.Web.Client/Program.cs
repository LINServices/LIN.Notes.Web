global using Microsoft.AspNetCore.Components;
global using Microsoft.JSInterop;
global using LIN.Types.Responses;
using LIN.Access.Auth;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthenticationService();

LIN.Access.Notes.Build.Init();

await builder.Build().RunAsync();
