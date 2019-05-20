using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PsuTimetable
{
	class PairCell : ViewCell
	{
		public PairCell()
		{
			var nameLabel = new Label
			{
				TextColor = Color.Black,
				HorizontalOptions = LayoutOptions.Start,
				HorizontalTextAlignment = TextAlignment.Start
			};

			var startTimeLabel = new Label()
			{
				TextColor = Color.Accent,
				FontSize = 20,
				HorizontalOptions = LayoutOptions.Start,
				MinimumWidthRequest = 60,
				WidthRequest = 60,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Start
			};

			var teacherNameLabel = new Label()
			{
				FontSize = 11,
				TextColor = Color.Gray,
				HorizontalOptions = LayoutOptions.EndAndExpand,
				HorizontalTextAlignment = TextAlignment.End
			};

			var classroomLabel = new Label()
			{
				FontSize = 11,
				TextColor = Color.Gray,
			};

			nameLabel.SetBinding(Label.TextProperty, new Binding("Name"));
			startTimeLabel.SetBinding(Label.TextProperty, new Binding("StartTime"));
			teacherNameLabel.SetBinding(Label.TextProperty, new Binding("TeacherName"));
			classroomLabel.SetBinding(Label.TextProperty, new Binding("Classroom"));

			var verticaLayout = new StackLayout()
			{
				Orientation = StackOrientation.Vertical,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = {
					nameLabel,
					new StackLayout()
					{
						Orientation = StackOrientation.Horizontal,
						HorizontalOptions = LayoutOptions.FillAndExpand,
						Children = {
							classroomLabel,
							teacherNameLabel
						}
					}
				}
			};

			var horizontalLayout = new StackLayout()
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Children = {
					startTimeLabel,
					verticaLayout
				}
			};

			View = horizontalLayout;
		}
	}
}
