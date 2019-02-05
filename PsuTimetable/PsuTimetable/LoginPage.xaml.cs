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
		Switch saveCredentialsSwitch;

		public LoginPage()
		{
			//InitializeComponent();

			Title = "Вход в ЕТИС";
			Padding = new Thickness(5, 20, 5, 0);

			messageLabel = new Label {
				HorizontalTextAlignment = TextAlignment.Center
			};

			usernameEntry = new Entry {
				Placeholder = "Имя пользователя",
				ReturnType = ReturnType.Next
			};

			passwordEntry = new Entry {
				Placeholder = "Пароль",
				IsPassword = true,
				ReturnType = ReturnType.Done
			};

			Button loginButton = new Button {
				Text = "Войти",
				HorizontalOptions = LayoutOptions.FillAndExpand,
				TranslationY = 20
			};
			loginButton.Clicked += OnButtonClicked;

			saveCredentialsSwitch = new Switch {
				IsToggled = true
			};

			usernameEntry.Completed += (object sender, EventArgs args) => {
				passwordEntry.Focus();
			};

			passwordEntry.Completed += (object sender, EventArgs args) => {
				Login(usernameEntry.Text, passwordEntry.Text, saveCredentialsSwitch.IsToggled);
			};

			Content = new StackLayout {
				Children = {
					messageLabel,
					usernameEntry,
					passwordEntry,
					new StackLayout {
						Orientation = StackOrientation.Horizontal,
						HorizontalOptions = LayoutOptions.End,
						Children = {
							new Label {
								Text = "Запомнить меня",
								VerticalTextAlignment = TextAlignment.Center
							},
							saveCredentialsSwitch
						}
					},
					loginButton
				}
			};
		}

		private void OnButtonClicked(object sender, EventArgs args)
		{
			Login(usernameEntry.Text, passwordEntry.Text, saveCredentialsSwitch.IsToggled);
		}
		
		private async void Login(string username, string password, bool bSaveCredentials)
		{
			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			{
				messageLabel.Text = "Введите имя пользователя и пароль";
				return;
			}

			if (!App.IsConnectionAvailable())
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
				passwordEntry.Focus();
				return;
			}
			else if (html.Contains("Превышен лимит"))
			{
				messageLabel.Text = "Превышен лимит (5) неудачных попыток ввода пароля. Повторите попытку через 10 минут";
				return;
			}

			if (bSaveCredentials && !Credentials.IsSaved())
			{
				await Credentials.Save(username, password);
			}

			Navigation.InsertPageBefore(new MainPage(), this);
			await Navigation.PopAsync();
		}
	}
}