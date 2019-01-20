using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Web;
using Plugin.Connectivity;

namespace PsuTimetable
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
	{
		Label messageLabel;
		Entry usernameEntry;
		Entry passwordEntry;

		public LoginPage()
		{
			//InitializeComponent();

			Title = "Расписание ЕТИС";
			Padding = new Thickness(5, 20, 5, 0);

			messageLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center
			};

			usernameEntry = new Entry
			{
				Text = string.Empty,
				Placeholder = "Имя пользователя",
				ReturnType = ReturnType.Next
			};

			passwordEntry = new Entry
			{
				Text = string.Empty,
				Placeholder = "Пароль",
				IsPassword = true,
				ReturnType = ReturnType.Send
			};

			Button button = new Button
			{
				Text = "Войти",
				HorizontalOptions = LayoutOptions.Center
			};
			button.Clicked += OnButtonClicked;


			usernameEntry.Completed += (object sender, EventArgs args) => {
				passwordEntry.Focus();
			};

			//passwordEntry.Completed += (object sender, EventArgs args) => {
			//	button.OnButtonClicked(null);
			//};

			Label loginInfoLabel = new Label
			{
				Text = "Вход в ЕТИС",
				FontSize = 20,
				TextColor = Color.Black,
				HorizontalOptions = LayoutOptions.Center
			};

			Content = new StackLayout
			{
				Children = {
					messageLabel,
					loginInfoLabel,
					usernameEntry,
					passwordEntry,
					button
				}
			};
		}

		async void OnButtonClicked(object sender, EventArgs args)
		{
			if (usernameEntry.Text == string.Empty || passwordEntry.Text == string.Empty)
			{
				messageLabel.Text = "Введите имя пользователя и пароль";
				return;
			}

			if (!CrossConnectivity.Current.IsConnected)
			{
				//await DisplayAlert("Внимание", "Не удалось подключиться к сети", "OK");
				messageLabel.Text = "Не удалось подключиться к сети";
				return;
			}

			bool bLoggedIn = await Login(usernameEntry.Text, passwordEntry.Text);
			if (bLoggedIn)
			{
				App.IsLoggedIn = true;
				Navigation.InsertPageBefore(new MainPage(), this);
				await Navigation.PopAsync();
			}
			else
			{
				messageLabel.Text = "Неверное имя пользователя или пароль";
				passwordEntry.Text = string.Empty;
			}
		}

		private async Task<bool> Login(string username, string password)
		{
			string jsonData = "p_redirect=&p_username=" + HttpUtility.UrlEncode(username, Encoding.GetEncoding(1251)) + "&p_password=" + password;
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

			HttpResponseMessage response = await App.MainClient.PostAsync("stu.login", content);
			string html = await response.Content.ReadAsStringAsync();

			return !html.Contains("Неверное имя пользователя или пароль");
		}
	}
}