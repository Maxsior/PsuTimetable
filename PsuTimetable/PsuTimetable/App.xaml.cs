using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net.Http;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace PsuTimetable
{
	public partial class App : Application
	{
		public static bool IsLoggedIn { get; set; }
		public static HttpClient MainClient { get; set; }

		public App()
		{
			InitializeComponent();

			MainClient = new HttpClient
			{
				BaseAddress = new Uri("https://student.psu.ru/pls/stu_cus_et/")
			};

			if (IsLoggedIn)
			{
				MainPage = new NavigationPage(new MainPage());
			}
			else
			{
				MainPage = new NavigationPage(new LoginPage());
			}
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
