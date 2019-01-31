using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PsuTimetable
{
	public class Credentials
	{
		private const string encryptionKey = "CredentialsEncryptionKey";

		public static string Username
		{
			get
			{
				if (Application.Current.Properties.ContainsKey("Username"))
				{
					string value = Application.Current.Properties["Username"].ToString();
					return Encryption.Decrypt(value, encryptionKey);
				}

				return string.Empty;
			}
		}

		public static string Password
		{
			get
			{
				if (Application.Current.Properties.ContainsKey("Password"))
				{
					string value = Application.Current.Properties["Password"].ToString();
					return Encryption.Decrypt(value, encryptionKey);
				}

				return string.Empty;
			}
		}

		public static bool IsSaved()
		{
			return Application.Current.Properties.ContainsKey("Username");
		}

		public static async Task Save(string username, string password)
		{
			App.Current.Properties.Add("Username", Encryption.Encrypt(username, encryptionKey));
			App.Current.Properties.Add("Password", Encryption.Encrypt(password, encryptionKey));
			await App.Current.SavePropertiesAsync();
		}

		public static async Task Clear()
		{
			App.Current.Properties.Clear();
			await App.Current.SavePropertiesAsync();
		}
	}
}
