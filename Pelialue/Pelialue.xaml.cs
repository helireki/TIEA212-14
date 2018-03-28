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

namespace Pelialue
{
    /// <summary>
    /// Interaction logic for Pelialue.xaml
    /// Hallitaan nappuloita ja niiden liikkeitä
    /// </summary>
    public partial class Pelialue : UserControl
    {
        private Pelinappula.Pelinappula siirrettavaNappula = new Pelinappula.Pelinappula();
        private Point siirrettavanNappulanPaikka = new Point(-1, -1);
        private static int kokoAlue;
        private SolidColorBrush ruudukko;
        private SolidColorBrush alaPelaajanVari;
        private SolidColorBrush ylaPelaajanVari;

        private Pelinappula.Pelinappula[] mustat_nappulat;
        private Pelinappula.Pelinappula[] valkoiset_nappulat;
        private Point[] m_paikat;
        private Point[] v_paikat;
        // tähän lista, jonne siirrot
        private List<Point> mustat_siirrot;
        private List<Point> valkoiset_siirrot;

        private int vuoro = 0;

        /// <summary>
        /// mustien siirtojen property
        /// </summary>
        public List<Point> Mustat_siirrot
        {
            get { return mustat_siirrot; }
        }

        /// <summary>
        /// Valkoisten siirtojen property
        /// </summary>
        public List<Point> Valkoiset_siirrot
        {
            get { return valkoiset_siirrot; }
        }


        /// <summary>
        /// Alustetaan kontrolli
        /// </summary>
        public Pelialue(int koko, SolidColorBrush ruudukko, SolidColorBrush alaPelaajanVari, SolidColorBrush ylaPelaajanVari)
        {
            kokoAlue = koko;
            this.ruudukko = ruudukko;
            this.alaPelaajanVari = alaPelaajanVari;
            this.ylaPelaajanVari = ylaPelaajanVari;
            mustat_nappulat = new Pelinappula.Pelinappula[kokoAlue];
            valkoiset_nappulat = new Pelinappula.Pelinappula[kokoAlue];
            m_paikat = new Point[kokoAlue];
            v_paikat = new Point[kokoAlue];
            mustat_siirrot = new List<Point>();
            valkoiset_siirrot = new List<Point>();

            InitializeComponent();
        }


        /// <summary>
        /// Luodaan ruudukko ja nappulat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pelialue_Loaded(object sender, RoutedEventArgs e)
        {
            LuoRuudukko();
            LuoNappulat();
        }

        /// <summary>
        /// Luodaan gridiin rivit, kolumnit ja laatta-kontrollit
        /// </summary>
        private void LuoRuudukko()
        {
            double koko = this.Height / (kokoAlue/2);
            for (int riv = 0; riv < kokoAlue / 2; riv++)
            {
                for (int sar = 0; sar < kokoAlue / 2; sar++)
                {
                    RowDefinition r = new RowDefinition();
                    r.Height = new GridLength(koko, GridUnitType.Pixel);
                    ColumnDefinition c = new ColumnDefinition();
                    c.Width = new GridLength(koko, GridUnitType.Pixel);
                    gridi.RowDefinitions.Add(r);
                    gridi.ColumnDefinitions.Add(c);

                    Laatta.Laatta laatta = new Laatta.Laatta();
                    if (ruudukko == null) ruudukko = Brushes.Wheat;
                    Laatta.Laatta.SetVari(laatta, ruudukko);
                    gridi.Children.Add(laatta);
                    Grid.SetColumn(laatta, sar);
                    Grid.SetRow(laatta, riv);
                }
            }
        }


