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
using System.Windows.Media.Animation;

namespace Pelinappula
{
    /// <summary>
    /// Interaction logic for Pelinappula.xaml
    /// </summary>
    public partial class Pelinappula : UserControl
    {
        private Point paikka;
        private bool onkoTammi = false;

        /// <summary>
        /// onkoTammi property
        /// </summary>
        public bool OnkoTammi
        {
            get { return onkoTammi; }
            set { onkoTammi = value; }
        }

        /// <summary>
        /// Kontrollin alustus ja nappulan datakontekstin sidonta
        /// </summary>
        public Pelinappula()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        // tarvitaan oma delegate koska perusversio käyttää normaalia routedeventargsia
        public delegate void NappulaArgsRoutedEventHandler(object sender, NappulaEventArgs e);
        
        /// <summary>
        /// Oma luokka oman datan siirtelyyn
        /// </summary>
        public class NappulaEventArgs : RoutedEventArgs
        {
            private Point _piste = new Point(0,0);

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
            public NappulaEventArgs(RoutedEvent routedEvent, Point l): base(routedEvent)
            {
                piste = l;
            }
        }


        // määritellään routedevent, typeof pitää olla nyt NappulaArgsRoutedEventHandler
        public static readonly RoutedEvent NappulaArgsEvent = EventManager.RegisterRoutedEvent("NappulaArgs",
            RoutingStrategy.Bubble, typeof(NappulaArgsRoutedEventHandler), typeof(Pelinappula));

        /// <summary>
        /// Routedeventin luonti
        /// </summary>
        public event RoutedEventHandler NappulaArgs
        {
            add { AddHandler(NappulaArgsEvent, value); }
            remove { RemoveHandler(NappulaArgsEvent, value); }
        }


        /// <summary>
        /// Aiheutetaan routedevent
        /// </summary>
        private void RaiseNappulaArgsEvent()
        { 
            // nyt luodaan oma args-luokan esiintymä, tarvitaan routeventin tyyppi (NappulaArgsEvent!) ja varsinainen oma parametri
            NappulaEventArgs newEventArgs = new NappulaEventArgs(Pelinappula.NappulaArgsEvent, paikka);
            RaiseEvent(newEventArgs);
        }


        /// <summary>
        /// Lähetetään routedeventillä ruudun koordinaatit pelialueelle
        /// </summary>
        private void nappula_Click(object sender, RoutedEventArgs e)
        {
            int kolumni = Grid.GetColumn(this);
            int rivi = Grid.GetRow(this);
            paikka = new Point(kolumni, rivi);
            RaiseNappulaArgsEvent();
        }


        // nappulan väri dependency propertynä, nyt voidaan koodissa muuttaa väriä
        public static readonly DependencyProperty VariProperty =
            DependencyProperty.RegisterAttached("Vari",
                typeof(SolidColorBrush),
                typeof(Pelinappula),
                new UIPropertyMetadata(new SolidColorBrush(Colors.Black)));

        public static readonly DependencyProperty TammiVariProperty =
            DependencyProperty.RegisterAttached("TammiVari",
            typeof(SolidColorBrush),
            typeof(Pelinappula),
            new UIPropertyMetadata(new SolidColorBrush(Colors.Red)));

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
        /// Värin getteri
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Brush GetVari(DependencyObject element)
        {
            return (Brush)element.GetValue(VariProperty);
        }


        /// <summary>
        /// Tammimerkin läpinäkymättömyys dependency propertynä
        /// </summary>
        public static DependencyProperty LapinakymattomyysProperty =
            DependencyProperty.RegisterAttached("Lapinakymattomyys",
            typeof(double),
            typeof(Pelinappula),
            new UIPropertyMetadata(0.0));


        /// <summary>
        /// Läpinäkymättömyyden setteri
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetLapinakymattomyys(DependencyObject element, Brush value)
        {
            element.SetValue(LapinakymattomyysProperty, value);
        }


        /// <summary>
        /// Läpinäkymättömyyden getteri
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static double GetLapinakymattomyys(DependencyObject element)
        {
            return (double)element.GetValue(LapinakymattomyysProperty);
        }


        /// <summary>
        /// Käynnistetään animaatio, jolla tuodaan näkyviin nappulan tammimerkki
        /// Jostain syystä mitään ei tapahdu, vaikka animaatio käynnistetään
        /// </summary>
        public void MuutaTammeksi()
        {
            DoubleAnimation muutos = new DoubleAnimation();
            muutos.From = 0.0;
            muutos.To = 1.0;
            muutos.Duration = new Duration(TimeSpan.FromSeconds(5));

            nappula1.BeginAnimation(LapinakymattomyysProperty, muutos);
        }
    }
}
