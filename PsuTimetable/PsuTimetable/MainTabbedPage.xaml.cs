using System;
using System.Globalization;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PsuTimetable
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainTabbedPage : TabbedPage
	{
		Label debugLabel;
		Entry consoleEntry;
		int currentWeekId = 0;

		public MainTabbedPage()
		{
			InitializeComponent();

			debugLabel = new Label
			{
				HorizontalTextAlignment = TextAlignment.Start,
				TextColor = Color.Gray
			};

			consoleEntry = new Entry
			{
				Placeholder = "Console",
				ReturnType = ReturnType.Send,
				TextColor = Color.Black,
				PlaceholderColor = Color.Gray,
				IsSpellCheckEnabled = false
			};
			consoleEntry.Completed += ConsoleEntry_Completed;

			// Debug Page
			settingsPage.Padding = new Thickness(5, 0, 5, 0);
			settingsPage.Content = new StackLayout
			{
				Children =
				{
					consoleEntry,
					new ScrollView
					{
						VerticalOptions = LayoutOptions.FillAndExpand,
						Content = debugLabel
					}
				}
			};

			// Placeholder page
			var placeholderPage = new ContentPage
			{
				Content = new ActivityIndicator
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Color = Color.Accent,
					HeightRequest = 48,
					WidthRequest = 48,
					IsRunning = true
				}
			};
			shedulePage.Children.Add(placeholderPage);

			CurrentPage = shedulePage;

			// Load and update timetable
			Timetable.Load();
			currentWeekId = Timetable.GetCurrentWeekId();

			if (Timetable.NeedUpdate())
			{
				Refresh();
			}
			else
			{
				WriteDebugLine("Обновлено " + Timetable.GetLastUpdate().ToShortDateString());
				UpdateUI();
			}
		}

		private void WriteDebugLine(string text)
		{
			debugLabel.Text += text + "\n";
		}

		private async void ConsoleEntry_Completed(object sender, EventArgs e)
		{
			WriteDebugLine("> " + consoleEntry.Text);

			if (consoleEntry.Text == "clear")
			{
				debugLabel.Text = "";
			}
			else if (consoleEntry.Text.StartsWith("currentWeekId="))
			{
				int temp;
				if (int.TryParse(consoleEntry.Text.Substring(14), out temp))
				{
					if (temp >= 0 && temp < Timetable.GetWeeks().Count)
					{
						currentWeekId = temp;
						UpdateUI();
					}
					else
					{
						WriteDebugLine("Out of bounds");
					}
				}
			}
			else if (consoleEntry.Text == "logout")
			{
				await Logout();
			}
			else if (consoleEntry.Text == "refresh")
			{
				await Refresh();
			}
			else if (consoleEntry.Text == "currentWeekId")
			{
				WriteDebugLine(Timetable.GetCurrentWeekId().ToString());
			}
			else
			{
				WriteDebugLine("Unknown command");
			}
		}

		private async Task Logout()
		{
			Timetable.Clear();
			await Credentials.Clear();

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
						WriteDebugLine("Ошибка входа: " + errorCode.ToString());
					}
				}

				if (App.IsSignedIn)
				{
					// Add refresh indicator
					await Timetable.Update();
					Timetable.Save();

					currentWeekId = Timetable.GetCurrentWeekId();
					UpdateUI();

					WriteDebugLine("Обновлено только что");
				}
			}
			else
			{
				if (Timetable.Load())
				{
					WriteDebugLine("Режим просмотра оффлайн");
					UpdateUI();
				}
				else
				{
					WriteDebugLine("Не удалось загрузить расписание");
				}
			}
		}

		private void UpdateUI()
		{
			shedulePage.Children.Clear();

			// Days pages
			var weeks = Timetable.GetWeeks();
			foreach (Day day in weeks[currentWeekId].Days)
			{
				ContentPage page = new ContentPage
				{
					Title = day.Name,
					Padding = new Thickness(0, 10, 5, 0)
				};

				if (day.Pairs.Count == 0)
				{
					page.Content = new Label
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.FillAndExpand,
						HorizontalTextAlignment = TextAlignment.Center,
						VerticalTextAlignment = TextAlignment.Center,
						TextColor = Color.Gray,
						Text = "Пар нет!",
						FontSize = 20
					};
				}
				else
				{
					page.Content = new ListView
					{
						SeparatorVisibility = SeparatorVisibility.None,
						RowHeight = 70,
						SelectionMode = ListViewSelectionMode.None,
						ItemTemplate = new DataTemplate(typeof(PairCell)),
						ItemsSource = day.Pairs
					};
				}

				shedulePage.Children.Add(page);

				// Select page with current day
				if (day.Name.ToLower() == DateTime.Now.ToString("dddd, dd MMMM"))
				{
					shedulePage.CurrentPage = page;
				}
			}
		}

		private async void UpdateButton_Clicked(object sender, EventArgs e)
		{
			await Refresh();
		}

		private void AccountButton_Clicked(object sender, EventArgs e)
		{
			DisplayAlert("Фамилия Имя Отчество", "Факультет", "Выйти");
		}
	}
}