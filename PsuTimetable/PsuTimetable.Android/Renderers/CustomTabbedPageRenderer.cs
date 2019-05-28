using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using System;
using PsuTimetable.Controls;
using PsuTimetable.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;


[assembly: ExportRenderer(typeof(CustomTabbedPage), typeof(CustomTabbedPageRenderer))]
namespace PsuTimetable.Droid.Renderers
{
	public class CustomTabbedPageRenderer : TabbedPageRenderer
	{
		private TabLayout tabLayout = null;

		public CustomTabbedPageRenderer(Context context) : base(context) { }

		protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
		{
			base.OnElementChanged(e);

			this.tabLayout = (TabLayout)this.GetChildAt(1);

			var selectPosition = this.tabLayout.SelectedTabPosition;

			tabLayout.TabMode = TabLayout.ModeScrollable;
			tabLayout.TabGravity = TabLayout.GravityFill;

			Handler h = new Handler();
			Action myAction = () =>
			{
				tabLayout.GetTabAt(selectPosition).Select();
			};

			h.PostDelayed(myAction, 1);
		}
	}
}