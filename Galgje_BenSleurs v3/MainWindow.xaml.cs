using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
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
using Galgje_BenSleurs.Models;
using Microsoft.VisualBasic;

namespace Galgje_BenSleurs
{
    /// ------------------------------------
    /// Author:         Ben Sleurs
    /// Last Change:    23/12/2021
    /// Description:    Logic interaction with MainWindow.xaml
    /// --------------------------------------
    public partial class MainWindow : Window
    {
        #region Declaration variables
        private string word; //Te raden woord
        private string name; //naam speler
        private int lives = 10;
        private int timerTime = 10; //Maximum tijd voor timer
        private int timer = 10; //Huidige tijd voor timer
        private bool isDisco = false;
        private bool isHighScoreEligable = true;
        private List<Highscore> highscoreList = new List<Highscore>();
        private List<Char> hintLettersAvailable = new List<Char>(); //foute letters die nog nie geraden of als hint gegeven zijn

        private StringBuilder correctLetters = new StringBuilder(); 
        private StringBuilder wrongLetters = new StringBuilder();
        private StringBuilder wrongWords = new StringBuilder();
        private StringBuilder highScores = new StringBuilder(); //stringbuilder met alle highscores in de juiste volgorde

        private DispatcherTimer dispatcher = new DispatcherTimer(); //dispatcher voor de countdowntimer
        private DispatcherTimer discopatcher = new DispatcherTimer(); //dispatcher voor disco ticks

        private MediaPlayer Flea = new MediaPlayer(); //discomuziek
        private Uri fleaUri = new Uri(@"Assets/SpanishFlea.mp3", UriKind.RelativeOrAbsolute);
        private MediaPlayer buzzer = new MediaPlayer();
        private Uri buzzerUri = new Uri(@"Assets/Buzzer.mp3", UriKind.Relative);

