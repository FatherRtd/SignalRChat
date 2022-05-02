using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Client
{
	class Program
	{
		private static HubConnection connection;

		static async Task Main(string[] args)
		{
			connection = new HubConnectionBuilder()
				.WithUrl("http://localhost:5001/hub")
				.Build();


			connection.On<string, string>("SendMessage", (user, message) => Console.WriteLine($"Message from {user}: {message}"));
			await connection.StartAsync();

			string group = Console.ReadLine();
			string userName = Console.ReadLine();

			await connection.SendAsync("JoinGroup", group);

			bool exit = false;

			while (!exit)
			{
				var message = Console.ReadLine();

				if (message != "exit")
					await connection.SendAsync("SendMessage",userName, message, group);
				else
					exit = true;
			}
			Console.ReadKey();
		}
	}
}
