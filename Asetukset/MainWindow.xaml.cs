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

namespace Asetukset
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int p_koko = 0;
        private SolidColorBrush ruudukonVari;
        private SolidColorBrush alaPelaajanVari;
        private SolidColorBrush ylaPelaajanVari;

        private int onkoBreakthrough = 0;
        private Button ruudukkoVari2 = new Button();
        private SolidColorBrush ruudukonToinenVari;
        Label toinenRuudukonVari = new Label();
        /// <summary>
        /// Ruudukon koko -property
        /// </summary>
        public int Koko
        {
            get { return p_koko; }
            set { p_koko = value; }
        }


        /// <summary>
        /// Ruudukon väri -property
        /// Tammen tapauksessa tämä on tummemmat ruudut myös
        /// </summary>
        public SolidColorBrush RuudukonVari
        {
            get { return ruudukonVari; }
            set { ruudukonVari = value; }
        }


        /// <summary>
        /// Alhaalla olevien nappuloiden väri -property
        /// </summary>
        public SolidColorBrush AlaPelaajanVari
        {
            get { return alaPelaajanVari; }
            set { alaPelaajanVari = value; }
        }


        /// <summary>
        /// Ylhäällä oleven nappuloiden väri -property
        /// </summary>
        public SolidColorBrush YlaPelaajanVari
        {
            get { return ylaPelaajanVari; }
            set { ylaPelaajanVari = value; }
        }


        /// <summary>
        /// Pelinvaihdon property
        /// </summary>
        public int OnkoBreakthrough
        {
            get { return onkoBreakthrough; }
            set { onkoBreakthrough = value; }
        }


        /// <summary>
        /// Vaaleamman ruudukon värin property
        /// </summary>
        public SolidColorBrush ToinenRuudukonVari
        {
            get { return ruudukonToinenVari; }
            set { ruudukonToinenVari = value; }
        }


        /// <summary>
        /// Alustetaan ikkuna ja onkoBreakthrough sekä lisätään click-tapahtuma toisen ruudukon värin valitsemiseen
        /// trakoitetulle buttonille
        /// </summary>
        public MainWindow(int onkoBreakthrough)
        {
            InitializeComponent();
            this.onkoBreakthrough = onkoBreakthrough;
            ruudukkoVari2.Click += new RoutedEventHandler(ruudukkoVari2_Click);
            if (this.onkoBreakthrough == 0) buttonBreak.IsChecked = true;
            else buttonTammi.IsChecked = true;
        }


        /// <summary>
        /// Mitä tapahtuu kun kilkataan Ok-nappulaa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            String koko = comboKoko.Text;
            try
            {
                Koko = Int16.Parse(koko);
            }
            catch
            {
                this.Close();
            }
            this.Close();
        }


        /// <summary>
        /// Mitä tapahtuu kun klikataan ruudukon värin vaihtamisen nappulaa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRuudukkoVari_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush vari = ValitseVari();
            if (vari != null) RuudukonVari = vari;
        }


        /// <summary>
        /// Mitä tapahtuu kun klikataan alhaalla olevien nappuloiden värin vaihtamisen nappulaa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAlaPelaajaVari_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush vari = ValitseVari();
            if (vari != null) AlaPelaajanVari = vari;
        }


        /// <summary>
        /// mitä tapahtuu kun klikataan ylhäällä olevien nappuloiden värin vaihtamisen nappulaa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonYlaPelaajaVari_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush vari = ValitseVari();
            if (vari != null) YlaPelaajanVari = vari;
        }


        /// <summary>
        /// Valitaan väri Colordialogin avulla
        /// </summary>
        /// <returns>Väri joka valittiin</returns>
        private SolidColorBrush ValitseVari()
        {
            System.Windows.Forms.ColorDialog dlg = new System.Windows.Forms.ColorDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BrushConverter conv = new BrushConverter();

                string variStringina = System.Drawing.ColorTranslator.ToHtml(dlg.Color);

                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(variStringina));
            }
            return null;
        }


        /// <summary>
        /// mitä tapahtuu kun klikataan Cancel-nappulaa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Koko = -1;
            RuudukonVari = null;
            AlaPelaajanVari = null;
            YlaPelaajanVari = null;
            ToinenRuudukonVari = null;
            onkoBreakthrough = -1;

            this.Close();
        }

        private void buttonBreak_Checked(object sender, RoutedEventArgs e)
        {
            if (onkoBreakthrough == 0) return;
            onkoBreakthrough = 0;
            labelRuudukko1.Content = "Valitse ruudukon väri";

            paneeli.Children.Remove(ruudukkoVari2);
            paneeli.Children.Remove(toinenRuudukonVari);
        }

        private void buttonTammi_Checked(object sender, RoutedEventArgs e)
        {
            if (onkoBreakthrough != 0) return;
            onkoBreakthrough = 1;
            labelRuudukko1.Content = "Valitse tummempien ruutujen väri";

            toinenRuudukonVari.Content = "Valitse vaaleampien ruutujen väri";
            toinenRuudukonVari.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            paneeli.Children.Add(toinenRuudukonVari);

            ruudukkoVari2.Height = 23;
            ruudukkoVari2.Width = 73;
            ruudukkoVari2.Content = "Valitse väri _4";
            paneeli.Children.Add(ruudukkoVari2);
        }


        private void ruudukkoVari2_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush vari = ValitseVari();
            if (vari != null) ToinenRuudukonVari = vari;
        }
    }
}
