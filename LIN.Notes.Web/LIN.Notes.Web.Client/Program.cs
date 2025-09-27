global using LIN.Access.Notes;
global using LIN.Types.Notes.Models;
global using Microsoft.AspNetCore.Components;
global using Microsoft.JSInterop;
global using LIN.Types.Responses;
using LIN.Access.Auth;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthenticationService();

builder.Services.AddNotesService("https://api.linplatform.com/notes/");

LIN.Notes.Web.Client.Services.Realtime.Build();

await builder.Build().RunAsync();
