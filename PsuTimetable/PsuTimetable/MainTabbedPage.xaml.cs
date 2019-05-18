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
		Timetable timetable;

		Label debugLabel;
		Entry consoleEntry;
		int currentWeek = 0;

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
			else if (consoleEntry.Text.StartsWith("currentWeek="))
			{
				int temp;
				if (int.TryParse(consoleEntry.Text.Substring(12), out temp))
				{
					if (temp >= 0 && temp < timetable.Weeks.Count)
					{
						currentWeek = temp;
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
				await Credentials.Clear();

				Navigation.InsertPageBefore(new LoginPage(), this);
				await Navigation.PopAsync();
			}
			else if (consoleEntry.Text == "refresh")
			{
				await timetable.Update();
				UpdateUI();
			}
			else if (consoleEntry.Text == "currentWeekId")
			{
				WriteDebugLine(timetable.currentWeekId.ToString());
			}
			else if (consoleEntry.Text == "save")
			{
				timetable.Save();
			}
			else if (consoleEntry.Text == "load")
			{
				timetable.Load();
				UpdateUI();
			}
			else
			{
				WriteDebugLine("Unknown command");
			}
		}

		protected override async void OnAppearing()
		{
			timetable = new Timetable();

			if (App.IsConnectionAvailable())
			{
				bool bSuccess = false;

				if (Credentials.IsSaved())
				{
					// Add authorization indicator
					int errorCode = await App.SendLoginRequest(Credentials.Username, Credentials.Password);

					if (errorCode == 0)
					{
						bSuccess = true;
					}
					else
					{
						WriteDebugLine("Ошибка входа: " + errorCode.ToString());
					}
				}
				else
				{
					bSuccess = true;
				}

				if (bSuccess)
				{
					// Add refresh indicator
					await timetable.Update();
					timetable.Save();
					UpdateUI();
				}
			}
			else
			{
				WriteDebugLine("Режим просмотра оффлайн");

				if (!timetable.Load())
				{
					WriteDebugLine("Не удалось загрузить расписание");
				}

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
			foreach (Day day in timetable.Weeks[currentWeek].Days)
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