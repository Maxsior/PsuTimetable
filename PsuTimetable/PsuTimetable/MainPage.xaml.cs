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

			Title = "Расписание";
			
			var toolbarItem = new ToolbarItem {
				Text = "Выход"
			};
			toolbarItem.Clicked += OnLogoutButtonClicked;
			ToolbarItems.Add(toolbarItem);

			messageLabel = new Label {
				HorizontalOptions = LayoutOptions.Center
			};
			
			Content = new StackLayout {
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

		private async void UpdateTimetableUI()
		{
			// TODO: Store Timetable and update it if there is internet connection
			await timetable.Update();

			// Update UI
			messageLabel.Text = timetable.currentWeek.name + "\n\n";

			foreach (Day day in timetable.currentWeek.days)
			{
				messageLabel.Text += "==========================================\n";
				messageLabel.Text += day.name + '\n';
				messageLabel.Text += "==========================================\n\n";

				if (day.bPairs)
				{
					foreach (Pair pair in day.pairs)
					{
						messageLabel.Text += "------------------------------------------\n";
						messageLabel.Text += pair.number + '\n';
						messageLabel.Text += pair.startTime + '\n';
						messageLabel.Text += "------------------------------------------\n";

						if (pair.bExist)
						{
							messageLabel.Text += pair.name + '\n';
							messageLabel.Text += pair.classroom + '\n';
							messageLabel.Text += pair.teacherName + "\n\n";
						}
						else
						{
							messageLabel.Text += "Пары нет\n\n";
						}
					}
				}
				else
				{
					messageLabel.Text += "Пар нет\n\n";
				}
			}

		}
	}
}
