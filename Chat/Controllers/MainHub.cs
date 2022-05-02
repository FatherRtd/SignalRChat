using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Controllers
{
	public class MainHub : Hub
	{
		private static readonly Dictionary<string, string> ConnectionsGroup = new Dictionary<string, string>();

		public async Task JoinGroup(string group)
		{
			if (ConnectionsGroup.ContainsKey(Context.ConnectionId))
			{
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConnectionsGroup[Context.ConnectionId]);
				ConnectionsGroup.Remove(Context.ConnectionId);
			}
			ConnectionsGroup.Add(Context.ConnectionId, group);
			await Groups.AddToGroupAsync(Context.ConnectionId, group);
		}

        public async Task SendMessage(string userName, string message, string group)
		{
			await Clients.OthersInGroup(group).SendAsync("SendMessage",userName, message);
		}
    }
}
