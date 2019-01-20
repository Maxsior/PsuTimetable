using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PsuTimetable
{
	public partial class MainPage : ContentPage
	{
		Label messageLabel;

		public MainPage()
		{
			//InitializeComponent();

			// TO-DO: Check internet connection

			messageLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center
			};

			var toolbarItem = new ToolbarItem
			{
				Text = "Выход"
			};
			toolbarItem.Clicked += OnLogoutButtonClicked;
			ToolbarItems.Add(toolbarItem);

			Title = "Расписание";

			Content = new StackLayout
			{
				Children = {
					new ScrollView {
						VerticalOptions = LayoutOptions.FillAndExpand,
						Content = messageLabel
					}
				}
			};
		}

		async void OnLogoutButtonClicked(object sender, EventArgs e)
		{
			await Logout();
		}

		async Task Logout()
		{
			App.IsLoggedIn = false;
			Navigation.InsertPageBefore(new LoginPage(), this);
			await Navigation.PopAsync();
		}
	}
}
