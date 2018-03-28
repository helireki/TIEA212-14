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
using System.Windows.Forms;
using System.IO;

namespace Breakthrough
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Pelin kasassa pitävä ikkuna
    /// </summary>
    public partial class MainWindow : Window
    {
        private Pelialue.Pelialue pelialue;
        private Tammialue.Tammialue tammialue;
        private int koko = 16;
        private SolidColorBrush ruudukonVari;
        private SolidColorBrush alaPelaajanVari;
        private SolidColorBrush ylaPelaajanVari;
        private String muunnos_kirjaimet = "abcdefghijklmnop";
        private int[] muunnos_luvut = { 16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1 };

        private String muistutus = "Ruudut ovat samaa muotoa kuin shakissa.";
        private int onkoBreakthrough = 0;
        private SolidColorBrush vaaleaRuutuVari;
        /// <summary>
        /// Alustetaaan ikkuna
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Kun stackpanel on ladattu, luodaan pelivalinnan mukaan oikea pelialue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void paneeli_Loaded(object sender, RoutedEventArgs e)
        {
            if (onkoBreakthrough == 0)
            {
                pelialue = new Pelialue.Pelialue(koko, ruudukonVari, alaPelaajanVari, ylaPelaajanVari);
                boxi.Child = pelialue;
            }
            else
            {
                tammialue = new Tammialue.Tammialue(koko, ruudukonVari, vaaleaRuutuVari, alaPelaajanVari, ylaPelaajanVari);
                boxi.Child = tammialue;
            }
        }


        /// <summary>
        /// Mitä tapahtuu kun klikataan About-menuitemiä
        /// Avataan About-dialogi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            About.MainWindow about = new About.MainWindow();
            about.ShowDialog();
        }


        /// <summary>
        /// Mitä tapahtuu kun klikataan Aloita-menuitemiä
        /// Poistetaan nykyiset nappulat ja luodaan uudet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Aloita_Click(object sender, RoutedEventArgs e)
        {
            if (onkoBreakthrough == 0)
            {
                pelialue.PoistaNappulat();
                pelialue.LuoNappulat();
            }
            else
            {
                tammialue = new Tammialue.Tammialue(koko, ruudukonVari, vaaleaRuutuVari, alaPelaajanVari, ylaPelaajanVari);
            }
        }


        /// <summary>
        /// Mitä tapahtuu kun klikataan Sulje-menuitemiä
        /// Tämä ikkuna suljetaan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sulje_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// Mitä tapahtuu kun klikataan Ohjeet-menuitemiä
        /// Avataan Ohjeet-dialogi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ohjeet_Click(object sender, RoutedEventArgs e)
        {
            Ohjeet.MainWindow ohjeet = new Ohjeet.MainWindow();
            ohjeet.ShowDialog();
        }


        /// <summary>
        /// Mitä tapahtuu kun kilkkaa Asetukset-menuitemiä.
        /// Voi säätää kumpi peli, ruudukon ja nappuloiden värit sekä ruudukon koon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Asetukset_Click(object sender, RoutedEventArgs e)
        {
            Asetukset.MainWindow asetukset = new Asetukset.MainWindow(onkoBreakthrough);
            asetukset.ShowDialog();

            int testiOnkoBreak = asetukset.OnkoBreakthrough;
            int testikoko = asetukset.Koko * 2;
            SolidColorBrush testiRuudukonVari = asetukset.RuudukonVari;
            SolidColorBrush testiAlaPelaajanVari = asetukset.AlaPelaajanVari;
            SolidColorBrush testiYlaPelaajanVari = asetukset.YlaPelaajanVari;

            if (testiOnkoBreak >= 0) onkoBreakthrough = testiOnkoBreak;
            if (testikoko > 0) koko = testikoko;
            if (testiRuudukonVari != null) ruudukonVari = testiRuudukonVari;
            if (testiAlaPelaajanVari != null) alaPelaajanVari = testiAlaPelaajanVari;
            if (testiYlaPelaajanVari != null) ylaPelaajanVari = testiYlaPelaajanVari;

            if (onkoBreakthrough != 0)
            {
                SolidColorBrush testiVaaleaRuutuVari = asetukset.ToinenRuudukonVari;
                if (testiVaaleaRuutuVari != null) vaaleaRuutuVari = testiVaaleaRuutuVari;
            }
            paneeli_Loaded(sender, e);
        }


        /// <summary>
        /// Mitä tapahtuu kun klikkaa Tulosta pelitulos -menuitemiä.
        /// Tulostetaan pelaajien nimet, tehdyt siirrot ja kuva pelilaudasta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tulosta_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.PrintDialog dialogi = new System.Windows.Controls.PrintDialog();
            if (dialogi.ShowDialog() != true) return;

            String[] erottimet = new String[]{" "};
            String p1 = textBox1.Text;
            String p2 = textBox2.Text;
            string[] sanat = p1.Split(erottimet, StringSplitOptions.RemoveEmptyEntries);
            string[] sanat2 = p2.Split(erottimet, StringSplitOptions.RemoveEmptyEntries);
            if (sanat.Length == 0) p1 = "YläPelaaja";
            if (sanat2.Length == 0) p2 = "AlaPelaaja";

            StackPanel tulostus = new StackPanel();
            tulostus.Margin = new Thickness(50);

            TextBlock omaBlock = new TextBlock();

            StringBuilder teksti = new StringBuilder();
            teksti.Append(muistutus + "\r\n" + p1 + " " + p2 + "\r\n");
            List<String> lista = new List<String>();

            int pixelWidth = 0;
            int pixelHeight = 0;
            if (onkoBreakthrough == 0)
            {
                lista = KokoaSiirrot(pelialue.Mustat_siirrot, pelialue.Valkoiset_siirrot);
                pixelWidth = (int)pelialue.ActualWidth;
                pixelHeight = (int)pelialue.ActualHeight;
            }
            else
            {
                lista = KokoaSiirrot(tammialue.Valkoiset_siirrot, tammialue.Punaiset_siirrot);
                pixelWidth = (int)tammialue.ActualWidth;
                pixelHeight = (int)tammialue.ActualHeight;
            }

            foreach (String alkio in lista)
                teksti.Append(alkio + "\r\n");

            omaBlock.Text = teksti.ToString();
            tulostus.Children.Add(omaBlock);

            // luodaan rendertragetbitmap, joka lisätään kuvan sourceksi
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, 
                96, 96, PixelFormats.Pbgra32);
            if (onkoBreakthrough == 0) renderTargetBitmap.Render(pelialue);
            else renderTargetBitmap.Render(tammialue);
            Image kuva = new Image();
            kuva.Source = renderTargetBitmap;

            tulostus.Children.Add(kuva);

            tulostus.Measure(new Size(dialogi.PrintableAreaWidth, dialogi.PrintableAreaHeight));
            tulostus.Arrange(new Rect(new Point(0, 0), tulostus.DesiredSize));

            dialogi.PrintVisual(tulostus, "Tulokset");
        }


        /// <summary>
        /// Mitä tapahtuu  kun klikkaa Tallenna pelitulos -menuitemiä.
        /// Pelaajien nimet ja siirrot tallennetaan tekstitiedostoon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tallenna_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<String> teksti = new List<String>();
                teksti.Add(muistutus);
                teksti.Add(textBox1.Text + "  " + textBox2.Text);
                List<String> lisattavat = new List<String>();
                if (onkoBreakthrough == 0) lisattavat = KokoaSiirrot(pelialue.Mustat_siirrot, pelialue.Valkoiset_siirrot);
                else lisattavat = KokoaSiirrot(tammialue.Valkoiset_siirrot, tammialue.Punaiset_siirrot);

                // yhdistetään listat
                teksti.AddRange(lisattavat);

                String polku = saveFileDialog.FileName;
                // tarkistetaan, onko nimessä txt-päätettä: jos ei lisätään
                String paate = polku.Substring(polku.Length - 4);
                if (!paate.Equals(".txt")) polku = polku + ".txt";

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(polku))
                {
                    foreach (string line in teksti)
                    {
                       file.WriteLine(line);
                    }
                }
            }
        }


        /// <summary>
        /// Kootaan pelaajien siirrot-listoista yksi yhteinen lista, jossa on alkiona yksi molempien siirto
        /// </summary>
        /// <returns>Lista, jossa siirrot pareina</returns>
        private List<String> KokoaSiirrot(List<Point> ylaPelaajanSiirrot, List<Point> alaPelaajanSiirrot)
        {
            List<String> t = new List<String>();

            // etsitään indeksi, jonka perusteella käydään läpi muunnoskirjaimet ja -luvut
            int indeksi = 0;
            for (int i = 0; i < muunnos_luvut.Length; i++)
            {
                if (koko / 2 == muunnos_luvut[i])
                {
                    indeksi = i;
                    break;
                }
            }
            // esim. paikka (0,0) vastaa paikkaa (muunnos_kirjaimet[0], muunnos_luvut[indeksi])
            int pituus = alaPelaajanSiirrot.Count;
            if (pituus < ylaPelaajanSiirrot.Count) pituus = ylaPelaajanSiirrot.Count;
            int j = 0;
            while (j < pituus)
            {
                String alkupaikka = MuutaPaikka(ylaPelaajanSiirrot, j, indeksi);
                String loppupaikka = MuutaPaikka(ylaPelaajanSiirrot, j + 1, indeksi);

                String alkupaikka2 = MuutaPaikka(alaPelaajanSiirrot, j, indeksi);
                String loppupaikka2 = MuutaPaikka(alaPelaajanSiirrot, j + 1, indeksi);

                t.Add(alkupaikka + ": " + loppupaikka + "     " + alkupaikka2 + ": " + loppupaikka2);
                j += 2;
            }
            return t;
        }


        /// <summary>
        /// Muunnetaan point-tyyppiä oleva paikka stringiksi, joka vastaa shakkipelin paikkoja
        /// </summary>
        /// <param name="paikat">Lista, jonka paikkaa käsitellään</param>
        /// <param name="i">Mikä indeksi paikat-taulukosta</param>
        /// <param name="indeksi">Mikä indeksi muunnos_luvut-taulukosta</param>
        /// <returns>Muunnoksen stringinä</returns>
        private String MuutaPaikka(List<Point> paikat,int i, int indeksi)
        {
            if (paikat.Count - 1 < i) return "  ";
            int ax = (int)paikat[i].X;
            int ay = (int)paikat[i].Y;
            String muunnos = muunnos_kirjaimet[ax] + muunnos_luvut[indeksi + ay].ToString();
            return muunnos;
        }
    }
}
