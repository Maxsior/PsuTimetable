using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			CurrentPage = shedulePage;

			//ToolbarItems.Add(new ToolbarItem { Text = "Search" });

			Timetable.Load();
			currentWeekId = Timetable.GetCurrentWeekId();
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
				if (int.TryParse(consoleEntry.Text.Substring(15), out temp))
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

		protected override async void OnAppearing()
		{
			if (Timetable.NeedUpdate())
			{
				await Refresh();
			}
			else
			{
				WriteDebugLine("Обновлено " + Timetable.GetLastUpdate().ToShortDateString());
				UpdateUI();
			}

			base.OnAppearing();
		}

		private void UpdateUI()
		{
			shedulePage.Children.Clear();

			// Debug Page
			ContentPage debugPage = new ContentPage
			{
				Title = "Debug",
				Padding = new Thickness(5, 0, 5, 0),
				Content = new StackLayout
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
				}
			};
			shedulePage.Children.Add(debugPage);

			// Days pages
			var weeks = Timetable.GetWeeks();
			foreach (Day day in weeks[currentWeekId].Days)
			{
				Label label = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					TextColor = Color.Black
				};

				ListView listView = new ListView
				{
					SeparatorVisibility = SeparatorVisibility.None,
					SelectionMode = ListViewSelectionMode.None,
					IsPullToRefreshEnabled = true
				};

				var list = new List<string>();
				listView.SeparatorColor = Color.Gray;
				listView.SelectionMode = ListViewSelectionMode.None;

				if (day.ContainPairs)
				{
					foreach (Pair pair in day.Pairs)
					{
						list.Add(pair.Number + " " + pair.StartTime + " | " + (pair.IsExist ? pair.Name : "Пары нет"));
					}
				}
				else
				{
					list.Add("Пар нет");
				}

				listView.ItemsSource = list;

				ContentPage page = new ContentPage
				{
					Title = day.Name,
					Content = listView
				};

				shedulePage.Children.Add(page);
			}
		}
	}
}