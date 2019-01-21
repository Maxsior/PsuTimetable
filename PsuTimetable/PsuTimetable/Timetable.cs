using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;

namespace PsuTimetable
{
	public struct Pair
	{
		public bool bExist;
		public string name;
		public string number;
		public string startTime;
		public string teacherName;
		public string classroom;
	}

	public struct Day
	{
		public bool bPairs;
		public string name;
		public Pair[] pairs;
	}

	public struct Week
	{
		public string name;
		public uint number;
		public Day[] days;
	}

	public class Timetable
    {
		public Week currentWeek;

		public Timetable()
		{

		}
		
		public async Task Update()
		{
			HttpResponseMessage response = await App.MainClient.GetAsync("stu.timetable");
			string html = await response.Content.ReadAsStringAsync();

			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(html);

			// TODO: Store all weeks
			currentWeek.days = new Day[6];
			currentWeek.name = htmlDoc.DocumentNode.SelectSingleNode("//html/body/div[2]/div/div[2]/div[2]/div[2]/span").InnerText.TrimEnd('\n');

			HtmlNode timetableNode = htmlDoc.DocumentNode.SelectSingleNode("//html/body/div[2]/div/div[2]/div[3]");
			if (timetableNode != null)
			{
				int dayNumber = 0;
				HtmlNodeCollection daysNodes = timetableNode.SelectNodes("./div");

				foreach (HtmlNode dayNode in daysNodes)
				{
					HtmlNode tableNode = dayNode.SelectSingleNode("./table");
					currentWeek.days[dayNumber].bPairs = (tableNode != null);
					currentWeek.days[dayNumber].name = dayNode.SelectSingleNode("./h3").InnerText;

					if (tableNode != null)
					{
						int pairNumber = 0;
						HtmlNodeCollection pairsNodes = tableNode.SelectNodes("./tr");
						currentWeek.days[dayNumber].pairs = new Pair[pairsNodes.Count];

						foreach (HtmlNode pairNode in pairsNodes)
						{
							HtmlNode pairInfoNode = pairNode.SelectSingleNode("./td[2]/div");
							currentWeek.days[dayNumber].pairs[pairNumber].bExist = (pairInfoNode != null);
							currentWeek.days[dayNumber].pairs[pairNumber].startTime = pairNode.SelectSingleNode("./td[1]/font").InnerText;
							currentWeek.days[dayNumber].pairs[pairNumber].number = (pairNumber + 1).ToString() + " пара";

							if (pairInfoNode != null)
							{
								currentWeek.days[dayNumber].pairs[pairNumber].name = pairInfoNode.SelectSingleNode("./div[1]/span[2]").InnerText.Trim('\n');
								currentWeek.days[dayNumber].pairs[pairNumber].teacherName = pairInfoNode.SelectSingleNode("./div[1]/span[1]/a[1]").InnerText;
								currentWeek.days[dayNumber].pairs[pairNumber].classroom = pairInfoNode.SelectSingleNode("./div[2]/span").InnerText;
							}

							pairNumber++;
						}
					}

					dayNumber++;
				}
			}
		}
	}
}
