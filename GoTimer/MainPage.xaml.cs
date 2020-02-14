using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace GoTimer
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainPageViewModel();
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            (BindingContext as MainPageViewModel)?.StartCommand?.Execute();
        }

        private void MainPage_OnLayoutChanged(object sender, EventArgs e)
        {
            var t = TotalGrid.Width;

            

            t -= 10;

            var step = ((t - 100) / 2) / 6;

            double hmargin = 10.0;
            double vmargin = ((TotalGrid.Height / 2) - 50) - ((TotalGrid.Width / 2) - 5);

            RingOne.Margin = new Thickness(hmargin, vmargin);
            RingOne.CornerRadius = (float)t / 2;

            hmargin += step;
            vmargin += step;
            t -= step;

            RingTwo.Margin = new Thickness(hmargin, vmargin);
            RingTwo.CornerRadius = (float)t / 2;

            hmargin += step;
            vmargin += step;
            t -= step;

            RingThree.Margin = new Thickness(hmargin, vmargin);
            RingThree.CornerRadius = (float)t / 2;

            hmargin += step;
            vmargin += step;
            t -= step;

            RingFour.Margin = new Thickness(hmargin, vmargin);
            RingFour.CornerRadius = (float)t / 2;

            hmargin += step;
            vmargin += step;
            t -= step;

            RingFive.Margin = new Thickness(hmargin, vmargin);
            RingFive.CornerRadius = (float)t / 2;

            hmargin += step;
            vmargin += step;
            t -= step;

            RingSix.Margin = new Thickness(hmargin, vmargin);
            RingSix.CornerRadius = (float)t / 2;

            hmargin += step;
            vmargin += step;
            t -= step;

            RingSeven.Margin = new Thickness(hmargin, vmargin);
            RingSeven.CornerRadius = (float)t / 2;

        }
    }
}
