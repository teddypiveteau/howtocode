// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using TestClientConsole;

Console.WriteLine("Hello, World!");

var client = new MyHttpClient();
client.SetApiUrl("https://localhost:7149");

var isUserCreated = await client.CreateNewUser("teddy", "The bear !");
var isUserLogged = await client.Authenticate("teddy", "The bear !");

var timer = new Stopwatch();
timer.Start();
while (true)
{
    var weatherInfo = await client.GetWeatherInfo();

    Console.WriteLine($"{timer.ToString()} : {weatherInfo != null} (token step : {client.TokenStep})");
}


var toto = 3;