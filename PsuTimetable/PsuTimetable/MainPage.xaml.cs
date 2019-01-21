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
		Timetable timetable;

		public MainPage()
		{
			//InitializeComponent();

			// TODO: Check internet connection

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

			timetable = new Timetable();

			UpdateTimetableUI();
		}

		private async void UpdateTimetableUI()
		{
			await timetable.Update();

			messageLabel.Text = timetable.currentWeek.name + '\n';
			messageLabel.Text += timetable.currentWeek.days[0].name + '\n';
			messageLabel.Text += timetable.currentWeek.days[0].pairs[0].number + '\n';
			messageLabel.Text += timetable.currentWeek.days[0].pairs[0].name + '\n';
			messageLabel.Text += timetable.currentWeek.days[0].pairs[0].startTime + '\n';
			messageLabel.Text += timetable.currentWeek.days[0].pairs[0].teacherName + '\n';
			messageLabel.Text += timetable.currentWeek.days[0].pairs[0].classroom + '\n';
		}

		private async void OnLogoutButtonClicked(object sender, EventArgs e)
		{
			await Logout();
		}

		private async Task Logout()
		{
			App.IsLoggedIn = false;
			Navigation.InsertPageBefore(new LoginPage(), this);
			await Navigation.PopAsync();
		}
	}
}
