using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PsuTimetable
{
    class TeacherCell : ViewCell
    {
        public TeacherCell()
        {
			var teacherImage = new Image();

            var NameLabel = new Label()
            {
                FontSize = 14,
                TextColor = Color.Black
            };

            var ChairLabel = new Label()
            {
                FontSize = 11,
                TextColor = Color.Gray
            };

            var DescriptionLabel = new Label()
            {
                FontSize = 11,
                TextColor = Color.Gray
            };


            teacherImage.SetBinding(Image.SourceProperty, new Binding("ImageUri"));
            NameLabel.SetBinding(Label.TextProperty, new Binding("Name"));
            ChairLabel.SetBinding(Label.TextProperty, new Binding("Chair"));
            DescriptionLabel.SetBinding(Label.TextProperty, new Binding("Description"));

            var verticaLayout = new StackLayout()
            {
                Orientation = StackOrientation.Vertical,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { NameLabel, ChairLabel, DescriptionLabel }
            };

            var horizontalLayout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(5, 0, 5, 10),

                Children = {
                    teacherImage,
                    verticaLayout
                }
            };

            View = horizontalLayout;
        }
    }
}
