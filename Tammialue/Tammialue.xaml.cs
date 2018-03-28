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

namespace Tammialue
{
    /// <summary>
    /// Interaction logic for Tammialue.xaml  // tammialueen luonti
    /// // tammeksi muuttuminen puuttuu animaation osalta
    /// </summary>
    public partial class Tammialue : UserControl
    {
        private static int kokoAlue;
        private SolidColorBrush ruudukkoTumma;
        private SolidColorBrush ruudukkoVaalea;
        private SolidColorBrush alaPelaajanVari;
        private SolidColorBrush ylaPelaajanVari;

        private List<Point> sallitutPaikat = new List<Point>();
        private List<Point> syotavatPaikat = new List<Point>();
        private Laatta.Laatta muutettuLaatta;
        private SolidColorBrush laatanOikeaVari;

        private Pelinappula.Pelinappula[] punaiset_nappulat;
        private Pelinappula.Pelinappula[] valkoiset_nappulat;
        private Point[] p_paikat;
        private Point[] v_paikat;

        private List<Point> punaiset_siirrot = new List<Point>();
        private List<Point> valkoiset_siirrot = new List<Point>();
        private List<Point> vapaat_paikat = new List<Point>();

        private int vuoro = 0;
        private Pelinappula.Pelinappula siirrettavaNappula = new Pelinappula.Pelinappula();
        private Point siirrettavanNappulanPaikka = new Point(-1, -1);
        private int taulukoidenKoko = 0;

        private List<Laatta.Laatta> laatat = new List<Laatta.Laatta>();
        private Point[] laattapaikat;

        private List<Point> voivat_syoda = new List<Point>();
        private Point ainut_joka_saa_liikkua = new Point(-1, -1);

        private bool onkoSyotyRekursiivisesti = false;


        /// <summary>
        /// Punaisten siirtojen property
        /// </summary>
        public List<Point> Punaiset_siirrot
        {
            get { return punaiset_siirrot; }
        }


        /// <summary>
        /// Valkoisten siirtojen property
        /// </summary>
        public List<Point> Valkoiset_siirrot
        {
            get { return valkoiset_siirrot; }
        }


        /// <summary>
        /// Alustetaan kontrolli ja siihen liittvät taulukot ja värit
        /// </summary>
        /// <param name="koko"></param>
        /// <param name="ruudukkoTumma"></param>
        /// <param name="ruudukkoVaalea"></param>
        /// <param name="alaPelaajanVari"></param>
        /// <param name="ylaPelaajanVari"></param>
        public Tammialue(int koko, SolidColorBrush ruudukkoTumma, SolidColorBrush ruudukkoVaalea, SolidColorBrush alaPelaajanVari,
            SolidColorBrush ylaPelaajanVari)
        {
            kokoAlue = koko;
            this.ruudukkoTumma = ruudukkoTumma;
            this.ruudukkoVaalea = ruudukkoVaalea;
            this.alaPelaajanVari = alaPelaajanVari;
            this.ylaPelaajanVari = ylaPelaajanVari;

            laattapaikat = new Point[kokoAlue * kokoAlue];

            InitializeComponent();
        }


        /// <summary>
        /// Kun grid on ladattu, luodaan ruudukko ja nappulat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridi_Loaded(object sender, RoutedEventArgs e)
        {
            LuoRuudukko();
            LuoNappulat();
        }