        /// <summary>
        /// Nappuloiden luonti (mustat ja valkoiset) ja arvotaan aloittaja
        /// </summary>
        public void LuoNappulat()
        {
            // ylänappulat
            int i = 0;
            for (int riv = 0; riv < 2; riv++)
            {
                for (int sar = 0; sar < kokoAlue / 2; sar++)
                {
                    Pelinappula.Pelinappula nappula = new Pelinappula.Pelinappula();
                    if (ylaPelaajanVari == null) ylaPelaajanVari = Brushes.Black;
                    Pelinappula.Pelinappula.SetVari(nappula, ylaPelaajanVari);
                    mustat_nappulat[i] = nappula;
                    gridi.Children.Add(nappula);
                    Grid.SetColumn(nappula, sar);
                    Grid.SetRow(nappula, riv);
                    m_paikat[i] = new Point(sar, riv);
                    i++;
                }
            }
            i = 0;
            for (int riv = (kokoAlue / 2 - 2); riv < kokoAlue / 2; riv++)
            {
                for (int sar = 0; sar < kokoAlue / 2; sar++)
                {
                    Pelinappula.Pelinappula nappula = new Pelinappula.Pelinappula();
                    if (alaPelaajanVari == null) alaPelaajanVari = Brushes.White;
                    Pelinappula.Pelinappula.SetVari(nappula, alaPelaajanVari);
                    valkoiset_nappulat[i] = nappula;
                    gridi.Children.Add(nappula);
                    Grid.SetColumn(nappula, sar);
                    Grid.SetRow(nappula, riv);
                    v_paikat[i] = new Point(sar, riv);
                    i++;
                }
            }
            // aloittajan arvonta
            Random rand = new Random();
            vuoro = rand.Next(0, 2);
        }


        /// <summary>
        /// Nappuloiden poisto (peliä uudelleen ladattaessa)
        /// </summary>
        public void PoistaNappulat()
        {
            for (int i = 0; i < kokoAlue; i++)
            {
                gridi.Children.Remove(mustat_nappulat[i]);
                gridi.Children.Remove(valkoiset_nappulat[i]);
            }
            vuoro = 0;
        }


        /// <summary>
        /// Nappulan liikuttaminen (ja mahdollienen syönti, kun klikattu, että halutaan siirtyä
        /// laatalle, jossa on joku)
        /// </summary>
        /// <param name="uusiPaikka">Paikka johon halutaan siirtyä</param>
        private void LiikutaNappulaa(Point uusiPaikka)
        {
            Pelinappula.Pelinappula[] t;
            Point[] p;
            String suunta = "";
            Point[] toinen_p;
            Pelinappula.Pelinappula[] toinen_t;
            List<Point> siirrot;
            if (vuoro == 0) // jos on valkoisten vuoro
            {
                t = valkoiset_nappulat;
                p = v_paikat;
                suunta = "ylos";
                toinen_p = m_paikat;
                toinen_t = mustat_nappulat;
                siirrot = valkoiset_siirrot;
            }
            else
            {
                if (vuoro >= 2) return; // jos vuoro ei ole kummankaan
                t = mustat_nappulat;
                p = m_paikat;
                suunta = "alas";
                toinen_p = v_paikat;
                toinen_t = valkoiset_nappulat;
                siirrot = mustat_siirrot;
            }
            int indeksi = EtsiIndeksi(siirrettavanNappulanPaikka, p);
            // jos indeksi on -1, jotain on pielessä
            if (indeksi == -1) return;
            Pelinappula.Pelinappula liikutettava = t[indeksi];

            if (!OnkoPaikkaValidi(p[indeksi], uusiPaikka, suunta, p, toinen_p)) return;

            SyoJaLiiku(siirrettavanNappulanPaikka, uusiPaikka, p, toinen_p, t, toinen_t, siirrot); 
        }


        /// <summary>
        /// Tutkitaan, onko haluttu paikka oikeanlainen
        /// </summary>
        /// <param name="nykPaikka">Nykyinen paikka</param>
        /// <param name="uusiPaikka">Haluttu uusi paikka</param>
        /// <param name="suunta">Mihin suuntaan pitäisi olla liikkumassa</param>
        /// <param name="oma_p">Vuorossa olevan pelaajan nappuloiden paikkojen taulukko</param>
        /// <param name="toinen_p">Toisen pelaajan nappuloiden paikkojen taulukko</param>
        /// <returns>Onko uusi paikka validi vai ei</returns>
        private bool OnkoPaikkaValidi(Point nykPaikka, Point uusiPaikka, String suunta, Point[] oma_p, Point[] toinen_p)
        {
            // jos uusi paikka on oman nappulan paikka
            for (int i = 0; i < kokoAlue / 2; i++)
            {
                if (uusiPaikka == oma_p[i]) return false;
            }
            // liian leveät hypyt
            if (uusiPaikka.X > nykPaikka.X + 1 || uusiPaikka.X < nykPaikka.X - 1) return false;
            if (suunta.Equals("ylos")) // valkoiset nappulat
            {
                // liian pitkät hypyt
                if (uusiPaikka.Y >= nykPaikka.Y || uusiPaikka.Y < nykPaikka.Y - 1) return false;
                // pattitilanne vastustajan nappulan kanssa
                if (EtsiIndeksi(uusiPaikka, toinen_p) > -1)
                {
                    if (uusiPaikka.X == nykPaikka.X && uusiPaikka.Y == nykPaikka.Y - 1) return false;
                    return true;
                }
                return true;
            }
            else // mustat nappulat
            {
                if (uusiPaikka.Y <= nykPaikka.Y || uusiPaikka.Y > nykPaikka.Y + 1) return false;
                if (EtsiIndeksi(uusiPaikka, toinen_p) > -1)
                {
                    if (uusiPaikka.X == nykPaikka.X && uusiPaikka.Y == nykPaikka.Y + 1) return false;
                    return true;
                }
                return true;
            }
        }


