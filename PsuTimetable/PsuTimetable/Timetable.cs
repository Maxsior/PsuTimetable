using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace PsuTimetable
{
	public class Pair
	{
		public bool IsExist { get; set; }
		public string Name { get; set; }
		public string Number { get; set; }
		public string StartTime { get; set; }
		public string TeacherName { get; set; }
		public string Classroom { get; set; }
	}

	public class Day
	{
		public bool ContainPairs { get; set; }
		public string Name { get; set; }
		public List<Pair> Pairs { get; set; }
		public Day() => Pairs = new List<Pair>();
	}

	public class Week
	{
		public int Number { get; set; }
		public string Name { get; set; }
		public List<Day> Days { get; set; }
		public Week() => Days = new List<Day>();
	}

	public class Timetable
    {
		// Store currentWeekId in database
		public int currentWeekId;
		public List<Week> Weeks { get; set; }
		public Week CurrentWeek => Weeks[currentWeekId];

		public Timetable()
		{
			Weeks = new List<Week>();
		}

		public bool IsSaved()
		{
			string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "timetable.xml");
			return File.Exists(filePath);
		}

		public void Save()
		{
			string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "timetable.xml");

			using (var writer = File.OpenWrite(filePath))
			{
				var serializer = new XmlSerializer(typeof(List<Week>));
				serializer.Serialize(writer, Weeks);
			}
		}
		
		public bool Load()
		{
			string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "timetable.xml");

			if (File.Exists(filePath))
			{
				string text = File.ReadAllText(filePath);

				using (var reader = new StringReader(text))
				{
					var serializer = new XmlSerializer(typeof(List<Week>));
					Weeks = (List<Week>)serializer.Deserialize(reader);

					return Weeks.Count != 0;
				}
			}

			return false;
		}

		public async Task Update()
		{
			HttpResponseMessage response = await App.MainClient.GetAsync("stu.timetable");
			string html = await response.Content.ReadAsStringAsync();

			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);

			int startWeekNumber = -1;
			HtmlNode weeksNode = htmlDoc.DocumentNode.SelectSingleNode("//html/body/div[2]/div/div[2]/div[2]/ul");
			foreach (HtmlNode weekNode in weeksNode.SelectNodes("./li"))
			{
				Week week;

				HtmlNode refNode = weekNode.SelectSingleNode("./a");
				if (refNode != null)
				{
					int weekNumber = int.Parse(refNode.InnerText);
					if (startWeekNumber == -1)
						startWeekNumber = weekNumber;

					week = await InitWeek(weekNumber);
				}
				else
				{
					int weekNumber = int.Parse(weekNode.InnerText);
					if (startWeekNumber == -1)
						startWeekNumber = weekNumber;

					week = await InitWeek(weekNumber);
					currentWeekId = weekNumber - startWeekNumber;
				}

				Weeks.Add(week);
			}
		}

		public async Task<Week> InitWeek(int weekNumber)
		{
			HttpResponseMessage response = await App.MainClient.GetAsync("stu.timetable?p_cons=n&p_week=" + weekNumber.ToString());
			string html = await response.Content.ReadAsStringAsync();

			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);

			Week week = new Week
			{
				Number = weekNumber
			};

			HtmlNode weekNameNode = htmlDoc.DocumentNode.SelectSingleNode("//html/body/div[2]/div/div[2]/div[2]/div[2]/span");
			HtmlNode timetableNode = htmlDoc.DocumentNode.SelectSingleNode("//html/body/div[2]/div/div[2]/div[3]");

			if (weekNameNode != null)
			{
				week.Name = weekNameNode.InnerText.TrimEnd('\n');
			}
			
			if (timetableNode != null)
			{
				foreach (HtmlNode dayNode in timetableNode.SelectNodes("./div"))
				{
					Day day = new Day();

					HtmlNode tableNode = dayNode.SelectSingleNode("./table");
					day.ContainPairs = (tableNode != null);
					day.Name = dayNode.SelectSingleNode("./h3").InnerText;

					if (tableNode != null)
					{
						foreach (HtmlNode pairNode in tableNode.SelectNodes("./tr"))
						{
							Pair pair = new Pair();

							HtmlNode pairInfoNode = pairNode.SelectSingleNode("./td[2]/div");
							pair.IsExist = (pairInfoNode != null);
							pair.StartTime = pairNode.SelectSingleNode("./td[1]/font").InnerText;
							pair.Number = pairNode.SelectSingleNode("./td[1]").InnerText.Substring(0, 6);

							if (pairInfoNode != null)
							{
								pair.Name = pairInfoNode.SelectSingleNode("./div[1]/span[2]").InnerText.Trim('\n');
								pair.TeacherName = pairInfoNode.SelectSingleNode("./div[1]/span[1]/a[1]").InnerText;
								pair.Classroom = pairInfoNode.SelectSingleNode("./div[2]/span").InnerText;
							}

							day.Pairs.Add(pair);
						}
					}

					week.Days.Add(day);
				}
			}

			return week;
		}
	}
}
