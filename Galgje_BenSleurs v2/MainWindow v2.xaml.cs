using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

//Password box en win fixen

namespace Galgje_BenSleurs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string woord;
        int levens = 10;
        int timer = 10;
        bool isDisco = false;
        StringBuilder juisteLetters = new StringBuilder(); 
        StringBuilder fouteLetters = new StringBuilder();
        StringBuilder fouteWoorden = new StringBuilder();

        DispatcherTimer dispatcher = new DispatcherTimer();
        DispatcherTimer discopatcher = new DispatcherTimer();


        MediaPlayer Flea = new MediaPlayer();
        Uri fleaUri = new Uri(@"Assets/SpanishFlea.mp3", UriKind.RelativeOrAbsolute);
        MediaPlayer buzzer = new MediaPlayer();
        Uri buzzerUri = new Uri(@"Assets/Buzzer.mp3", UriKind.Relative);


        Border[,] borders;
        public MainWindow()
        {
            
            InitializeComponent();
            Grid.SetZIndex(Grid_Background, -1);
            Loaded += WilJeDisco;
            dispatcher.Interval = TimeSpan.FromSeconds(1);
            dispatcher.Tick += UpdateTime;
            dispatcher.Start();
            NieuwSpel();


        }

       

        private void B_Nieuw_Click(object sender, RoutedEventArgs e)
        {
            NieuwSpel();
            
            
        }

        private void B_Verberg_Click(object sender, RoutedEventArgs e)
        {
            Verberg();
        }

        private void B_Raad_Click(object sender, RoutedEventArgs e)
        {
            Raad();
        }
        //Verbergd main info label en laat levens en letter labels zien
        private void ShowInGame()
        {
            TBL_Info.Visibility = Visibility.Hidden;
            L_Levens.Visibility = Visibility.Visible;
            L_Juist.Visibility = Visibility.Visible;
            TBL_Juist_Output.Visibility = Visibility.Visible;
            L_Fout.Visibility = Visibility.Visible;
            L_Fout_Output.Visibility = Visibility.Visible;
            B_Raad.Visibility = Visibility.Visible;
            B_Verberg.Visibility = Visibility.Hidden;
            L_Levens.Content = "10 Levens Over";
            L_Juist.Content = "Juiste Letters:";
            L_Fout.Content = "Foute Letters:";
        }
        //Doet omgekeerde van ShowInGame
        private void ShowMenu()
        {
            
            
            L_Levens.Visibility = Visibility.Hidden;
            L_Juist.Visibility = Visibility.Hidden;
            TBL_Juist_Output.Visibility = Visibility.Hidden;
            L_Fout.Visibility = Visibility.Hidden;
            L_Fout_Output.Visibility = Visibility.Hidden;
            B_Raad.Visibility = Visibility.Hidden;
            B_Raad.IsEnabled = false;
            L_Timer.Visibility = Visibility.Hidden;
            B_Verberg.IsEnabled = false;
            TB_Input.IsEnabled = false;
            fouteWoorden.Clear();
            L_Foute_Woorden.Content = fouteWoorden;
            dispatcher.Stop();
            
        }
        //Update levens na er fout gegokt is, kan ook gameover uitvoeren bij nul levens
        private void UpdateLevens()
        {
            levens--;
            BuzzerMusic();
            LostLifeAnimation();
            UpdateImage();
            if (levens>1)
            {
                L_Levens.Content = $"Nog {levens} levens over"; 
            }
            else if(levens==1)
            {
                L_Levens.Content = $"Nog 1 leven over";
            }
            else
            {
                GameOver();
            }
        }
        //De speler heeft nul levens het spel moet eindigen
        private void GameOver()
        {
            ShowMenu();
            TBL_GameOver.Visibility = Visibility.Visible;
            TBL_GameOver.Text = $"Jammer! Je hebt het woord niet kunnen raden. Het juiste woord was \"{woord}\""; 
        }
        // proces dat gebeurd als de spelen gewonnen heeft
        private void Gewonnen()
        {
            ShowMenu();
            if (levens < 10)
            {
                TBL_Info.SetValue(Grid.ColumnSpanProperty, 2);
            }
            TBL_Info.Visibility = Visibility.Visible;
            TBL_Info.Text = "HOERA!! Je hebt het gewonnen!!";
        }
        // proces dat bij fouteletter wordt uitgevoerd
        private void FouteLetter()
        {
            ResetTimer();
            UpdateLevens();
            fouteLetters.Append(TB_Input.Text.ToLower()+" ");
            L_Fout_Output.Content = fouteLetters.ToString();
            
        }
        // proces dat bij juiste letter wordt uitgevoerd
        private void JuisteLetter()
        {
            ResetTimer();
            UpdateJuisteLetters(char.Parse(TB_Input.Text.ToLower()));
            TBL_Juist_Output.Text = juisteLetters.ToString();
            
        }
        // proces dat bij een fout woord wordt uitgevoerd
        private void FoutWoord()
        {
            ResetTimer();
            UpdateLevens();
            fouteWoorden.AppendLine(TB_Input.Text.ToLower());
            L_Foute_Woorden.Content = fouteWoorden.ToString();
            
        }

        //Kleurt alle grid elementen en buttons met een random kleur in
        private void Disco(object sender, EventArgs e)
        {
            Random random = new Random();
            var kleur = new Color();
            for (int i = 0; i < borders.GetLength(0); i++)
            {
                for (int j = 0; j < borders.GetLength(1); j++)
                {
                    kleur.R = Convert.ToByte(random.Next(0, 256));
                    kleur.G = Convert.ToByte(random.Next(0, 256));
                    kleur.B = Convert.ToByte(random.Next(0, 256));
                    kleur.A = 255;
                    borders[i,j].Background = new SolidColorBrush(kleur);
                }
            }
            DiscoButton(B_Nieuw);
            DiscoButton(B_Raad);
            DiscoButton(B_Verberg);   
        }
        //Maakt borders aan op elke grid element van Grid_Background
        private Border[,] FillBorders()
        {
            var cols = Grid_Background.ColumnDefinitions.Count;
            var rows = Grid_Background.RowDefinitions.Count;

            Border[,] borders = new Border[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var border = new Border();

                    Grid_Main.Children.Add(border);

                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    Grid.SetZIndex(border, -2);

                    borders[i, j] = border;
                }
            }
            return borders;
        }
        
        //Pop-up box om te kiezen of je in een kermis wilt spelen
        private void WilJeDisco(object sender, RoutedEventArgs e)
        {
            MessageBoxResult discoBox = MessageBox.Show("Wil je in een disco spelen?", "Disco?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (discoBox == MessageBoxResult.Yes)
            {
                StartDisco();
                StartMuziek();
            }
        }
        //Start de kermis, inclusief muziek en random kleuren
        private void StartDisco()
        {
            isDisco = true;
            borders = FillBorders();
            discopatcher.Interval = TimeSpan.FromMilliseconds(370);
            discopatcher.Tick += Disco;
            discopatcher.Start();
        }
        private void DiscoButton(Label a)
        {
            Random random = new Random();
            Color kleur = new Color();
            kleur.R = Convert.ToByte(random.Next(0, 256));
            kleur.G = Convert.ToByte(random.Next(0, 256));
            kleur.B = Convert.ToByte(random.Next(0, 256));
            kleur.A = 255;
            a.Background = new SolidColorBrush(kleur);
        }
        //Start kermis muziek
        private void StartMuziek()  
        {
            Flea.Open(fleaUri);
            Flea.Play();
            Flea.MediaEnded += new EventHandler(RestartFlea);
        }
        //Zorgt ervoor dat de muziek blijft loopen
        private void RestartFlea(object sender, EventArgs e)
        {
            Flea.Open(fleaUri);
            Flea.Play();
        }
        private void ContinueFlea(object sender, EventArgs e)
        {
            Flea.Play();
        }
        //Proces dat afgaat als er een letter of woord wordt geraden
        private void Raad()
        {

            ResetBackground();
            if (TB_Input.Text.Length > 1)
            {
                if (TB_Input.Text.ToLower() == woord)
                {
                    Gewonnen();
                }
                else if (fouteWoorden.ToString().Contains(TB_Input.Text)==false)
                {
                    FoutWoord();      
                }
            }
            else if (TB_Input.Text.Length==1 && juisteLetters.ToString().Contains(TB_Input.Text)==false)
            {
                
                if (woord.Contains(TB_Input.Text.ToLower()))
                {
                   JuisteLetter();
                }
                else if(!fouteLetters.ToString().Contains(TB_Input.Text))
                {
                    FouteLetter();
                }
                
            }
            TB_Input.Text = "";
            

        }
        //Proces dat afgaat als het geheime woord wordt verborgen
        private void Verberg()
        {
            if (TB_Input.Text.Length > 1)
            {
                ResetBackground();
                ResetTimer();
                woord = TB_Input.Text.ToLower().Trim();
                FillJuisteWoord(woord);
                TB_Input.Text = "";
                B_Verberg.IsEnabled = false;
                B_Raad.IsEnabled = true;
                ShowInGame();
            }
            else
            {
                TB_Input.Text = "";
                TBL_Info.Text = "Je woord moet minsten 2 karakters lang zijn";
            }
        }
        //Gaat raad of verberg button oproepen op enter afhankelijk van de staat van het spel
        private void TB_Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key==Key.Enter&&B_Raad.IsEnabled)
            {
                Raad();
            }
            else if (e.Key==Key.Enter&&B_Verberg.IsEnabled)
            {
                Verberg();
            }     
        }
        //Start nieuw spel
        private void NieuwSpel()
        {
            ResetBackground();
            ClearText();
            TBL_Info.Text = "Speler 2: geef een geheim woord in";
            TBL_Info.SetValue(Grid.ColumnSpanProperty, 3);
            TB_Input.IsEnabled = true;
            B_Raad.IsEnabled = false;
            B_Verberg.IsEnabled = true;
            B_Raad.Visibility = Visibility.Hidden;
            B_Verberg.Visibility = Visibility.Visible;
            TBL_Info.Visibility = Visibility.Visible;
            L_Timer.Visibility = Visibility.Hidden;
            TBL_GameOver.Visibility = Visibility.Hidden;
            dispatcher.Stop();
            levens = 10;
            IMG_Galg.Source = null;
        }
        //Laat het woord in soort van wachtwoord vorm zien ●
        private void FillJuisteWoord(string woord)
        {
            juisteLetters.Clear();
            for (int i = 0; i < woord.Length; i++)
            {
                if (woord[i]==' ')
                {
                    juisteLetters.Append(" ");
                }
                else
                {
                    juisteLetters.Append("●");
                }
                
            }
            TBL_Juist_Output.Text = juisteLetters.ToString();
        }
        //Vormt de verborgen letters om in de juiste letter
        private void UpdateJuisteLetters(char a)
        {
            for (int i = 0; i < juisteLetters.Length; i++)
            {
                if (woord[i]==a)
                {
                    juisteLetters.Replace('●', a, i,1);
                }
            }
        }
        //Zet de timer terug op 10, verandert niets aan achtergrond of muziek
        private void ResetTimer()
        {
            B_Raad.IsEnabled = true;
            B_Verberg.IsEnabled = true;
            dispatcher.Stop();
            dispatcher.Start();
            timer = 10;
            L_Timer.Content = timer.ToString();
        }
        //Reset Background
        private void ResetBackground()
        {
            Grid_Background.Background = Brushes.Transparent;
            L_Timer.Visibility = Visibility.Visible;
            //if (isDisco)
            //{
            //    Flea.Play();
            //}  
        }
        //Update de timer en checked of de tijd verlopen is
        private void UpdateTime(object sender, EventArgs e)
        {
            if (timer>0)
            {
                timer--;
                L_Timer.Content = timer.ToString();
            }
            else if(timer==0)
            {
                timer--;
                OutOfTime();
            }
            else
            {
                ResetBackground();
                ResetTimer();
            }
        }
        //De tijd was om, de speler verliest 1 leven
        private void OutOfTime()
        { 
            L_Timer.Content = "Te laat!";
            B_Raad.IsEnabled = false;
            B_Verberg.IsEnabled = false;
            //if (isDisco)
            //{
            //    Flea.Pause();
            //}
            UpdateLevens();
        }
        //Kleurt button in random kleur


        private void MouseEnter_Label(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.BorderBrush = Brushes.Blue;
            lbl.BorderThickness = new Thickness(4);
        }
        private void MouseLeave_Label(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.BorderBrush = Brushes.Black;
            lbl.BorderThickness = new Thickness(2);
        }


        //Schaamteloos gestolen van Robin Oger
        private void LostLifeAnimation()
        {
            var color = Colors.Transparent;
            var errorColor = Colors.Red;
            Grid_Background.Background = new SolidColorBrush(color);

            var animation = new ColorAnimation
            {
                From = color,
                To = errorColor,
                Duration = TimeSpan.FromMilliseconds(750),
                AutoReverse = true,
            };

            Grid_Background.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);



        }
        private void BuzzerMusic()
        {
            if (isDisco)
            {
                Flea.Pause();
            }
            buzzer.Open(buzzerUri);
            buzzer.Play();
            if (isDisco)
            {
                buzzer.MediaEnded += ContinueFlea;
            }
        }
        private void UpdateImage()
        {
            
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri($"/assets/Galg{10-levens}.png",UriKind.RelativeOrAbsolute);
            bi.EndInit();
            IMG_Galg.Source = bi;
            

        }
        private void ClearText()
        {
            L_Juist.Content = "";
            TBL_Juist_Output.Text = "";
            L_Fout.Content = "";
            L_Fout_Output.Content = "";
            L_Levens.Content = "";
            L_Foute_Woorden.Content = "";
            TB_Input.Text = "";
        }
    }
}
