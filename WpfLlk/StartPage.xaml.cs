using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WpfLlk
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {

        Window parentWindow;
        public Window ParentWindow
        {
            get { return parentWindow; }
            set { parentWindow = value; }
        }

        public StartPage()
        {
            InitializeComponent();
        }

        private void CloseGame(object sender, RoutedEventArgs e)
        {
            ParentWindow.Close();
        }

        private void NormalGame(object sender, RoutedEventArgs e)
        {
            GamePage mygame = new GamePage();
            mygame.ParentWindow = ParentWindow;
            mygame.GameHeight = 10;
            mygame.GameWidth = 10;
            mygame.ADmode = false;

            mygame.reveal_partfour.Visibility = Visibility.Hidden;

            ParentWindow.Content = mygame;
        }
        private void PropsGame(object sender, RoutedEventArgs e)
        {
            GamePage mygame = new GamePage();
            mygame.ParentWindow = ParentWindow;
            mygame.ADmode = false;
            mygame.GameHeight = 10;
            mygame.GameWidth = 10;
            mygame.Propsnum = 5;

            mygame.reveal_partfour.Visibility = Visibility.Hidden;

            ParentWindow.Content = mygame;
        }
        private void ADGame(object sender, RoutedEventArgs e)
        {
            GamePage mygame = new GamePage();
            mygame.ParentWindow = ParentWindow;
            mygame.GameHeight = 20;
            mygame.GameWidth = 10;
            mygame.ADmode = true;
            mygame.ADmodeDiffculyt = 20;
            mygame.outterLayer = 5;
            mygame.GameDiffculty = 10;

            mygame.reveal_partone.Height = 0;
            mygame.reveal_partone.Visibility = Visibility.Hidden;
            mygame.reveal_parttwo.Height = 0;
            mygame.reveal_parttwo.Visibility = Visibility.Hidden;
           

            ParentWindow.Content = mygame;
        }
    }
}