        private Random random = new Random();
        private Border[,] borders; //2d array om borders aan elk grid element toe te voegen
        private string[] galgjeWoorden = System.IO.File.ReadAllLines("../../../Assets/SinglePlayerWords.txt");
        
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            Grid.SetZIndex(Grid_Background, -1);
            dispatcher.Interval = TimeSpan.FromSeconds(1);
            dispatcher.Tick += UpdateTime;
            Loaded += DoYouWantDisco;
        }
        #region Button Methods
        /// <summary>
        /// Roept NewGame() op om een nieuw spel te starten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void B_Nieuw_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }
        /// <summary>
        /// Roept NewGameSinglePlayer() op om singleplayer te starten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void B_SinglePlayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NewGameSinglePlayer();
        }
        /// <summary>
        /// Roept NewGameMultiPlayer() op om multiplayer te starten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void B_MultiPlayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NewGameMultiPlayer();
        }
        /// <summary>
        /// Roept HideWord() op om het ingegeven woord op te slaan en te verbergen
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void B_Verberg_Click(object sender, RoutedEventArgs e)
        {
            HideWord();
        }
        /// <summary>
        /// Roept Guess() op om de input te checken met het woord
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void B_Raad_Click(object sender, RoutedEventArgs e)
        {
            Guess();
        }
        /// <summary>
        /// Enter event voor raad en verberg
        /// Roept Guess() of HideWord() op via de enter toets, afhankelijk van welke van de 2 aanstaat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Knop die ingeduwt werd door de speler</param>
        private void TB_Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && B_Raad.IsEnabled)
            {
                Guess();
            }
            else if (e.Key == Key.Enter && B_Verberg.IsEnabled)
            {
                HideWord();
            }
        }
        /// <summary>
        /// Geeft een messagebox weer met daarin de highscores
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MU_Highscores_Click(object sender, RoutedEventArgs e)
        {
            _ = MessageBox.Show(highScores.ToString(), "Highscores", MessageBoxButton.OK, MessageBoxImage.Information); ;
        }
        /// <summary>
        /// Genereert een char dat nog niet als hint gegeven of zelf geraden is via GenerateCharNotInWord()
        /// Toont deze char via een messagebox, als er geen chars meer over zijn laat de messagebox weten dat de speler alle woorden kent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MU_Hint_Click(object sender, RoutedEventArgs e)
        {
            isHighScoreEligable = false;
            char a = GenerateCharNotInWord();
            _ = a != '!'
                ? MessageBox.Show($"De letter \"{a}\" zit niet in het woord","Hint", MessageBoxButton.OK,MessageBoxImage.Information)
                : MessageBox.Show($"Je kent alle foute letters!", "Hint",MessageBoxButton.OK,MessageBoxImage.Information);
        }
        /// <summary>
        /// Roept UpdateTimerTime() op om de maximum tijd te updaten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MU_timer_Click(object sender, RoutedEventArgs e)
        {
            UpdateTimerTime();
        }
        /// <summary>
        /// Regex om te zorgen dat in de inputbox enkel letters ingegeven kunnen worden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TB_Input_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        /// <summary>
        /// Hover effect voor Buttons
        /// Als muis op een van de buttons komt krijg deze een dikkere blauwe rand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseEnter_Label(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.BorderBrush = Brushes.Blue;
            lbl.BorderThickness = new Thickness(4);
        }
        /// <summary>
        /// Hover effect voor Buttons
        /// Als muis een van de buttons verlaat krijgen deze terug de standaard zwarte rand
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseLeave_Label(object sender, MouseEventArgs e)
        {
            Label lbl = (Label)sender;
            lbl.BorderBrush = Brushes.Black;
            lbl.BorderThickness = new Thickness(2);
        }
        #endregion
        #region Game start methodes
        /// <summary>
        /// Start een nieuw spel: Oude teksten worden gecleared en de UI laat de Singleplayer en Multiplayer buttons zien
        /// Info tekst vraag voor een naam
        /// Levens worden gereset op 10 en dispatcher wordt gestopt
        /// </summary>
        private void NewGame()
        {
            ClearText();
            TBL_Info.Text = "Geef je naam in";
            B_MultiPlayer.Visibility = Visibility.Visible;
            B_SinglePlayer.Visibility = Visibility.Visible;
            B_Nieuw.Visibility = Visibility.Hidden;
            TBL_Info.SetValue(Grid.ColumnSpanProperty, 3);
            TBL_Info.Visibility = Visibility.Visible;
            L_Timer.Visibility = Visibility.Hidden;
            TBL_GameOver.Visibility = Visibility.Hidden;
            B_Raad.IsEnabled = false;
            B_Verberg.IsEnabled = false;
            IMG_Galg.Source = null;
            TB_Input.IsEnabled = true;
            lives = 10;
            dispatcher.Stop();
        }
        /// <summary>
        /// Een SinglePlayer game wordt gestart
        /// De naam wordt opgeslagen via SaveName()
        /// UI wordt geupdate en een random woord uit de lijst wordt gekozen
        /// MakeGame() wordt opgeroepen op het spel effectief te laten starten
        /// </summary>
        private void NewGameSinglePlayer()
        {
            SaveName();
            B_Raad.IsEnabled = true;
            B_Raad.Visibility = Visibility.Visible;
            B_SinglePlayer.Visibility = Visibility.Hidden;
            B_MultiPlayer.Visibility = Visibility.Hidden;
            B_Nieuw.Visibility = Visibility.Visible;
            word = galgjeWoorden[random.Next(0, galgjeWoorden.Length)].ToLower().Trim();
            MakeGame(word);
        }
        /// <summary>
        /// Een Multiplayer game wordt gestart
        /// De naam wordt opgeslagen via SaveName()
        /// UI wordt geupdate en de 2de speler wordt gevraagd een woord in te geven
        /// </summary>
        private void NewGameMultiPlayer()
        {
            SaveName();
            TBL_Info.Text = "Speler 2: geef een geheim woord in";
            B_Raad.IsEnabled = false;
            B_Verberg.IsEnabled = true;
            B_Raad.Visibility = Visibility.Hidden;
            B_Verberg.Visibility = Visibility.Visible;
            B_Nieuw.Visibility = Visibility.Visible;
            B_Verberg.Visibility = Visibility.Visible;
            B_SinglePlayer.Visibility = Visibility.Hidden;
            B_MultiPlayer.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// De naam in de inputbox wordt opgeslagen in de string name
        /// als de inputbox leeg is krijgt name de waarde "Anonieme gebruiker"
        /// </summary>
        private void SaveName()
        {
            if (TB_Input.Text.Length != 0)
            {
                name = TB_Input.Text;
            }
            else
            {
                name = "Anonieme gebruiker";
            }
            TB_Input.Text = "";
        }
        /// <summary>
        /// Het spel start effectief
        /// Een lijst met alle letters van het alfabet wordt aangemaakt om later letters uit te halen, deze lijst wordt gebruikt om een hint letter te geven
        /// UI wordt geupdate en het woord komt tevoorschijn in wachtwoordvorm
        /// Timer wordt gereset en gestart
        /// </summary>
        /// <param name="woord">Het woord dat geraden moet worden</param>
        private void MakeGame(string woord)
        {
            hintLettersAvailable = Alphabet();
            isHighScoreEligable = true;
            ResetTimer();
            FillCorrectWord(woord);
            ShowInGame();
            dispatcher.Start();
        }
        /// <summary>
        /// Laat het verborgen woord in wachtwoord vorm (●●●) zien en toont dieze in juiste letters
        /// </summary>
        /// <param name="woord">Het verborgen woord dat geraden moet worden</param>
        private void FillCorrectWord(string woord)
        {
            correctLetters.Clear();
            for (int i = 0; i < woord.Length; i++)
            {
                if (woord[i] != ' ')
                {
                    correctLetters.Append("●");
                    hintLettersAvailable.Remove(woord[i]);
                }
                else
                {
                    correctLetters.Append(" ");
                }
            }
            TBL_Juist_Output.Text = correctLetters.ToString();
        }
        /// <summary>
        /// Probeert het ingegeven woord te verbergen en op te slaan.
        /// Als de input legitiem is wordt deze opgeslagen in lowercase in 'word' en start het spelt aan de hand van dit woord
        /// Als de input niet legitiem is wordt aan de speler gevraagd om een nieuw woord in te geven
        /// </summary>
        private void HideWord()
        {
            if (TB_Input.Text.Length > 1)
            {
                word = TB_Input.Text.ToLower().Trim();
                MakeGame(word);
            }
            else
            {
                TB_Input.Text = "";
                TBL_Info.Text = "Je woord moet minsten 2 karakters lang zijn";
            }
        }
        #endregion
        #region UI based methods
        /// <summary>
        /// Methode die de UI update. Toont UI zoals hij tijdens het spel moet uitzien
        /// Bepaalde knoppen worden zichtbaar of onzichtbaar
        /// Tekst wordt geupdate
        /// Bepaalde menuitems worden enabled of disabled
        /// </summary>
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
            B_Verberg.IsEnabled = false;
            B_Raad.IsEnabled = true;
            MU_Features.IsEnabled = false;
            MU_Hint.IsEnabled = true;
            L_Levens.Content = "10 Levens Over";
            L_Juist.Content = "Juiste Letters:";
            L_Fout.Content = "Foute Letters:";
            TB_Input.Text = "";
        }
        /// <summary>
        /// Methode die de UI update. Toont UI zoals hij er buiten het spel moet uitzien
        /// Bepaalde knoppen worden zichtbaar of onzichtbaar
        /// Tekst wordt geupdate
        /// Bepaalde menuitems worden enabled of disabled
        /// </summary>
        private void ShowMenu()
        {
            L_Levens.Visibility = Visibility.Hidden;
            L_Juist.Visibility = Visibility.Hidden;
            TBL_Juist_Output.Visibility = Visibility.Hidden;
            L_Fout.Visibility = Visibility.Hidden;
            L_Fout_Output.Visibility = Visibility.Hidden;
            B_Raad.Visibility = Visibility.Hidden;
            L_Timer.Visibility = Visibility.Hidden;
            B_Raad.IsEnabled = false;
            B_Verberg.IsEnabled = false;
            TB_Input.IsEnabled = false;
        }
        /// <summary>
        /// Reset de countdowntimer, zet deze terug op de maximum waarde
        /// </summary>
        private void ResetTimer()
        {
            dispatcher.Stop();
            dispatcher.Start();
            timer = timerTime;
            L_Timer.Content = timer.ToString();
            L_Timer.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Animatie die getoont wordt bij een foute gok
        /// Scherm toont een rode flash
        /// </summary>
        private void LostLifeAnimation()
        {
            Color startColor = Colors.Transparent;
            Color endColor = Colors.Red;
            Grid_Background.Background = new SolidColorBrush(startColor);

            ColorAnimation animation = new ColorAnimation
            {
                From = startColor,
                To = endColor,
                Duration = TimeSpan.FromMilliseconds(750),
                AutoReverse = true,
            };

            Grid_Background.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }
        /// <summary>
        /// Galgje image wordt geupdate aan de hand van het aantal levens
        /// </summary>
        private void UpdateImage()
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri($"/assets/Galg{10 - lives}.png", UriKind.RelativeOrAbsolute);
            bi.EndInit();
            IMG_Galg.Source = bi;
        }
        /// <summary>
        /// Cleared alle tekst en stringbuilders
        /// stopt de dispatcher
        /// Enabled bepaalde menu items
        /// </summary>
        private void ClearText()
        {
            L_Juist.Content = "";
            TBL_Juist_Output.Text = "";
            L_Fout.Content = "";
            L_Fout_Output.Content = "";
            L_Levens.Content = "";
            L_Foute_Woorden.Content = "";
            TB_Input.Text = "";
            wrongWords.Clear();
            wrongLetters.Clear();
            L_Foute_Woorden.Content = wrongWords;
            dispatcher.Stop();
            MU_Hint.IsEnabled = false;
            MU_Features.IsEnabled = true;
        }
        /// <summary>
        /// Speelt buzzer af
        /// </summary>
        private void BuzzerMusic()
        {
            buzzer.Open(buzzerUri);
            buzzer.Play();
        }
        #endregion
        #region Game updates methods
        /// <summary>
        /// checked of de gok juist of fout is, cleared de inputtextbox
        /// </summary>
        private void Guess()
        {
            if (TB_Input.Text.Length > 1)
            {
                if (TB_Input.Text.ToLower() == word)
                {
                    Victory();
                }
                else if (wrongWords.ToString().Contains(TB_Input.Text) == false)
                {
                    WrongWord();
                }
            }
            else if (TB_Input.Text.Length == 1 && correctLetters.ToString().Contains(TB_Input.Text) == false)
            {
                if (word.Contains(TB_Input.Text.ToLower()))
                {
                    CorrectLetter();
                }
                else if (!wrongLetters.ToString().Contains(TB_Input.Text))
                {
                    WrongLetter();
                }
            }
            TB_Input.Text = "";
        }
        /// <summary>
        /// De foute letter wordt uit de hintletters gehaald, wrongletters krijgt de foute letter extra
        /// Timer wordt gereset en lives worden geupdate met 1 leven minder
        /// </summary>
        private void WrongLetter()
        {
            hintLettersAvailable.Remove(char.Parse(TB_Input.Text.ToLower()));
            wrongLetters.Append(TB_Input.Text.ToLower() + " ");
            L_Fout_Output.Content = wrongLetters.ToString();
            ResetTimer();
            UpdateLives();
        }
        /// <summary>
        /// timer wordt gereset en het verborgen woord wordt geupdate met de juiste letter
        /// </summary>
        private void CorrectLetter()
        {
            ResetTimer();
            UpdateCorrectLetters(char.Parse(TB_Input.Text.ToLower()));
            TBL_Juist_Output.Text = correctLetters.ToString();
        }
        /// <summary>
        /// Vervangt de posities in het verborgen woord met de juiste gegokte letter
        /// </summary>
        /// <param name="a"></param>
        private void UpdateCorrectLetters(char a)
        {
            for (int i = 0; i < correctLetters.Length; i++)
            {
                if (word[i] == a)
                {
                    correctLetters.Replace('●', a, i, 1);
                }
            }
        }
        /// <summary>
        /// De timer wordt gereset en levens worden geupdate
        /// Het fout woord komt bij de foute woorden extra
        /// </summary>
        private void WrongWord()
        {
            ResetTimer();
            wrongWords.AppendLine(TB_Input.Text.ToLower());
            L_Foute_Woorden.Content = wrongWords.ToString();
            UpdateLives();
        }
        /// <summary>
        /// Er is een foute gok gemaakt
        /// Levens gaan met 1 omlaag
        /// een buzzer en rode flash spelen zich af, de galg image wordt geupdate
        /// er wordt gechecked hoeveel levens en het spel wordt mogelijks beëindigd
        /// </summary>
        private void UpdateLives()
        {
            lives--;
            BuzzerMusic();
            LostLifeAnimation();
            UpdateImage();
            if (lives>1)
            {
                L_Levens.Content = $"Nog {lives} levens over"; 
            }
            else if(lives==1)
            {
                L_Levens.Content = $"Nog 1 leven over";
            }
            else
            {
                GameOver();
            }
        }
        /// <summary>
        /// De speler heeft verloren
        /// De UI wordt geupdate naar een startmenu en een gameover tekst verschijnt
        /// </summary>
        private void GameOver()
        {
            ShowMenu();
            ClearText();
            TBL_GameOver.Visibility = Visibility.Visible;
            TBL_GameOver.Text = $"Jammer! Je hebt het woord niet kunnen raden. Het juiste woord was \"{word}\""; 
        }
        /// <summary>
        /// De speler heeft gewonnen
        /// Mogelijks worden de highscores geupdate
        /// De UI wordt geupdate naar een startmenu met een overwinningstekst
        /// </summary>
        private void Victory()
        {
            if (isHighScoreEligable==true)
            {
                UpdateHighScores();
            }
            ShowMenu();
            ClearText();
            if (lives < 10)
            {
                TBL_Info.SetValue(Grid.ColumnSpanProperty, 2);
            }
            TBL_Info.Visibility = Visibility.Visible;
            TBL_Info.Text = "HOERA!! Je hebt het gewonnen!!";
        }
        /// <summary>
        /// Update de highscores, er wordt een highscore object aangemaakt met de naam, gebruikelevens en het moment van de overwinning
        /// highscorelist en highscore stringbuilder worden geupdate en gesort van minste levens gebruikt naar meeste
        /// </summary>
        private void UpdateHighScores()
        {
            Highscore highscore = new Highscore(name, 10-lives, DateTime.Now);
            FillAndSortHighScoreList(highscore);
            highScores.Clear();
            for (int i = 0; i < highscoreList.Count; i++)
            {
                highScores.AppendLine($"{i+1}: {highscoreList[i].ToString()}");
            } 
        }
        /// <summary>
        /// insert de highscore op basis van levens gebruikt op de juiste plaats
        /// </summary>
        /// <param name="highscore">Het highscore object dat geïnsert wordt in de lijst</param>
        private void FillAndSortHighScoreList(Highscore highscore)
        {
            if (highscoreList.Count == 0)
            {
                highscoreList.Add(highscore);
            }
            else
            {
                int length = highscoreList.Count;
                for (int i = 0; i < length; i++)
                {
                    if (highscore.levens <= highscoreList[i].levens)
                    {
                        highscoreList.Insert(i, highscore);
                        break;
                    }
                    else
                    {
                        if (i == highscoreList.Count - 1)
                        {
                            highscoreList.Add(highscore);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Telt 1 seconde af elke seconde
        /// Bij 0 seconden wordt OutOfTime() opgeroepen en verliest de speler 1 leven
        /// bij -1 seconde wordt de timer terug gereset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTime(object sender, EventArgs e)
        {
            if (timer > 0)
            {
                timer--;
                L_Timer.Content = timer.ToString();
            }
            else if (timer == 0)
            {
                timer--;
                OutOfTime();
            }
            else
            {
                ResetTimer();
                B_Raad.IsEnabled = true;
            }
        }
        /// <summary>
        /// De speler heeft geen gok gewaagd binnen de tijd
        /// De speler verlies 1 leven en kan 1 seconde geen gok wagen om te voorkomen dat hij een extra leven verliest op de maximumtijd
        /// </summary>
        private void OutOfTime()
        {
            L_Timer.Content = "Te laat!";
            B_Raad.IsEnabled = false;
            UpdateLives();
        }
        #endregion
        #region Disco methods
        /// <summary>
        /// Messagebox bij start spel die vraagt of je in een disco wilt spelen
        /// bij 'ja' wordt de disco gestart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoYouWantDisco(object sender, RoutedEventArgs e)
        {
            MessageBoxResult discoBox = MessageBox.Show("Wil je in een disco spelen?", "Disco?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (discoBox == MessageBoxResult.Yes)
            {
                StartDisco();
                StartMusic();
            }
        }
        /// <summary>
        /// De disco wordt gestart
        /// elke grid element gaat random kleuren krijgen en discomuziek start
        /// </summary>
        private void StartDisco()
        {
            isDisco = true;
            borders = FillBorders();
            discopatcher.Interval = TimeSpan.FromMilliseconds(370);
            discopatcher.Tick += Disco;
            discopatcher.Start();
        }
        /// <summary>
        /// Elke grid element krijgt een border, deze border zullen de random kleuren krijgen
        /// </summary>
        /// <returns></returns>
        private Border[,] FillBorders()
        {
            int cols = Grid_Background.ColumnDefinitions.Count;
            int rows = Grid_Background.RowDefinitions.Count;

            Border[,] borders = new Border[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Border border = new Border();

                    Grid_Main.Children.Add(border);

                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    Grid.SetZIndex(border, -2);

                    borders[i, j] = border;
                }
            }
            return borders;
        }
        /// <summary>
        /// Kleurt op elk interval elke border en button een randomkleur in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Disco(object sender, EventArgs e)
        {
            Random random = new Random();
            Color color = new Color();
            for (int i = 0; i < borders.GetLength(0); i++)
            {
                for (int j = 0; j < borders.GetLength(1); j++)
                {
                    color.R = Convert.ToByte(random.Next(0, 256));
                    color.G = Convert.ToByte(random.Next(0, 256));
                    color.B = Convert.ToByte(random.Next(0, 256));
                    color.A = 255;
                    borders[i, j].Background = new SolidColorBrush(color);
                }
            }
            DiscoButton(B_Nieuw);
            DiscoButton(B_Raad);
            DiscoButton(B_Verberg);
            DiscoButton(B_MultiPlayer);
            DiscoButton(B_SinglePlayer);
            DiscoMenu(Menu_Main);
            DiscoMenu(MU_Hint);
            DiscoMenu(MU_Spel);
            DiscoMenu(MU_Features);
            DiscoMenu(MU_Highscores);
            DiscoMenu(MU_timer);
            DiscoMenu(MU_Start);
            DiscoMenu(MU_Sluit);

            
            
        }

        /// <summary>
        /// Geeft de meegegeven button een randomkleur
        /// </summary>
        /// <param name="a">Het label (button) die de random kleur krijgt</param>
        private void DiscoButton(Label a)
        {
            Random random = new Random();
            Color color = new Color();
            color.R = Convert.ToByte(random.Next(0, 256));
            color.G = Convert.ToByte(random.Next(0, 256));
            color.B = Convert.ToByte(random.Next(0, 256));
            color.A = 255;
            a.Background = new SolidColorBrush(color);
        }
        /// <summary>
        /// Kleurt het meegegeven menu in een randomkleur
        /// </summary>
        /// <param name="a">Het menu dat de random kleur krijgt</param>
        private void DiscoMenu(Menu a)
        {
            Random random = new Random();
            Color color = new Color();
            color.R = Convert.ToByte(random.Next(0, 256));
            color.G = Convert.ToByte(random.Next(0, 256));
            color.B = Convert.ToByte(random.Next(0, 256));
            color.A = 255;
            a.Background = new SolidColorBrush(color);
        }
        /// <summary>
        /// Kleurt het meegegeven menuitem in een randomkleur
        /// </summary>
        /// <param name="a">Het menuitem dat de random kleur krijgt</param>
        private void DiscoMenu(MenuItem a)
        {
            Random random = new Random();
            Color color = new Color();
            color.R = Convert.ToByte(random.Next(0, 256));
            color.G = Convert.ToByte(random.Next(0, 256));
            color.B = Convert.ToByte(random.Next(0, 256));
            color.A = 255;
            a.Background = new SolidColorBrush(color);
        }
        /// <summary>
        /// Start de discomuziek
        /// </summary>
        private void StartMusic()  
        {
            Flea.Open(fleaUri);
            Flea.Play();
            Flea.MediaEnded += new EventHandler(RestartFlea);
        }
        /// <summary>
        /// Herstart discomuziek
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestartFlea(object sender, EventArgs e)
        {
            Flea.Open(fleaUri);
            Flea.Play();
        }
        /// <summary>
        /// Herstart discomuziek na een mogelijk pauze
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContinueFlea(object sender, EventArgs e)
        {
            Flea.Play();
        }
        #endregion
        #region Change variables methods
        /// <summary>
        /// Geeft een inputbox weer voor de gebruiken om de maximumtijd te kunnen veranderen
        /// bij een foute input (niet tussen 5 en 20) komt de inputbox terug te voorschijn
        /// </summary>
        private void UpdateTimerTime()
        {
            int intTimerTime;
            string stringTimerTime = Microsoft.VisualBasic.Interaction.InputBox("Geef in hoeveel seconden elke ronde mag duren (tussen 5 en 20 seconden)", "Timer instellen", "10");
            bool isGetal = int.TryParse(stringTimerTime, out intTimerTime);
            if (isGetal && intTimerTime >= 5 && intTimerTime <= 20)
            {
                timerTime = intTimerTime;
            }
            else if (stringTimerTime != "")
            {
                UpdateTimerTime();
            }
        }
        #endregion
        #region Miscellaneous
        /// <summary>
        /// Sluit het spel af
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpelAfsluiten(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// Genereert een letter die niet in het woord zit en waarvan de speler nog niet weet dat hij niet in het woord zit
        /// Als er geen letter meer over is returned hij '!'
        /// </summary>
        /// <returns>letter of '!'</returns>
        private char GenerateCharNotInWord()
        {
            if (hintLettersAvailable.Count != 0)
            {
                Random random = new Random();
                int index = random.Next(0, hintLettersAvailable.Count);
                char a = hintLettersAvailable[index];
                hintLettersAvailable.RemoveAt(index);
                return a;
            }
            else return '!';
        }
        /// <summary>
        /// Genereert een list met alle letters van het alphabet
        /// </summary>
        /// <returns>Alphabet in list</returns>
        private List<Char> Alphabet()
        {
            char[] temp = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
            List<Char> alphabet = new List<Char>(temp);
            return alphabet;
        }
        #endregion
    }
}
