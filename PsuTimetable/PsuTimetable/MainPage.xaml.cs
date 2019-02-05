using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace PsuTimetable
{
	public partial class MainPage : ContentPage
	{
		Label infoLabel;
		Frame infoFrame;
		Label messageLabel;
		Timetable timetable;

		Color warningColor = Color.FromHex("#ffa000"); // #ffa000 #ffab00
		Color errorColor = Color.FromHex("#d1321c");

		public MainPage()
		{
			//InitializeComponent();
			
			Title = "Расписание";
			
			var toolbarItem = new ToolbarItem {
				Text = "Выход"
			};
			toolbarItem.Clicked += OnLogoutButtonClicked;
			ToolbarItems.Add(toolbarItem);

			infoLabel = new Label {
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				TextColor = Color.White
			};

			infoFrame = new Frame
			{
				BackgroundColor = warningColor,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 30,
				CornerRadius = 0,
				Padding = 0,
				Content = infoLabel
			};

			messageLabel = new Label {
				HorizontalOptions = LayoutOptions.Center
			};
			
			Content = new StackLayout {
				Children = {
					infoFrame,
					new ScrollView {
						VerticalOptions = LayoutOptions.FillAndExpand,
						Content = messageLabel
					}
				}
			};
		}

		private void ShowMessage(string text, Color color)
		{
			infoLabel.Text = text;
			infoFrame.BackgroundColor = color;
		}
		
		private void OnLogoutButtonClicked(object sender, EventArgs e)
		{
			Logout();
		}

		private async void Logout()
		{
			await Credentials.Clear();

			Navigation.InsertPageBefore(new LoginPage(), this);
			await Navigation.PopAsync();
		}

		protected override async void OnAppearing()
		{
			timetable = new Timetable();
			
			if (App.IsConnectionAvailable())
			{
				if (Credentials.IsSaved())
				{
					int errorCode = await App.SendLoginRequest(Credentials.Username, Credentials.Password);

					if (errorCode == 0)
					{
						await timetable.Update();
						UpdateTimetableUI();
						await timetable.Save();
					}
					else
					{
						ShowMessage("Ошибка входа " + errorCode.ToString(), errorColor);
					}
				}
				else
				{
					await timetable.Update();
					UpdateTimetableUI();
					await timetable.Save();
				}
			}
			else
			{
				await timetable.Load();
				ShowMessage("Режим просмотра оффлайн", warningColor);
				UpdateTimetableUI();
			}
			
			base.OnAppearing();
		}

		private void UpdateTimetableUI()
		{
			messageLabel.Text = timetable.CurrentWeek.Name + "\n\n";

			foreach (Day day in timetable.CurrentWeek.Days)
			{
				messageLabel.Text += "==========================================\n";
				messageLabel.Text += day.Name + '\n';
				messageLabel.Text += "==========================================\n\n";

				if (day.ContainPairs)
				{
					foreach (Pair pair in day.Pairs)
					{
						messageLabel.Text += "------------------------------------------\n";
						messageLabel.Text += pair.Number + " " + pair.StartTime + '\n';
						messageLabel.Text += "------------------------------------------\n";

						if (pair.IsExist)
						{
							messageLabel.Text += pair.Name + '\n';
							messageLabel.Text += pair.Classroom + '\n';
							messageLabel.Text += pair.TeacherName + "\n\n";
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