        /// <summary>
        /// Luodaan gridiin rivit, kolumnit ja laatta-kontrollit
        /// </summary>
        public void LuoRuudukko()
        {
            double koko = this.Height / (kokoAlue / 2);
            SolidColorBrush edellinenVari = ruudukkoVaalea;
            SolidColorBrush seuraavaVari = ruudukkoTumma;

            int i = 0;
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

                    if (ruudukkoTumma == null) ruudukkoTumma = Brushes.DarkGreen;
                    if (ruudukkoVaalea == null) ruudukkoVaalea = Brushes.Wheat;

                    Laatta.Laatta laatta = new Laatta.Laatta();
                    SolidColorBrush maalattavaVari = ruudukkoVaalea;
                    if (edellinenVari == ruudukkoVaalea)
                    {
                        maalattavaVari = ruudukkoTumma;
                        sallitutPaikat.Add(new Point(sar, riv));
                        vapaat_paikat.Add(new Point(sar, riv));
                        if (riv < 3) taulukoidenKoko++;
                    }
                    Laatta.Laatta.SetVari(laatta, maalattavaVari);
                    gridi.Children.Add(laatta);
                    Grid.SetColumn(laatta, sar);
                    Grid.SetRow(laatta, riv);

                    laatat.Add(laatta);
                    laattapaikat[i] = new Point(sar, riv);

                    seuraavaVari = edellinenVari;
                    edellinenVari = maalattavaVari;
                    i++;
                }
                if (kokoAlue / 2 % 2 == 0) edellinenVari = seuraavaVari;
            }
            punaiset_nappulat = new Pelinappula.Pelinappula[taulukoidenKoko];
            valkoiset_nappulat = new Pelinappula.Pelinappula[taulukoidenKoko];
            p_paikat = new Point[taulukoidenKoko];
            v_paikat = new Point[taulukoidenKoko];
        }


        /// <summary>
        /// Luodaan nappulat
        /// </summary>
        public void LuoNappulat()
        {
            for (int i = 0; i < sallitutPaikat.Count / 2; i++)
            {
                int sar = (int)sallitutPaikat[i].X;
                int riv = (int)sallitutPaikat[i].Y;
                if (riv >= 3) break;

                Pelinappula.Pelinappula nappula = new Pelinappula.Pelinappula();
                if (ylaPelaajanVari == null) ylaPelaajanVari = Brushes.White;
                Pelinappula.Pelinappula.SetVari(nappula, ylaPelaajanVari);
                valkoiset_nappulat[i] = nappula;
                gridi.Children.Add(nappula);

                Grid.SetColumn(nappula, sar);
                Grid.SetRow(nappula, riv);
                v_paikat[i] = sallitutPaikat[i];
                vapaat_paikat.Remove(new Point(sar, riv));
            }

            int indeksi = taulukoidenKoko - 1;
            for (int j = sallitutPaikat.Count - 1; j > 0; j--)
            {
                int sar = (int)sallitutPaikat[j].X;
                int riv = (int)sallitutPaikat[j].Y;
                if (riv < kokoAlue / 2 - 3) break;

                Pelinappula.Pelinappula nappula = new Pelinappula.Pelinappula();
                if (alaPelaajanVari == null) alaPelaajanVari = Brushes.DarkRed;
                Pelinappula.Pelinappula.SetVari(nappula, alaPelaajanVari);
                punaiset_nappulat[indeksi] = nappula;
                gridi.Children.Add(nappula);

                Grid.SetColumn(nappula, sar);
                Grid.SetRow(nappula, riv);
                p_paikat[indeksi] = sallitutPaikat[j];
                vapaat_paikat.Remove(new Point(sar, riv));
                indeksi--;
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
            
            for (int i = 0; i < taulukoidenKoko; i++)
            {
                gridi.Children.Remove(valkoiset_nappulat[i]);
                gridi.Children.Remove(punaiset_nappulat[i]);
            }
            vuoro = 0;
        }


        /// <summary>
        /// Syodaan nappulat, jotka pitää
        /// </summary>
        /// <param name="syotavat">Syötävät nappulat (paikat)</param>
        /// <param name="syotavatNappulat">Pelaajan nappulat, jonka nappuloita syödään</param>
        /// <param name="syotavienPaikat">Syötävän pelaajan nappuloiden paikat</param>
        /// <param name="alku">Syövän nappulan alkupaikka</param>
        /// <param name="loppu">Syövän nappulan loppupaikka</param>
        /// <returns>-1, jos ei syöty, 0 jos on</returns>
        private int Syo(List<Point> syotavat, Pelinappula.Pelinappula[] syotavatNappulat, Point[] syotavienPaikat, Point alku, Point loppu)
        {
            int i = 0;
            if (syotavat.Count <= 0) return -1;
            Point syotava = syotavat[0];
            if (alku.Equals(new Point(0, 0)) && loppu.Equals(new Point(0, 0))) i = EtsiIndeksi(syotava, syotavienPaikat, syotavienPaikat.Length);
            else // kun on seampi mahdollisuus syödä
            {
                List<int> indeksit = new List<int>();
                Point syotavaNappula = new Point(-1, -1);

                int x = -1;
                int y = -1;
                if (alku.X + 1 == loppu.X - 1) x = (int)alku.X + 1;
                if (alku.X - 1 == loppu.X + 1) x = (int)alku.X - 1;

                if (alku.Y + 1 == loppu.Y - 1) y = (int)loppu.Y - 1;
                if (alku.Y - 1 == loppu.Y + 1) y = (int)loppu.Y + 1;
                syotavaNappula.X = x; // nyt tiedetään syötävän paikka
                syotavaNappula.Y = y;
                // haetaan kaikkien syötävien indeksit syotavienPaikat-taulukosta
                for (int j = 0; j < syotavat.Count; j++)
                {
                    indeksit.Add(EtsiIndeksi(syotavat[j], syotavienPaikat, syotavienPaikat.Length));
                }
                // etsitään oikea indeksi syötävän nappulan paikalle
                for (int k = 0; k < syotavat.Count; k++)
                {
                    if (syotavienPaikat[indeksit[k]].Equals(syotavaNappula)) i = indeksit[k];
                }
                syotava = syotavaNappula;
            }
            if (i != -1)
            {
                gridi.Children.Remove(syotavatNappulat[i]);
                syotavienPaikat[i] = new Point(-1, -1);
                // tähän tarkistus, onko kaikki nappulat syöty, ja jos on ilmoitetaan voittaja
                if (TarkistaOnkoPeliVoitettu(syotavienPaikat)) IlmoitaVoittaja();
                vapaat_paikat.Add(syotava);
                syotavatPaikat = new List<Point>();
                return 0;
            }
            return -1;
        }


        /// <summary>
        /// Tarkistetaan onko peli voitettu syömällä vastustajan kaikki nappulat
        /// </summary>
        /// <param name="syotavienPaikat">Nappulat, joita on syöty (paikat)</param>
        /// <returns>true jos voitettiin, false jos ei</returns>
        private bool TarkistaOnkoPeliVoitettu(Point[] syotavienPaikat)
        {
            for (int i = 0; i < syotavienPaikat.Length; i++)
            {
                if (!syotavienPaikat[i].Equals(new Point(-1, -1))) return false;
            }
            return true;
        }


        /// <summary>
        /// Vaihdetaan voittaneen nappulan väri
        /// </summary>
        /// <param name="nappula">Minkä nappulan väri vaihdetaan</param>
        private void IlmoitaVoittaja()
        {
            audio.Play();
            Pelinappula.Pelinappula.SetVari(siirrettavaNappula, Brushes.SkyBlue);
            vuoro = 2;
        }


        /// <summary>
        /// Tarkastetaan voiko pelaajan nappulat syödä
        /// </summary>
        /// <param name="nappulat">Pelaajan nappulat</param>
        /// <param name="paikat">Pelaajan nappuloiden paikat</param>
        /// <param name="suunta">Mihin suuntaan pelaaja pelaa</param>
        /// <param name="toisen_paikat">Vastustajan nappuloiden paikat</param>
        /// <returns></returns>
        private List<Point> VoikoPelaajanNappulatSyoda(Pelinappula.Pelinappula[] nappulat, Point[] paikat, String suunta, Point[] toisen_paikat)
        {
            String nappulanSuunta = suunta;
            List<Point> voivat_syoda_paikat = new List<Point>();
            for (int i = 0; i < paikat.Length; i++)
            {
                if (nappulat[i].OnkoTammi) nappulanSuunta = "kumpikin";
                else nappulanSuunta = suunta;
                List<Point> mahdpaikat = EtsiMahdollisetPaikat(nappulat, i, paikat, nappulanSuunta, toisen_paikat); // pitää tarkentaa suuntaa nappulakohtaiseksi!!
                if (mahdpaikat.Count > 0)
                {
                    for (int j = 0; j < mahdpaikat.Count; j++)
                    {
                        if (OnkoSyonninLoppupaikka(paikat[i], mahdpaikat[j])) voivat_syoda_paikat.Add(paikat[i]);
                    }
                }
            }
            return voivat_syoda_paikat;
        }


        /// <summary>
        /// Tutkitaan onko loppupaikkaan päästy syömällä
        /// </summary>
        /// <param name="alku">Alkupaikka</param>
        /// <param name="loppu">Loppupaikka</param>
        /// <returns></returns>
        private bool OnkoSyonninLoppupaikka(Point alku, Point loppu)
        {
            if (alku.X + 2 == loppu.X) return true;
            if (alku.X - 2 == loppu.X) return true;

            if (alku.Y + 2 == loppu.Y) return true;
            if (alku.Y - 2 == loppu.Y) return true;
            return false;
        }


        /// <summary>
        /// Tutkitaan onko nappuloilla mahdollisia siirtoja
        /// </summary>
        /// <param name="nappulat">Tutkittavat nappulat</param>
        /// <param name="paikat">Tutkittavien nappuloiden paikat</param>
        /// <param name="suunta">Nappuloiden suunta</param>
        /// <param name="toisen_paikat">Toisten nappuloiden paikat</param>
        /// <returns>true jos on, false jos ei</returns>
        private bool OnkoMahdollisiaSiirtoja(Pelinappula.Pelinappula[] nappulat, Point[] paikat, String suunta, Point[] toisen_paikat)
        {
            String nappulanSuunta = suunta;
            for (int i = 0; i < paikat.Length; i++)
            {
                if (nappulat[i].OnkoTammi) nappulanSuunta = "kumpikin";
                else nappulanSuunta = suunta;
                List<Point> mahdpaikat = EtsiMahdollisetPaikat(nappulat, i, paikat, nappulanSuunta, toisen_paikat);
                if (mahdpaikat.Count > 0) return true;
            }
            return false;
        }


        /// <summary>
        /// Ilmoitetaan voittaja, kun pattitilanne vastustajalla
        /// </summary>
        /// <param name="voittajat">Voittaneen pelaajan nappulat</param>
        private void IlmoitaVoittaja(Pelinappula.Pelinappula[] voittajat)
        {
            for (int i = 0; i < voittajat.Length; i++)
            {
                Pelinappula.Pelinappula.SetVari(voittajat[i], Brushes.SkyBlue);
            }
        }


        /// <summary>
        /// Käsitellään nappulan klikkaus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridi_NappulaArgs(object sender, RoutedEventArgs e)
        {
            if (muutettuLaatta != null) Laatta.Laatta.SetVari(muutettuLaatta, laatanOikeaVari);
            syotavatPaikat = new List<Point>();
            Pelinappula.Pelinappula.NappulaEventArgs args = (Pelinappula.Pelinappula.NappulaEventArgs)e;
            Point klikatunPaikka = args.piste;
            if (vuoro == 0) // jos on punaisten vuoro
            {
                String suunta = "ylos";
                if (!OnkoMahdollisiaSiirtoja(punaiset_nappulat, p_paikat, suunta, v_paikat)) IlmoitaVoittaja(valkoiset_nappulat);
                int i = EtsiIndeksi(klikatunPaikka, p_paikat, taulukoidenKoko);
                if (i == -1) return; // jos paikassa ei ole punaista nappulaa

                voivat_syoda = VoikoPelaajanNappulatSyoda(punaiset_nappulat, p_paikat, suunta, v_paikat);
                if (voivat_syoda.Count > 0)
                {
                    if (!ainut_joka_saa_liikkua.Equals(new Point(-1, -1)))
                        if (!voivat_syoda.Contains(ainut_joka_saa_liikkua)) return;
                    if (!voivat_syoda.Contains(klikatunPaikka)) return;
                }
                syotavatPaikat = new List<Point>();
                VaihdaSiirrettavaa(suunta, klikatunPaikka, p_paikat, taulukoidenKoko, punaiset_nappulat, i, alaPelaajanVari, Brushes.DarkMagenta);
                SyoRekursiivisesti(punaiset_nappulat, p_paikat, i, suunta, v_paikat, punaiset_siirrot, valkoiset_nappulat, alaPelaajanVari, 1);
            }
            else
            {
                if (vuoro == 1) // jos on valkoisten vuoro
                {
                    String suunta = "alas";
                    int i = EtsiIndeksi(klikatunPaikka, v_paikat, taulukoidenKoko);
                    if (i == -1) return;

                    voivat_syoda = VoikoPelaajanNappulatSyoda(valkoiset_nappulat, v_paikat, suunta, p_paikat);
                    if (voivat_syoda.Count > 0)
                    {
                        if (!ainut_joka_saa_liikkua.Equals(new Point(-1, -1)))
                            if (!voivat_syoda.Contains(ainut_joka_saa_liikkua)) return;
                        if (!voivat_syoda.Contains(klikatunPaikka)) return;
                    }
                    syotavatPaikat = new List<Point>();
                    VaihdaSiirrettavaa(suunta, klikatunPaikka, v_paikat, taulukoidenKoko, valkoiset_nappulat, i, ylaPelaajanVari, Brushes.DarkGray);
                    SyoRekursiivisesti(valkoiset_nappulat, v_paikat, i, suunta, p_paikat, valkoiset_siirrot, punaiset_nappulat, ylaPelaajanVari, 0);
                }
            }
        }


        /// <summary>
        /// Syödään nappuloita rekursiivisesti
        /// </summary>
        /// <param name="omat_nappulat">Omat nappulat</param>
        /// <param name="omat_paikat">Omien nappuloiden paikat</param>
        /// <param name="i">Mikä indeksi</param>
        /// <param name="suunta">Nappuloiden suunta</param>
        /// <param name="toisen_paikat">Toisten nappuloiden paikat</param>
        /// <param name="omat_siirrot">Omat siirrot</param>
        /// <param name="toiset_nappulat">Toisten nappuloiden paikat</param>
        /// <param name="omaperusvari">Oma perusväri nappuloille</param>
        /// <param name="vuoro_vaihtuu">Toisen vuoro</param>
        private void SyoRekursiivisesti(Pelinappula.Pelinappula[] omat_nappulat, Point[] omat_paikat, int i, String suunta,
            Point[] toisen_paikat, List<Point> omat_siirrot, Pelinappula.Pelinappula[] toiset_nappulat, SolidColorBrush omaperusvari,
            int vuoro_vaihtuu)
        {
            Point paikka = OnkoYksiMahdollinenSiirto(omat_nappulat, omat_paikat, i, suunta, toisen_paikat);
            if (onkoSyotyRekursiivisesti)
            {
                // tarkistus, voiko nykyinen nappula syödä vielä useamman, jolloin se on ainut nappula jota voi liikuttaa
                voivat_syoda = VoikoPelaajanNappulatSyoda(omat_nappulat, omat_paikat, suunta, toisen_paikat);
                if (voivat_syoda.Contains(omat_paikat[i]))
                    ainut_joka_saa_liikkua = omat_paikat[i];
                else
                {
                    Pelinappula.Pelinappula.SetVari(siirrettavaNappula, omaperusvari);
                    ainut_joka_saa_liikkua = new Point(-1, -1);
                    vuoro = vuoro_vaihtuu;
                }
            }
            if (!paikka.Equals(new Point(-1, -1)))
            {
                // tarkistus, onko uusi paikka ei-syomispaikka ja ollaan kuitenkin rekursiivisesti liikuttu
                if (onkoSyotyRekursiivisesti)
                {
                    if (OnkoPerussiirto(paikka))
                    {
                        onkoSyotyRekursiivisesti = false;
                        vuoro = vuoro_vaihtuu;
                        ainut_joka_saa_liikkua = new Point(-1, -1);
                        return;
                    }
                }
                // automaattinen syönti ja/tai likutus
                SiirraNappulaa(paikka, omat_paikat, i, omat_siirrot, vuoro_vaihtuu);
                int tulos = Syo(syotavatPaikat, toiset_nappulat, toisen_paikat, new Point(0, 0), new Point(0, 0)); // vähän tyhmästi
                if (tulos < 0) // jos ei syoty nappulaa
                {
                    // lopulta värin ja vuoron vaihto
                    Pelinappula.Pelinappula.SetVari(siirrettavaNappula, omaperusvari);
                    vuoro = vuoro_vaihtuu;
                    siirrettavanNappulanPaikka = new Point(-1, -1);
                    return;
                }
                if (vuoro == 2) return;
                if (vuoro == vuoro_vaihtuu)
                {
                    Pelinappula.Pelinappula.SetVari(siirrettavaNappula, omaperusvari);
                    return;
                }
                // jos useampi syönti voidaan tehdä, niin rekursiivisesti
                onkoSyotyRekursiivisesti = true;
                SyoRekursiivisesti(omat_nappulat, omat_paikat, i, suunta, toisen_paikat, omat_siirrot, toiset_nappulat, omaperusvari,
                    vuoro_vaihtuu);
            }
            onkoSyotyRekursiivisesti = false;
        }


        /// <summary>
        /// Tutkitaan onko siirto perussiirto
        /// </summary>
        /// <param name="paikka"></param>
        /// <returns></returns>
        private bool OnkoPerussiirto(Point paikka)
        {
            if (Math.Abs(siirrettavanNappulanPaikka.X - paikka.X) > 1) return false;
            return true;
        }


        /// <summary>
        /// Vaihdetaan siirrettävää
        /// </summary>
        /// <param name="suunta">Nappuloiden suunat</param>
        /// <param name="klikatunPaikka">Klikatun paikka</param>
        /// <param name="omat_paikat">Omien nappuloiden paikka</param>
        /// <param name="taulukoidenKoko">Taulukoiden koko</param>
        /// <param name="omat_nappulat">Omat nappulat</param>
        /// <param name="i">Mikä indeksi</param>
        /// <param name="perusvari">Nappuloiden perusväri</param>
        /// <param name="korostusvari">Nappuloiden korostusväri</param>
        private void VaihdaSiirrettavaa(String suunta, Point klikatunPaikka, Point[] omat_paikat, int taulukoidenKoko,
            Pelinappula.Pelinappula[] omat_nappulat, int i, SolidColorBrush perusvari, SolidColorBrush korostusvari)
        {
            if (LoytyykoTaulukosta(siirrettavaNappula, omat_nappulat)) Pelinappula.Pelinappula.SetVari(siirrettavaNappula, perusvari);
            Pelinappula.Pelinappula.SetVari(omat_nappulat[i], korostusvari);
            siirrettavaNappula = omat_nappulat[i];
            siirrettavanNappulanPaikka = omat_paikat[i];
        }


        /// <summary>
        /// Käsitellään laatan klikkaus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridi_LaattaArgs(object sender, RoutedEventArgs e) // puuttu vielä syönti
        {
            Laatta.Laatta.LaattaEventArgs args = (Laatta.Laatta.LaattaEventArgs)e;
            Point uusiPaikka = args.piste;
            syotavatPaikat = new List<Point>();
            if (muutettuLaatta != null) Laatta.Laatta.SetVari(muutettuLaatta, laatanOikeaVari);

            int i = EtsiIndeksi(siirrettavanNappulanPaikka, p_paikat, taulukoidenKoko);


            if (i == -1) // siirrettävää ei löydy punaisista nappuloista
            {
                if (KasitteleVaaraLaatta(uusiPaikka)) return;
                if (vuoro == 0) return;
                String suunta = "alas";
                i = EtsiIndeksi(siirrettavanNappulanPaikka, v_paikat, taulukoidenKoko);
                if (valkoiset_nappulat[i].OnkoTammi) suunta = "kumpikin";
                List<Point> mahdPaikat = EtsiMahdollisetPaikat(valkoiset_nappulat, i, v_paikat, suunta, p_paikat);
                if (mahdPaikat.Contains(uusiPaikka))
                {
                    int tulos = TeeLiikutusJaSyonti(valkoiset_nappulat, uusiPaikka, v_paikat, i, valkoiset_siirrot, syotavatPaikat, punaiset_nappulat, p_paikat, 0);
                    voivat_syoda = VoikoPelaajanNappulatSyoda(valkoiset_nappulat, v_paikat, suunta, p_paikat);
                    if (tulos == 0)
                    {
                        if (voivat_syoda.Contains(v_paikat[i]))
                            ainut_joka_saa_liikkua = v_paikat[i];
                        else
                        {
                            ainut_joka_saa_liikkua = new Point(-1, -1);
                            vuoro = 0;
                        }
                    }
                    else vuoro = 0;
                }
                else
                {
                    MuutaLaatanVaria(uusiPaikka);
                    return;
                }
                if (LoytyykoTaulukosta(siirrettavaNappula, valkoiset_nappulat))
                    Pelinappula.Pelinappula.SetVari(siirrettavaNappula, ylaPelaajanVari);
                siirrettavanNappulanPaikka = new Point(-1, -1);
            }
            else
            {
                if (KasitteleVaaraLaatta(uusiPaikka)) return;
                if (vuoro != 0) return;
                String suunta = "ylos";
                if (punaiset_nappulat[i].OnkoTammi) suunta = "kumpikin";
                List<Point> mahdPaikat = EtsiMahdollisetPaikat(punaiset_nappulat, i, p_paikat, suunta, v_paikat);
                if (mahdPaikat.Contains(uusiPaikka))
                {
                    int tulos = TeeLiikutusJaSyonti(punaiset_nappulat, uusiPaikka, p_paikat, i, punaiset_siirrot, syotavatPaikat, valkoiset_nappulat, v_paikat, 1);
                    voivat_syoda = VoikoPelaajanNappulatSyoda(punaiset_nappulat, p_paikat, suunta, v_paikat);
                    if (tulos == 0)
                    {
                        if (voivat_syoda.Contains(p_paikat[i]))
                            ainut_joka_saa_liikkua = p_paikat[i];
                        else vuoro = 1;
                    }
                    else
                    {
                        ainut_joka_saa_liikkua = new Point(-1, -1);
                        vuoro = 1;
                    }
                }
                else
                {
                    MuutaLaatanVaria(uusiPaikka);
                    return;
                }
                if (LoytyykoTaulukosta(siirrettavaNappula, punaiset_nappulat))
                    Pelinappula.Pelinappula.SetVari(siirrettavaNappula, alaPelaajanVari);
                siirrettavanNappulanPaikka = new Point(-1, -1);
            }
        }


        /// <summary>
        /// Liikutetaan nappulaa ja syödään (kun klikattu laattaa)
        /// </summary>
        /// <param name="omat_nappulat">Omat nappulat</param>
        /// <param name="uusiPaikka">Uusi liikutettavan nappulan paikka</param>
        /// <param name="omat_paikat">Omat paikat</param>
        /// <param name="i">Mikä indeksi</param>
        /// <param name="omat_siirrot">Omat siirrot</param>
        /// <param name="syotavatPaikat">Syötävät paikat</param>
        /// <param name="toiset_nappulat">Toisen pelaajan nappulat</param>
        /// <param name="toiset_paikat">Toisen pelaajan nappuloiden paikat</param>
        /// <param name="vuoro_vaihtuu">Toisen vuoro</param>
        /// <returns></returns>
        private int TeeLiikutusJaSyonti(Pelinappula.Pelinappula[] omat_nappulat, Point uusiPaikka, Point[] omat_paikat, int i,
            List<Point> omat_siirrot, List<Point> syotavatPaikat, Pelinappula.Pelinappula[] toiset_nappulat, Point[] toiset_paikat, int vuoro_vaihtuu)
        {
            Point alkupaikka = siirrettavanNappulanPaikka;
            SiirraNappulaa(uusiPaikka, omat_paikat, i, omat_siirrot, vuoro_vaihtuu);
            Point loppupaikka = siirrettavanNappulanPaikka;
            int tulos = Syo(syotavatPaikat, toiset_nappulat, toiset_paikat, alkupaikka, loppupaikka);
            return tulos;
        }


        /// <summary>
        /// Käsitellään tilanne, jos klikattu laatta on laiton
        /// </summary>
        /// <param name="uusiPaikka">Nappulan uusi paikka</param>
        /// <returns>true jos on laiton, false jos ei</returns>
        private bool KasitteleVaaraLaatta(Point uusiPaikka)
        {
            bool olikoLaiton = false;
            if (!vapaat_paikat.Contains(uusiPaikka) || siirrettavanNappulanPaikka.Equals(new Point(-1, -1)))
            {
                MuutaLaatanVaria(uusiPaikka);
                olikoLaiton = true;
            }
            return olikoLaiton;
        }


        /// <summary>
        /// Muutetaan laatan väriä
        /// </summary>
        /// <param name="uusiPaikka">Laatan paikka</param>
        private void MuutaLaatanVaria(Point paikka)
        {
            int indeksi = EtsiIndeksi(paikka, laattapaikat, laattapaikat.Length);
            laatanOikeaVari = (SolidColorBrush)Laatta.Laatta.GetVari(laatat[indeksi]);
            Laatta.Laatta.SetVari(laatat[indeksi], Brushes.Red); //laittoman paikan käsittely
            muutettuLaatta = laatat[indeksi];
        }


        /// <summary>
        /// Siirretään nappulaa (täällä tammeksi animoimisen kutsu, mikä ei toimi)
        /// </summary>
        /// <param name="uusiPaikka">Nappulan uusi paikka</param>
        /// <param name="nappulanPaikat">Nappuloiden paikat</param>
        /// <param name="i">Mikä indeksi</param>
        /// <param name="siirrot">Nappuloiden siirot</param>
        /// <param name="vuoro_vaihtuu">Toisen vuoro</param>
        private void SiirraNappulaa(Point uusiPaikka, Point[] nappulanPaikat, int i, List<Point> siirrot, int vuoro_vaihtuu)
        {
            // siirto ja taulukoiden päivitys
            siirrot.Add(siirrettavanNappulanPaikka);
            // ruutu vapautuu
            vapaat_paikat.Add(siirrettavanNappulanPaikka);
            nappulanPaikat[i] = uusiPaikka;

            int kolumni = (int)uusiPaikka.X;
            int rivi = (int)uusiPaikka.Y;

            // siirretään nappulaa
            Grid.SetColumn(siirrettavaNappula, kolumni);
            Grid.SetRow(siirrettavaNappula, rivi);

            // päivitetään paikka
            siirrettavanNappulanPaikka = new Point(kolumni, rivi);
            siirrot.Add(new Point(kolumni, rivi));
            vapaat_paikat.Remove(siirrettavanNappulanPaikka);

            // tänne tarkitus, onko päästy vastustajan päähän --> muutetaan nappula tammeksi
            if (siirrettavanNappulanPaikka.Y == 0 || siirrettavanNappulanPaikka.Y >= kokoAlue / 2 - 1)
            {
                if (siirrettavaNappula.OnkoTammi) return;
                siirrettavaNappula.OnkoTammi = true;
                siirrettavaNappula.MuutaTammeksi();
                vuoro = vuoro_vaihtuu;
            }
        }


        /// <summary>
        /// Etsitään annetun paikan indeksi annetusta taulukosta
        /// </summary>
        /// <param name="paikka">Minkä indeksi etsitään</param>
        /// <param name="p">Mistä taulukosta etsitään</param>
        /// <returns>Indeksin jos löytyi, -1 jos ei</returns>
        private int EtsiIndeksi(Point paikka, Point[] p, int pituus)
        {
            for (int i = 0; i < pituus; i++)
            {
                if (p[i] == paikka)
                {
                    return i;
                }
            }
            return -1;
        }


        /// <summary>
        /// Tutkitaan löytyykö nappula taulukosta
        /// </summary>
        /// <param name="nap">Mikä nappula</param>
        /// <param name="t">Mikä talukko</param>
        /// <returns>true jos löytyy, false jos ei</returns>
        private bool LoytyykoTaulukosta(Pelinappula.Pelinappula nap, Pelinappula.Pelinappula[] t)
        {
            for (int i = 0; i < t.Length; i++)
            {
                if (t[i] == nap) return true;
            }
            return false;
        }


        /// <summary>
        /// Tutkitaan onko nappulalla vain yksi mahdollinen siirto
        /// </summary>
        /// <param name="nappulat">Nappulat</param>
        /// <param name="napPaikkaTaulukko">Nappuloiden paikat</param>
        /// <param name="indeksi">Mikä indeksi</param>
        /// <param name="suunta">Nappuloiden suunta</param>
        /// <param name="toisen_paikat">Vastustajan nappuloiden paikat</param>
        /// <returns>Mahdollisen paikan jos 1, (-1,-1) jos monta tai ei ollenkaan</returns>
        private Point OnkoYksiMahdollinenSiirto(Pelinappula.Pelinappula[] nappulat, Point[] napPaikkaTaulukko, int indeksi,
            String suunta, Point[] toisen_paikat)
        {
            String nappulanSuunta = suunta;
            if (nappulat[indeksi].OnkoTammi) nappulanSuunta = "kumpikin";
            List<Point> paikat = EtsiMahdollisetPaikat(nappulat, indeksi, napPaikkaTaulukko, nappulanSuunta, toisen_paikat);
            // jos listassa on vain yksi palautetaan tämä
            if (paikat.Count == 1) return paikat[0];
            return new Point(-1, -1);
        }


        /// <summary>
        /// Etsitään nappulan mahdolliset paikat
        /// </summary>
        /// <param name="nappulat">Nappulan taulukko</param>
        /// <param name="indeksi">Mikä indeksi</param>
        /// <param name="paikat">Nappulan paikkataulukko</param>
        /// <param name="suunta">Nappulan suunta</param>
        /// <param name="toisen_paikat">Vastustajan nappuloiden paikat</param>
        /// <returns></returns>
        private List<Point> EtsiMahdollisetPaikat(Pelinappula.Pelinappula[] nappulat, int indeksi, Point[] paikat,
            String suunta, Point[] toisen_paikat)
        {
            List<Point> mahdPaikat = new List<Point>();

            Point siirrettavanPaikka = paikat[indeksi];
            if (siirrettavanPaikka.Equals(new Point(-1, -1))) return mahdPaikat;
            int sarake = (int)siirrettavanPaikka.X;
            int rivi = (int)siirrettavanPaikka.Y;


            if (suunta.Equals("ylos"))
            {
                Point kokeilu = new Point(sarake + 1, rivi - 1);
                Point kokeilu2 = new Point(sarake - 1, rivi - 1);

                if (!sallitutPaikat.Contains(kokeilu) && !sallitutPaikat.Contains(kokeilu2)) return mahdPaikat;
                Point takana = new Point(sarake + 2, rivi - 2);
                Point takana2 = new Point(sarake - 2, rivi - 2);
                if (!vapaat_paikat.Contains(takana) && !vapaat_paikat.Contains(takana2))
                {
                    if (sallitutPaikat.Contains(kokeilu) && vapaat_paikat.Contains(kokeilu)) mahdPaikat.Add(kokeilu);
                    if (sallitutPaikat.Contains(kokeilu2) && vapaat_paikat.Contains(kokeilu2)) mahdPaikat.Add(kokeilu2);
                    return mahdPaikat;
                }
                mahdPaikat = TarkistaLoput(kokeilu, kokeilu2, takana, takana2, toisen_paikat);
            }
            if (suunta.Equals("alas"))
            {
                Point kokeilu = new Point(sarake + 1, rivi + 1);
                Point kokeilu2 = new Point(sarake - 1, rivi + 1);

                if (!sallitutPaikat.Contains(kokeilu) && !sallitutPaikat.Contains(kokeilu2)) return mahdPaikat;
                Point takana = new Point(sarake + 2, rivi + 2);
                Point takana2 = new Point(sarake - 2, rivi + 2);
                if (!vapaat_paikat.Contains(takana) && !vapaat_paikat.Contains(takana2))
                {
                    if (sallitutPaikat.Contains(kokeilu) && vapaat_paikat.Contains(kokeilu)) mahdPaikat.Add(kokeilu);
                    if (sallitutPaikat.Contains(kokeilu2) && vapaat_paikat.Contains(kokeilu2)) mahdPaikat.Add(kokeilu2);
                    return mahdPaikat;
                }
                mahdPaikat = TarkistaLoput(kokeilu, kokeilu2, takana, takana2, toisen_paikat);
            }
            if (suunta.Equals("kumpikin"))
            {
                Point kokeilu = new Point(sarake + 1, rivi - 1);
                Point kokeilu2 = new Point(sarake - 1, rivi - 1);
                Point kokeilu3 = new Point(sarake + 1, rivi + 1);
                Point kokeilu4 = new Point(sarake - 1, rivi + 1);

                if (!sallitutPaikat.Contains(kokeilu) && !sallitutPaikat.Contains(kokeilu2) && !sallitutPaikat.Contains(kokeilu3)
                    && !sallitutPaikat.Contains(kokeilu4)) return mahdPaikat;
                Point takana = new Point(sarake + 2, rivi - 2);
                Point takana2 = new Point(sarake - 2, rivi - 2);
                Point takana3 = new Point(sarake + 2, rivi + 2);
                Point takana4 = new Point(sarake - 2, rivi + 2);

                if (!vapaat_paikat.Contains(takana) && !vapaat_paikat.Contains(takana2) && !vapaat_paikat.Contains(takana3) && 
                    !vapaat_paikat.Contains(takana4))
                {
                    if (sallitutPaikat.Contains(kokeilu) && vapaat_paikat.Contains(kokeilu)) mahdPaikat.Add(kokeilu);
                    if (sallitutPaikat.Contains(kokeilu2) && vapaat_paikat.Contains(kokeilu2)) mahdPaikat.Add(kokeilu2);
                    if (sallitutPaikat.Contains(kokeilu3) && vapaat_paikat.Contains(kokeilu3)) mahdPaikat.Add(kokeilu);
                    if (sallitutPaikat.Contains(kokeilu4) && vapaat_paikat.Contains(kokeilu4)) mahdPaikat.Add(kokeilu2);
                    return mahdPaikat;
                }
                List<Point> osa_paikat = TarkistaLoput(kokeilu, kokeilu2, takana, takana2, toisen_paikat);
                List<Point> toinen_osa = TarkistaLoput(kokeilu3, kokeilu4, takana3, takana4, toisen_paikat);
                List<Point> palautus = new List<Point>();
                if (syotavatPaikat.Count <= 0)
                {
                    osa_paikat.AddRange(toinen_osa);
                    palautus.AddRange(osa_paikat);
                }
                else
                {
                    if ((OnkoSyonninLoppupaikka(siirrettavanPaikka, takana) && osa_paikat.Contains(takana)) || (OnkoSyonninLoppupaikka(siirrettavanPaikka, takana2) && osa_paikat.Contains(takana2)))
                        palautus.AddRange(osa_paikat);
                    if ((OnkoSyonninLoppupaikka(siirrettavanPaikka, takana3) && toinen_osa.Contains(takana3)) || (OnkoSyonninLoppupaikka(siirrettavanPaikka, takana4) && toinen_osa.Contains(takana4)))
                        palautus.AddRange(toinen_osa);
                }
                mahdPaikat = palautus;
            }
            return mahdPaikat;
        }


        /// <summary>
        /// Tarkistetaan tilanne, jossa nappulalla on mahdollisuus syödä
        /// </summary>
        /// <param name="kokeilu">Kokeilupaikka</param>
        /// <param name="kokeilu2">Toinen kokeilupaikka</param>
        /// <param name="takana">Mahdollinen paikka, jos syödään kokeilupaikassa oleva</param>
        /// <param name="takana2">Mahdollinen paikka jos syödään kokeilu2-paikassa oleva</param>
        /// <param name="toisen_paikat">Vastustajan paikat</param>
        /// <returns></returns>
        private List<Point> TarkistaLoput(Point kokeilu, Point kokeilu2, Point takana, Point takana2, Point[] toisen_paikat)
        {
            List<Point> lista = new List<Point>();
            if (sallitutPaikat.Contains(takana))
            {
                if (toisen_paikat.Contains(kokeilu) && vapaat_paikat.Contains(takana))
                {
                    lista.Add(takana);
                    syotavatPaikat.Add(kokeilu);
                }
            }
            if (sallitutPaikat.Contains(takana2))
            {
                if (toisen_paikat.Contains(kokeilu2) && vapaat_paikat.Contains(takana2))
                {
                    lista.Add(takana2);
                    syotavatPaikat.Add(kokeilu2);
                }
            }
            if (lista.Count > 0) return lista;

            if (sallitutPaikat.Contains(kokeilu) && vapaat_paikat.Contains(kokeilu)) lista.Add(kokeilu);
            if (sallitutPaikat.Contains(kokeilu2) && vapaat_paikat.Contains(kokeilu2)) lista.Add(kokeilu2);
            return lista;
        }
    }
}
