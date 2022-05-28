using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Galgje_BenSleurs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string woord;
        int levens = 10;
        StringBuilder juisteLetters = new StringBuilder();
        StringBuilder fouteLetters = new StringBuilder();
        StringBuilder fouteWoorden = new StringBuilder();
        
        
        StringBuilder discokleur = new StringBuilder();
        

        Border[,] borders;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += WilJeDisco;
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
        private void HideInfo()
        {
            L_Info.Visibility = Visibility.Hidden;
            L_Levens.Visibility = Visibility.Visible;
            L_Juist.Visibility = Visibility.Visible;
            L_Juist_Output.Visibility = Visibility.Visible;
            L_Fout.Visibility = Visibility.Visible;
            L_Fout_Output.Visibility = Visibility.Visible;
            B_Raad.Visibility = Visibility.Visible;
            B_Verberg.Visibility = Visibility.Hidden;
        }
        //Doet omgekeerde van HideInfo
        private void ShowInfo()
        {
            L_Info.Visibility = Visibility.Visible;
            L_Levens.Visibility = Visibility.Hidden;
            L_Juist.Visibility = Visibility.Hidden;
            L_Juist_Output.Visibility = Visibility.Hidden;
            L_Fout.Visibility = Visibility.Hidden;
            L_Fout_Output.Visibility = Visibility.Hidden;
        }
        private void UpdateLevens(int levens)
        {
            if (levens==1)
            {
                L_Levens.Content = $"Nog 1 leven over";
                
            }
            else
            {
                L_Levens.Content = $"Nog {levens} levens over";
               
            }
            
        }
        //De speler heeft nul levens het spel moet eindigen
        private void GameOver()
        {
            ShowInfo();
            L_Info.Content = $"Jammer! Je hebt het woord niet kunnen raden. \nHet juiste woord was \"{woord}\"";
            B_Raad.IsEnabled = false;
        }
        // proces dat bij fouteletter wordt uitgevoerd
        private void FouteLetter()
        {
            levens--;
            UpdateLevens(levens);
            fouteLetters.Append(TB_Input.Text.ToLower());
            L_Fout_Output.Content = fouteLetters.ToString();
           
        }
        // proces dat bij juiste letter wordt uitgevoerd
        private void JuisteLetter()
        {
            juisteLetters.Append(TB_Input.Text.ToLower());
            L_Juist_Output.Content = juisteLetters.ToString();
            

        }
        private void FoutWoord()
        {
            fouteWoorden.AppendLine(TB_Input.Text.ToLower());
            L_Foute_Woorden.Content = fouteWoorden.ToString();
        }
        // proces dat gebeurd als de spelen gewonnen heeft
        private void Gewonnen()
        {
            ShowInfo();
            L_Info.Content = "HOERA!! Je hebt het woord juist geraden!!";
            B_Raad.IsEnabled = false;
            
        }
        private void Disco()
        {
            Random random = new Random();
            for (int i = 0; i < borders.GetLength(0); i++)
            {
                for (int j = 0; j < borders.GetLength(1); j++)
                {




                    var kleur = new Color();
                    kleur.R = Convert.ToByte(random.Next(0, 256));
                    kleur.G = Convert.ToByte(random.Next(0, 256));
                    kleur.B = Convert.ToByte(random.Next(0, 256));
                    kleur.A = 255;
                    borders[i,j].Background = new SolidColorBrush(kleur);
                }
            }
        }
        private Border[,] FillBorders()
        {
            var cols = Grid_Main.ColumnDefinitions.Count;
            var rows = Grid_Main.RowDefinitions.Count;

            Border[,] borders = new Border[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var border = new Border();

                    Grid_Main.Children.Add(border);

                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    Grid.SetZIndex(border, -1);

                    borders[i, j] = border;
                }
            }

            return borders;
        }
        
        private void WilJeDisco(object sender, RoutedEventArgs e)
        {
            MessageBoxResult discoBox = MessageBox.Show("Wil je in een disco spelen?", "Wissen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (discoBox == MessageBoxResult.Yes)
            {
                StartDisco();
                StartMuziek();
            }
        }
        private void StartDisco()
        {
            DispatcherTimer dispatcher;
            borders = FillBorders();
            dispatcher = new DispatcherTimer();
            dispatcher.Interval = TimeSpan.FromMilliseconds(300);
            dispatcher.Tick += delegate { Disco(); };
            dispatcher.Start();
            
        }
        private void StartMuziek()
        {
            
            //MediaPlayer Flea = new MediaPlayer();
            //Uri fleaUri = new Uri("../../../Assets/SpanishFlea.mp3",UriKind.Relative);
            //Flea.Open(fleaUri);
            //Flea.Play();

            SoundPlayer player = new SoundPlayer("../../../Assets/SpanishFlea.wav");
            player.Load();
            player.PlayLooping();



        }
        private void Raad()
        {
            if (TB_Input.Text.Length > 1)
            {
                if (TB_Input.Text.ToLower() == woord)
                {
                    Gewonnen();

                }
                else
                {
                    FoutWoord();

                    levens--;
                    UpdateLevens(levens);

                }
            }

            else if (woord.Contains(TB_Input.Text.ToLower()))
            {
                JuisteLetter();
            }
            else
            {
                FouteLetter();
            }
            //TB_Input.Text = "";
            if (levens == 0)
            {
                GameOver();
            }
            TB_Input.Text = "";
        }

        private void Verberg()
        {
            if (TB_Input.Text.Length > 1)
            {
                woord = TB_Input.Text.ToLower();
                TB_Input.Text = "";
                B_Verberg.IsEnabled = false;
                B_Raad.IsEnabled = true;
                HideInfo();
                L_Levens.Content = "10 Levens Over";
                L_Juist.Content = "Juiste Letters:";
                L_Fout.Content = "Foute Letters:";
            }
            else
            {
                TB_Input.Text = "";
                L_Info.Content = "Je woord moet minsten 2 karakters lang zijn";
            }
        }
        private void TB_Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key==Key.Enter&&B_Raad.IsEnabled)
            {
                Raad();
            }
            else if (e.Key==Key.Enter&&B_Raad.IsEnabled==false)
            {
                Verberg();
            }
        }
        private void NieuwSpel()
        {
            L_Info.Content = "Speler 2: geef een geheim woord in";
            L_Juist.Content = "";
            L_Juist_Output.Content = "";
            L_Fout.Content = "";
            L_Fout_Output.Content = "";
            L_Levens.Content = "";
            L_Foute_Woorden.Content = "";
            TB_Input.IsEnabled = true;
            B_Raad.IsEnabled = false;
            B_Verberg.IsEnabled = true;
            B_Raad.Visibility = Visibility.Hidden;
            B_Verberg.Visibility = Visibility.Visible;
        }
    }
}
