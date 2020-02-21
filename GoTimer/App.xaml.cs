using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GoTimer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            GoTimerStatic.SaveProperty = (key, value) => Application.Current.Properties[key] = value;
            GoTimerStatic.GetProperty = key => Application.Current.Properties[key];
            GoTimerStatic.HasProperty = key => Application.Current.Properties.ContainsKey(key);
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
