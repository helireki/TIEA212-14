using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Laatta
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Laatta : UserControl
    {
        private Point paikka = new Point(0, 0);

        /// <summary>
        /// Alustetaan kontrolli
        /// </summary>
        public Laatta()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        // tarvitaan oma delegate koska perusversio käyttää normaalia routedeventargsia
        public delegate void LaattaArgsRoutedEventHandler(object sender, LaattaEventArgs e);

        /// <summary>
        /// oma luokka oman datan siirtelyyn
        /// </summary>
        public class LaattaEventArgs : RoutedEventArgs
        {
            private Point _piste = new Point(0, 0);

            /// <summary>
            /// Pisteproperty
            /// </summary>
            public Point piste
            {
                get { return _piste; }
                set { _piste = value; }
            }


            /// <summary>
            /// Oma versio eventargsista
            /// </summary>
            /// <param name="routedEvent"></param>
            /// <param name="l"></param>
            public LaattaEventArgs(RoutedEvent routedEvent, Point l)
                : base(routedEvent)
            {
                piste = l;
            }
        }


        // määritellään routedevent, typeof pitää olla nyt NappulaArgsRoutedEventHandler
        public static readonly RoutedEvent LaattaArgsEvent = EventManager.RegisterRoutedEvent("LaattaArgs",
            RoutingStrategy.Bubble, typeof(LaattaArgsRoutedEventHandler), typeof(Laatta));


        /// <summary>
        /// Routedeventin luonti
        /// </summary>
        public event RoutedEventHandler LaattaArgs
        {
            add { AddHandler(LaattaArgsEvent, value); }
            remove { RemoveHandler(LaattaArgsEvent, value); }
        }


        /// <summary>
        /// Aiheutetaan routedevent
        /// </summary>
        private void RaiseLaattaArgsEvent()
        {
            // nyt luodaan oma args-luokan esiintymä, tarvitaan routeventin tyyppi (LaattaArgsEvent!) ja varsinainen oma parametri
            LaattaEventArgs newEventArgs = new LaattaEventArgs(Laatta.LaattaArgsEvent, paikka);
            RaiseEvent(newEventArgs);
        }


        /// <summary>
        /// Lähetetään routedeventillä ruudun koordinaatit pelialueelle
        /// </summary>
        private void laatta_Click(object sender, RoutedEventArgs e)
        {
            int kolumni = Grid.GetColumn(this);
            int rivi = Grid.GetRow(this);
            paikka = new Point(kolumni, rivi);
            RaiseLaattaArgsEvent();
        }

        // nappulan väri dependency propertynä, nyt voidaan koodissa muuttaa väriä
        public static readonly DependencyProperty VariProperty =
            DependencyProperty.RegisterAttached("Vari",
                typeof(SolidColorBrush),
                typeof(Laatta),
                new UIPropertyMetadata(new SolidColorBrush(Colors.Wheat)));


        /// <summary>
        /// Värin setteri
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetVari(DependencyObject element, Brush value)
        {
            element.SetValue(VariProperty, value);
        }


        /// <summary>
        /// värin getteri
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Brush GetVari(DependencyObject element)
        {
            return (Brush)element.GetValue(VariProperty);
        }
    }
}