        /// <summary>
        /// Etsitään annetun paikan indeksi annetusta taulukosta
        /// </summary>
        /// <param name="paikka">Minkä indeksi etsitään</param>
        /// <param name="p">Mistä taulukosta etsitään</param>
        /// <returns>Indeksin jos löytyi, -1 jos ei</returns>
        private int EtsiIndeksi(Point paikka, Point[] p)
        {
            for (int i = 0; i < kokoAlue; i++)
            {
                if (p[i] == paikka)
                {
                    return i;
                }
            }
            return -1;
        }


        /// <summary>
        /// Vaihtaa nappulan väriä, jos sen saa valita ja muuttaa, mikä nappula on siirrettävänä
        /// </summary>
        /// <param name="mahdPaikka">Paikka, johon nappula olisi tarkoitus siirtää</param>
        /// <param name="paikat">Taulukko, jossa siirrettävän nappulan pitäisi olla</param>
        /// <param name="nappulat">Taulukko, jossa nappulan pitäisi olla</param>
        /// <param name="perus">Nappulan perusväri</param>
        /// <param name="valittu">Nappulan väri, kun se on valittu</param>
        private void VaihdaVariJaSiirrettava(Point mahdPaikka, Point[] paikat, Pelinappula.Pelinappula[] nappulat, SolidColorBrush perus, SolidColorBrush valittu)
        {
            int i = EtsiIndeksi(mahdPaikka, paikat);
            if (i != -1)
            {
                siirrettavanNappulanPaikka = mahdPaikka;
                // jos siirrettävä nappula löytyy taulukosta
                if (LoytyykoTaulukosta(siirrettavaNappula, nappulat)) Pelinappula.Pelinappula.SetVari(siirrettavaNappula, perus);
                Pelinappula.Pelinappula.SetVari(nappulat[i], valittu);
                siirrettavaNappula = nappulat[i]; // täällä vasta päivittyy nappula
            }
        }

        
        /// <summary>
        /// Käsitellään nappulan syönti ja syövän nappulan siirto sekä mahdollinen voitto
        /// // Tänne vielä siirtojen keräys!!
        /// </summary>
        /// <param name="syova">Nappula, joka liikkuu</param>
        /// <param name="syotava">Nappula, joka jää syönnin alle</param>
        /// <param name="syovat">Liikkuvan nappulan paikkataulukko</param>
        /// <param name="syotavat">Syönnin alle jäävän nappulan paikkataulukko</param>
        /// <param name="liikkuvat">Liikkuvan nappulan taulukko</param>
        /// <param name="poistuvat">Syonnin alle jäävän nappulan taulukko</param>
        private void SyoJaLiiku(Point syova, Point syotava, Point[] syovat, Point[] syotavat, 
            Pelinappula.Pelinappula[] liikkuvat, Pelinappula.Pelinappula[] poistuvat, List<Point> siirrot)
        {
            int indeksi = EtsiIndeksi(syova, syovat);
            if (indeksi == -1) return;
            Pelinappula.Pelinappula liikutettava = liikkuvat[indeksi];
            int rivi = (int)syotava.Y;
            int kolumni = (int)syotava.X;

            // otetaan nykyinen paikka talteen
            siirrot.Add(syova);
            // siirretään liikkuva nappula
            Grid.SetColumn(liikutettava, kolumni);
            Grid.SetRow(liikutettava, rivi);
            siirrot.Add(new Point(kolumni, rivi));

            // poistetaan poistettava nappula
            int i = EtsiIndeksi(syotava, syotavat);
            if (i != -1)
            {
                gridi.Children.Remove(poistuvat[i]);
                syotavat[i] = new Point(-1, -1);
            }

            if (vuoro == 0) // vuoron vaihto
            {
                v_paikat[indeksi] = syotava;
                Pelinappula.Pelinappula.SetVari(liikutettava, alaPelaajanVari);
                vuoro = 1;
                // jos nappula voitii
                if (syotava.Y <= 0) IlmoitaVoittaja(liikutettava);
            }
            else
            {
                if (vuoro == 1)
                {
                    m_paikat[indeksi] = syotava;
                    Pelinappula.Pelinappula.SetVari(liikutettava, ylaPelaajanVari);
                    vuoro = 0;
                    // jos nappula voitti
                    if (syotava.Y >= kokoAlue / 2 - 1) IlmoitaVoittaja(liikutettava);
                }
            }
        }


