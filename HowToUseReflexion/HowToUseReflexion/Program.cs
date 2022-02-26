// See https://aka.ms/new-console-template for more information
using HowToUseReflexion;

Console.WriteLine("Hello World!");
Console.WriteLine("This is a simple project written to show how to use refelexion in C# .Net 5");

//List all client class property names
Console.WriteLine($"Here are property names of a Client class : {string.Join(", ", ReflexionHelper.GetPropertyNames<Client>())}");

//Get client.Name property value
var client = new Client { Id = 48, Name = "Ninja", Firstname = "Coder" };

Console.WriteLine($"Here is client.Name property value : {ReflexionHelper.GetPropertyValue(client, nameof(Client.Name))}");

//Set client.Firstname property value
Console.WriteLine($"Here is client.Firstname property value before setting it : {ReflexionHelper.GetPropertyValue(client, nameof(Client.Firstname))}");
ReflexionHelper.SetPropertyValue(client, nameof(Client.Firstname), "Warrior");
Console.WriteLine($"Here is client.Firstname property value after setting it : {ReflexionHelper.GetPropertyValue(client, nameof(Client.Firstname))}");



//Instanciate Client using no arg constructor
var client2 = ReflexionHelper.GetNewInstance<Client>();
client2.Firstname = "Toto";

Console.WriteLine($"Here is client2.Firstname property value : {ReflexionHelper.GetPropertyValue(client2, nameof(Client.Firstname))}");


//Instanciate Client using (string, string) constructor
var client3 = ReflexionHelper.GetNewInstance<Client>(new object[] { "Teddy", "Bear" });

Console.WriteLine($"Here is client3.Firstname property value : {ReflexionHelper.GetPropertyValue(client3, nameof(Client.Firstname))}");


////Get property name where attribute is "SpecialField"



Console.Read();