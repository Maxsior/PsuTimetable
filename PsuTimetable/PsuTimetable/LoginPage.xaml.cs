using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;
using System.Web;

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
				HorizontalTextAlignment = TextAlignment.Center
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
				ReturnType = ReturnType.Go
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

			passwordEntry.Completed += (object sender, EventArgs args) => {
				Login(usernameEntry.Text, passwordEntry.Text);
			};

			Label loginInfoLabel = new Label
			{
				Text = "Вход в ЕТИС",
				FontSize = 20,
				TextColor = Color.Black,
				HorizontalTextAlignment = TextAlignment.Center
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

		private void OnButtonClicked(object sender, EventArgs args)
		{
			Login(usernameEntry.Text, passwordEntry.Text);
		}

		public static bool IsConnectionAvailable()
		{
			try
			{
				System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient("www.google.com", 80);
				client.Close();
				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
		}
		
		private async void Login(string username, string password)
		{
			if (username == string.Empty || password == string.Empty)
			{
				messageLabel.Text = "Введите имя пользователя и пароль";
				return;
			}

			if (!IsConnectionAvailable())
			{
				messageLabel.Text = "Не удалось подключиться к сети";
				return;
			}

			string jsonData = "p_redirect=&p_username=" + HttpUtility.UrlEncode(username, Encoding.GetEncoding(1251)) + "&p_password=" + password;
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

			HttpResponseMessage response = await App.MainClient.PostAsync("stu.login", content);

			if (!response.IsSuccessStatusCode)
			{
				messageLabel.Text = "Не удалось получить доступ к сайту (" + response.StatusCode.ToString() + ")";
				return;
			}

			string html = await response.Content.ReadAsStringAsync();

			if (html.Contains("Неверное имя пользователя или пароль"))
			{
				messageLabel.Text = "Неверное имя пользователя или пароль";
				passwordEntry.Text = string.Empty;
				return;
			}
			else if (html.Contains("Превышен лимит"))
			{
				messageLabel.Text = "Превышен лимит (5) неудачных попыток ввода пароля. Повторите попытку через 10 минут";
				return;
			}

			App.IsLoggedIn = true;
			Navigation.InsertPageBefore(new MainPage(), this);
			await Navigation.PopAsync();
		}
	}
}