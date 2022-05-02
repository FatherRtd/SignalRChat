using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Forms;

namespace ClientApp
{
	class Message
	{
		public string Text { get; set; }
		public string UserName { get; set; }
		public bool IsFromThisUser { get; set; }

		public Message(string userName, string text, bool isThisUser)
		{
			Text = text;
			UserName = userName;
			IsFromThisUser = isThisUser;
		}
	}

	class ChatTextAlignmentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool isThisUser = (bool)value;
			return isThisUser ? LayoutOptions.EndAndExpand : (object)LayoutOptions.StartAndExpand;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}

	class MainViewModel : BindableObject
	{
		private HubConnection hubConnection;

		private string userName;
		private string groupName;
		private string messageText;
		private bool isLoggedIn;

		public string UserName
		{
			get { return userName; }
			set
			{
				userName = value;
				OnPropertyChanged(nameof(UserName));
			}
		}
		public string GroupName
		{
			get { return groupName; }
			set
			{
				groupName = value; 
				OnPropertyChanged(nameof(GroupName));
			}
		}
		public string MessageText
		{
			get { return messageText; }
			set
			{
				messageText = value; 
				OnPropertyChanged(nameof(MessageText));
			}
		}
		public bool IsLoggedIn
		{
			get { return isLoggedIn; }
			set
			{
				isLoggedIn = value;
				OnPropertyChanged(nameof(IsLoggedIn));
			}
		}

		public ObservableCollection<Message> Messages { get; }

		public ICommand SendCommand { get; set; }
		public ICommand LogInCommand { get; set; }

		public MainViewModel()
		{
			hubConnection = new HubConnectionBuilder()
				.WithUrl("http://localhost:5001/hub")
				.Build();

			IsLoggedIn = false;
			Messages = new ObservableCollection<Message>();

			SendCommand = new Command(async () => await Send());
			LogInCommand = new Command(async () => await Connect());

			hubConnection.Closed += async (error) =>
			{
				Messages.Add(new Message(String.Empty, "Connection closed.",true));
				IsLoggedIn = false;
				await Task.Delay(5000);
				await Connect();
			};

			hubConnection.On<string, string>("SendMessage", ReceiveMessage);
		}

		public async Task Connect()
		{
			if(IsLoggedIn)
				return;
			try
			{
				await hubConnection.StartAsync();
				Messages.Add(new Message(String.Empty, $"You are in chat {GroupName}.", true));

				await hubConnection.SendAsync("JoinGroup", GroupName);

				IsLoggedIn = true;
			}
			catch (Exception e)
			{
				Messages.Add(new Message(String.Empty, $"Connection error: {e.Message}", true));
			}
		}

		public async Task Send()
		{
			try
			{
				await hubConnection.SendAsync("SendMessage", UserName, MessageText, GroupName);
				Messages.Add(new Message(UserName, MessageText, true));
				MessageText = "";
			}
			catch (Exception e)
			{
				Messages.Add(new Message(String.Empty, e.Message, true));
			}
		}

		public void ReceiveMessage(string name, string message)
		{
			Messages.Add(new Message(name, message, false));
		}
	}
}
