using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PsuTimetable
{
	public partial class MainPage : ContentPage
	{
		Label infoLabel;
		Frame infoFrame;
		Label timetableLabel;

		Color warningColor = Color.FromHex("#FFA000");
		Color errorColor = Color.FromHex("#D1321C");
		Color infoColor = Color.FromHex("#8BC34A");

		public MainPage()
		{
			//InitializeComponent();

			Title = "Расписание";

			var logoutToolbarItem = new ToolbarItem
			{
				Text = "Выход"
			};
			var refreshToolbarItem = new ToolbarItem
			{
				Text = "Обновить"
			};
			logoutToolbarItem.Clicked += LogoutToolbarItem_Clicked;
			refreshToolbarItem.Clicked += RefreshToolbarItem_Clicked;

			ToolbarItems.Add(logoutToolbarItem);
			ToolbarItems.Add(refreshToolbarItem);

			infoLabel = new Label
			{
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				TextColor = Color.White
			};

			infoFrame = new Frame
			{
				BackgroundColor = Color.White,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HeightRequest = 30,
				CornerRadius = 0,
				Padding = 0,
				Content = infoLabel
			};

			timetableLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center
			};

			Content = new StackLayout
			{
				BackgroundColor = Color.White,
				Children = {
					infoFrame,
					new ScrollView {
						VerticalOptions = LayoutOptions.FillAndExpand,
						Padding = new Thickness(10, 0, 10, 0),
						Content = timetableLabel
					}
				}
			};

			Timetable.Load();
			Teachers.Load();
		}

		private void ShowMessage(string text, Color color)
		{
			infoLabel.Text = text;
			infoFrame.BackgroundColor = color;
		}

		private async void LogoutToolbarItem_Clicked(object sender, EventArgs e)
		{
			await Logout();
		}

		private async void RefreshToolbarItem_Clicked(object sender, EventArgs e)
		{
			await Refresh();
		}

		private async Task Logout()
		{
			Timetable.Clear();
			Teachers.Clear();
			await Credentials.Clear();
			App.IsSignedIn = false;

			Navigation.InsertPageBefore(new LoginPage(), this);
			await Navigation.PopAsync();
		}

		private async Task Refresh()
		{
			if (App.IsConnectionAvailable())
			{
				if (!App.IsSignedIn && Credentials.IsSaved())
				{
					// Add authorization indicator
					int errorCode = await App.SendLoginRequest(Credentials.Username, Credentials.Password);

					if (errorCode == 0)
					{
						App.IsSignedIn = true;
					}
					else
					{
						ShowMessage("Ошибка входа: " + errorCode.ToString(), errorColor);
					}
				}

				if (App.IsSignedIn)
				{
					// Add refresh indicator
					await Timetable.Update();
					Timetable.Save();

					await Teachers.Update();
					Teachers.Save();

					UpdateUI();

					ShowMessage("Обновлено только что", infoColor);
				}
			}
			else
			{
				if (Timetable.Load())
				{
					Teachers.Load();

					ShowMessage("Режим просмотра оффлайн", warningColor);
					UpdateUI();
				}
				else
				{
					ShowMessage("Не удалось загрузить расписание", errorColor);
				}
			}
		}

		protected override async void OnAppearing()
		{
			if (Timetable.NeedUpdate())
			{
				await Refresh();
			}
			else
			{
				ShowMessage("Обновлено " + Timetable.GetLastUpdate().ToShortDateString(), infoColor);
				UpdateUI();
			}

			base.OnAppearing();
		}

		private void UpdateUI()
		{
			var weeks = Timetable.GetWeeks();
			int currentWeekId = Timetable.GetCurrentWeekId();
			var currentWeek = weeks[currentWeekId];

			timetableLabel.Text = "[" + currentWeek.Number + "] " + currentWeek.Name + "\n\n";

			foreach (Day day in currentWeek.Days)
			{
				timetableLabel.Text += "==========================================\n";
				timetableLabel.Text += day.Name + "\n";
				timetableLabel.Text += "==========================================\n\n";

				if (day.ContainPairs)
				{
					foreach (Pair pair in day.Pairs)
					{
						timetableLabel.Text += "------------------------------------------\n";
						timetableLabel.Text += pair.Number + " " + pair.StartTime + "\n";
						timetableLabel.Text += "------------------------------------------\n";

						if (pair.IsExist)
						{
							timetableLabel.Text += pair.Name + "\n";
							timetableLabel.Text += pair.Classroom + "\n";
							timetableLabel.Text += pair.TeacherName + "\n\n";
						}
						else
						{
							timetableLabel.Text += "Пары нет\n\n";
						}
					}
				}
				else
				{
					timetableLabel.Text += "Пар нет\n\n";
				}
			}

			timetableLabel.Text += "\n==========================================\n";
			timetableLabel.Text += "Преподаватели\n";
			timetableLabel.Text += "==========================================\n\n";

			var teachers = Teachers.GetTeachers();
			foreach (Teacher teacher in teachers)
			{
				timetableLabel.Text += teacher.Name + "\n";
				timetableLabel.Text += teacher.Chair + "\n";
				timetableLabel.Text += teacher.Description + "\n";
				timetableLabel.Text += teacher.ImageUri + "\n\n";
			}
		}
	}
}