        /// <summary>
        /// Vaihdetaan voittaneen nappulan väri
        /// </summary>
        /// <param name="nappula">Minkä nappulan väri vaihdetaan</param>
        private void IlmoitaVoittaja(Pelinappula.Pelinappula nappula)
        {
            audio.Play();
            Pelinappula.Pelinappula.SetVari(nappula, Brushes.SkyBlue);
            vuoro = 2;
        }


        /// <summary>
        /// Käsitellään routedevent, kun nappulaa on klikattu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridi_NappulaArgs(object sender, RoutedEventArgs e)
        {
            Pelinappula.Pelinappula.NappulaEventArgs args = (Pelinappula.Pelinappula.NappulaEventArgs)e;
            Point klikatunPaikka = args.piste;
            if (vuoro == 0) 
            {
                SyoJaSiirry(klikatunPaikka, "ylos", v_paikat, m_paikat, valkoiset_nappulat, mustat_nappulat, alaPelaajanVari, Brushes.Salmon, valkoiset_siirrot);
            }
            else
            {
                if (vuoro == 1) // jos on mustan vuoro
                {
                    SyoJaSiirry(klikatunPaikka, "alas", m_paikat, v_paikat, mustat_nappulat, valkoiset_nappulat, ylaPelaajanVari, Brushes.Gray, mustat_siirrot);
                }
            }
        }


        /// <summary>
        /// Käsitellään värinvaihto ja mahdollinen syöminen (kun on klikattu nappulaa)
        /// </summary>
        /// <param name="klikatunPaikka">Paikka mitä on klikattu</param>
        /// <param name="suunta">Mihin suuntaan ollaan liikkumassa</param>
        /// <param name="omat_paikat">Omien nappuloiden paikkataulukko</param>
        /// <param name="toisen_paikat">Toisten nappuloiden paikkataulukko</param>
        /// <param name="omat">Omien nappuloiden taulukko</param>
        /// <param name="toiset">Toisten nappuloiden taulukko</param>
        /// <param name="perus">Nappulan perusväri</param>
        /// <param name="valittu">Nappulan väri kun se on valittu</param>
        private void SyoJaSiirry(Point klikatunPaikka, String suunta, Point[] omat_paikat, Point[] toisen_paikat,
            Pelinappula.Pelinappula[] omat, Pelinappula.Pelinappula[] toiset, SolidColorBrush perus, SolidColorBrush valittu, List<Point> siirrot)
        {
            VaihdaVariJaSiirrettava(klikatunPaikka, omat_paikat, omat, perus, valittu);
            // jos on klikattu omaa nappulaa mennään sisälle
            if (OnkoPaikkaValidi(siirrettavanNappulanPaikka, klikatunPaikka, suunta, omat_paikat, toisen_paikat))
            {
                SyoJaLiiku(siirrettavanNappulanPaikka, klikatunPaikka, omat_paikat, toisen_paikat, omat, toiset, siirrot);
            }
        }


        /// <summary>
        /// Etsitään annetusta taulukosta annettu nappula
        /// </summary>
        /// <param name="nap">Etsittävä nappula</param>
        /// <param name="t">Taulukko josta etsitään</param>
        /// <returns>Löytyikö nappulaa vai ei</returns>
        private bool LoytyykoTaulukosta(Pelinappula.Pelinappula nap, Pelinappula.Pelinappula[] t)
        {
            for (int i = 0; i < t.Length; i++)
            {
                if (t[i] == nap) return true;
            }
            return false;
        }


        /// <summary>
        /// Käsitellään routedevent, kun laattaa on klikattu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridi_LaattaArgs(object sender, RoutedEventArgs e)
        {
            Laatta.Laatta.LaattaEventArgs args = (Laatta.Laatta.LaattaEventArgs)e;
             Point uusiPaikka = args.piste;
            LiikutaNappulaa(uusiPaikka);
        }
    }
}
