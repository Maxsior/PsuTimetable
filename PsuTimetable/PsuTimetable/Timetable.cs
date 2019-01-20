using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;

namespace PsuTimetable
{
	public struct Pair
	{
		public string name;
		public string number;
		public string startTime;
		public string teacherName;
		public string classroom;
	}

	public struct Day
	{
		public string name;
		public Pair[] pairs;
	}

	public struct Week
	{
		public uint number;
		public Day[] days;
	}

	public class Timetable
    {
		public Week currentWeek;

		public Timetable()
		{
			currentWeek = new Week();
			currentWeek.days = new Day[6];

			UpdateTimetable();
		}
		
		void UpdateTimetable()
		{
			
		}
	}
}
