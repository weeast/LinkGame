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
using System.Windows.Threading;
using System.ComponentModel;


namespace WpfLlk
{
    /// <summary>
    /// Interaction logic for GamePage.xaml
    /// </summary>
    /// 
    public partial class GamePage : Page
    {

        Window parentWindow;
        public Window ParentWindow
        {
            get { return parentWindow; }
            set { parentWindow = value; }
        }


        //游戏的逻辑矩阵
        //对于逻辑矩阵中点的储存为point[Height,Width]
        //对应于UI中Grid的point[row,col]
        public static int[,] Pic_List;
        //逻辑矩阵的大小
        public int GameHeight;
        public int GameWidth;

        //游戏难度
        public int GameDiffculty = 20;

        //
        public int coutTime = 0;

        //道具个数
        int propsnum;
        public int Propsnum
        {
            get { return propsnum; }
            set { propsnum = value; }
        } 

        //闯关模式
        public bool ADmode = false;
        public int outterLayer;
        public int ADmodeDiffculyt;
        public int ADmodeLevel;

        //狂暴状态
        public static bool Props_Crazystate = false; 

        //炸弹状态
        public static bool Props_Bombstate = false;


        //储存上次点击的图片
        public static List<Button> tempButtons = new List<Button>();
        //存储可选路径
        public static List<int[]> mentionPics = new List<int[]>();
        
        //计时器
        public static int mytime;
        public static DispatcherTimer TimeClock = new System.Windows.Threading.DispatcherTimer();

        //游戏计时的绑定源
        //当绑定的属性改变时改变前台数据的类
        public abstract class NotifyProperty : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public void OnChangedProperties(string propertyName)
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
        //继承了NotifyProperty的绑定源
        public class gameData : NotifyProperty
        {
            private string gametime = string.Empty;
            private int gamepoints = 0;
            private int targetpoints = 0;
            private int gamepointsinc = 1;
            private int props_cold = 0;
            private int props_crazy = 0;
            private int props_double = 0;
            private int props_bomb = 0;
            private int props_time = 0;

            public string gameTime
            {
                get
                {
                    return this.gametime;
                }

                set
                {
                    this.gametime = value;
                    this.OnChangedProperties(@"gameTime");
                }
            }

            public int GamePointsInc
            {
                get
                {
                    return this.gamepointsinc;
                }

                set
                {
                    this.gamepointsinc = value;
                    this.OnChangedProperties(@"GamePointsInc");
                }
            }

            public int TargetPoints
            {
                get
                {
                    return this.targetpoints;
                }
                set
                {
                    this.targetpoints = value;
                    this.OnChangedProperties(@"TargetPoints");
                }
            }

            public int gamePoints
            {
                get
                {
                    return this.gamepoints;
                }

                set
                {
                    this.gamepoints = value;
                    this.OnChangedProperties(@"gamePoints");
                }
            }

            public int PropsCold
            {
                get
                {
                    return this.props_cold;
                }
                set
                {
                    this.props_cold = value;
                    this.OnChangedProperties(@"PropsCold");
                }
            }
            public int PropsCrazy
            {
                get
                {
                    return this.props_crazy;
                }
                set
                {
                    this.props_crazy = value;
                    this.OnChangedProperties(@"PropsCrazy");
                }
            }
            public int PropsDouble
            {
                get
                {
                    return this.props_double;
                }
                set
                {
                    this.props_double = value;
                    this.OnChangedProperties(@"PropsDouble");
                }
            }
            public int PropsBomb
            {
                get
                {
                    return this.props_bomb;
                }
                set
                {
                    this.props_bomb = value;
                    this.OnChangedProperties(@"PropsBomb");
                }
            }
            public int PropsTime
            {
                get
                {
                    return this.props_time;
                }
                set
                {
                    this.props_time = value;
                    this.OnChangedProperties(@"PropsTime");
                }
            }
        }
        //实例化绑定源
        static internal gameData mygameData = new gameData();

        //点的结构体
        public struct Pointnode
        {
            public int x;
            public int y;
            public Pointnode(int x1, int y1)
            {
                x = x1;
                y = y1;
            }
        };

        //存放node
        public static Queue<Pointnode> PointQueue = new Queue<Pointnode>();


        //主窗口函数
        public GamePage()
        {
            InitializeComponent();            
        
            //将绑定源添加到主窗口
            this.DataContext = mygameData;

            InitializeCutom();

            
            //初始化计时器
            TimeClock.Tick += new EventHandler(delegate(object s, EventArgs a)
                {
                    if (mytime < 1)
                    {
                        mytime = 0;
                        string tempTime = mytime.ToString();
                        mygameData.gameTime = @tempTime;
                        TimeClock.Stop();
                        Stop_Button.IsEnabled = false;
                        Rerange_Button.IsEnabled = false;
                        Mention_Button.IsEnabled = false;
                        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(Pic_ListForm); i++)
                        {
                            Visual childVisual = (Visual)VisualTreeHelper.GetChild(Pic_ListForm, i);
                            if (childVisual is Button)
                            {
                                (childVisual as Button).IsEnabled = false;
                            }
                        }
                        Game_Information.Text = "失败啦啦~";
                    }
                    else
                    {
                        coutTime++;
                        if (!ADmode)
                        {
                            mytime--;
                            string tempTime = mytime.ToString();
                            mygameData.gameTime = @tempTime;
                            if (coutTime == 4)
                            {
                                if (mygameData.GamePointsInc > 1)
                                    mygameData.GamePointsInc--;
                                coutTime = 0;
                            }
                        }
                        else
                        {
                            if (coutTime == ADmodeDiffculyt)
                            {
                                tempButtons.Clear();
                                Pic_ListForm.Children.Clear();
                                outterLayer++;
                                coutTime = 0;
                                int GridRow = 0;
                                for (int i = GameHeight - outterLayer + 1; i < GameHeight + 1; i++)
                                {
                                    int GridCol = 0;
                                    for (int j = 1; j < GameWidth + 1; j++)
                                    {
                                        int contentTemp = 0;
                                        if (i > 0)
                                            contentTemp = Pic_List[i, j];

                                        if (contentTemp != 0)
                                        {
                                            if (GridRow < 10)
                                            {
                                                Button myButton = new Button();

                                                myButton.Content = contentTemp.ToString();
                                                myButton.Cursor = Cursors.Hand;
                                                myButton.Click += Pic_ButtonClick;

                                                Pic_ListForm.Children.Add(myButton);
                                                Grid.SetColumn(myButton, GridCol);
                                                Grid.SetRow(myButton, GridRow);
                                            }
                                            else
                                            {
                                                TimeClock.Stop();
                                                Stop_Button.IsEnabled = false;
                                                Rerange_Button.IsEnabled = false;
                                                Mention_Button.IsEnabled = false;
                                                for (int ini = 0; ini < VisualTreeHelper.GetChildrenCount(Pic_ListForm); ini++)
                                                {
                                                    Visual childVisual = (Visual)VisualTreeHelper.GetChild(Pic_ListForm, ini);
                                                    if (childVisual is Button)
                                                    {
                                                        (childVisual as Button).IsEnabled = false;
                                                    }
                                                }
                                                Game_Information.Text = "失败啦啦~";
                                                
                                            }

                                        }
                                        GridCol++;
                                    }
                                    GridRow++;
                                }
                            }
                        }
                    }
                });
            TimeClock.Interval = new TimeSpan(0, 0, 1);
        }


        //初始化绑定源
        void InitializeCutom()
        {
            mygameData.gameTime = string.Empty;
            mygameData.gamePoints = 0;
            mygameData.TargetPoints = 0;
            mygameData.GamePointsInc = 1;
            mygameData.PropsBomb = 0;
            mygameData.PropsCold = 0;
            mygameData.PropsDouble = 0;
            mygameData.PropsTime = 0;
            mygameData.PropsCrazy = 0;
        }

        //初始化游戏界面
        private void StartButton(object sender, RoutedEventArgs e)

        {
            TimeClock.Stop();

            int initializeProps = 0;
            Props_ColdButton.IsEnabled = false;
            mygameData.PropsCold = @initializeProps;

            Props_CrazyButton.IsEnabled = false;
            mygameData.PropsCrazy = @initializeProps;

            Props_DoubleButton.IsEnabled = false;
            mygameData.PropsDouble = @initializeProps;

            Props_BombButton.IsEnabled = false;
            mygameData.PropsBomb = @initializeProps;

            Props_TimeButton.IsEnabled = false;
            mygameData.PropsTime = @initializeProps;

            Props_Bombstate = false;

            Props_Crazystate = false;

            mygameData.GamePointsInc = 1;

            if (!ADmode)
            {
                Rerange_Button.IsEnabled = true;

                Mention_Button.IsEnabled = true;
            }

            Stop_Information.Visibility = Visibility.Hidden;
            
            Stop_Button.Visibility = Visibility.Visible;
            Stop_Button.IsEnabled = true;
            
            Continue_Button.Visibility = Visibility.Hidden;
            
            mentionPics.Clear();
            
            int initializePoints = 0;
            mygameData.gamePoints = @initializePoints;

            mytime = 100;
            mygameData.gameTime = @"100";

            outterLayer = 0;
            if (ADmode)
            {
                outterLayer = 5;
                ADmodeLevel = 1;
                int the_targetpoints = 50;
                mygameData.TargetPoints = @the_targetpoints;
            }
            ADmodeDiffculyt = 20;
            
            Game_Information.Text = " ";

            Start_Button.Content = "重新开始";
            
            StartGame();

            Pic_ListForm.Children.Clear();

            if (!ADmode)
            {
                for (int i = 1; i < GameHeight + 1; ++i)
                {
                    for (int j = 1; j < GameWidth + 1; ++j)
                    {
                        int contentTemp = Pic_List[i, j];

                        Button myButton = new Button();

                        myButton.Content = contentTemp.ToString();
                        myButton.Cursor = Cursors.Hand;
                        myButton.Click += Pic_ButtonClick;

                        Pic_ListForm.Children.Add(myButton);
                        Grid.SetColumn(myButton, j - 1);
                        Grid.SetRow(myButton, i - 1);
                    }
                }
            }
            else
            {
                int GridRow = 0;
                for (int i = GameHeight - outterLayer + 1; i < GameHeight + 1; i++)
                {
                    int GridCol = 0;
                    for (int j = 1; j < GameWidth + 1; j++)
                    {
                        int contentTemp = Pic_List[i, j];

                        if (contentTemp != 0)
                        {
                            Button myButton = new Button();

                            myButton.Content = contentTemp.ToString();
                            myButton.Cursor = Cursors.Hand;
                            myButton.Click += Pic_ButtonClick;

                            Pic_ListForm.Children.Add(myButton);
                            Grid.SetColumn(myButton, GridCol);
                            Grid.SetRow(myButton, GridRow);
                        }
                        GridCol++;
                    }
                    GridRow++;
                }
            }

            bool re = MentionFuc();
            if (!re)
            {
                Game_Information.Text = "进死胡同了，赶快重排吧~";
            }
        }

        //每张图片点击触发
        private void Pic_ButtonClick(object sender, RoutedEventArgs e)
        {
            //炸弹状态
            if (Props_Bombstate)
            {
                int Bombpropstime = mygameData.PropsBomb;
                Bombpropstime--;
                mygameData.PropsBomb = Bombpropstime;
                Visual mychildVisual = (Visual)VisualTreeHelper.GetChild(Props_Box, 3);
                Visual buttonVisual = (Visual)VisualTreeHelper.GetChild(mychildVisual, 0);
                Visual textVisual = (Visual)VisualTreeHelper.GetChild(mychildVisual, 1);
                if ((textVisual as TextBlock).Text != "0")
                    (buttonVisual as Button).IsEnabled = true;
                Props_Bombstate = false;
                Props_BombCancelButton.Visibility = Visibility.Hidden;
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(Pic_ListForm); i++)
                {
                    Visual childVisual = (Visual)VisualTreeHelper.GetChild(Pic_ListForm, i);
                    if (childVisual is Button)
                    {
                        (childVisual as Button).MouseEnter -= Props_BombHover;
                        (childVisual as Button).MouseLeave -= Props_BombLeave;
                        (childVisual as Button).BorderBrush = new SolidColorBrush(Color.FromRgb(112, 112, 112));
                        (childVisual as Button).BorderThickness = new Thickness(1);
                    }
                }
                Button[] bombButtons = new Button[GameDiffculty];
                int row;
                int col;
                row = Grid.GetRow((sender as Button));
                col = Grid.GetColumn((sender as Button));

                int[] rowline = new int[] { -1, 0, 1, 2 };
                int[] colline = new int[] { -1, 0, 1, 2 };

                if (row == 0)
                {
                    rowline[0] = 3;
                }
                if (row == 8)
                {
                    rowline[3] = -2;
                }
                if (row == 9)
                {
                    rowline[2] = -2;
                    rowline[3] = -3;
                }
                if (col == 0)
                {
                    colline[0] = 3;
                }
                if (col == 8)
                {
                    colline[3] = -2;
                }
                if (col == 9)
                {
                    colline[2] = -2;
                    colline[3] = -3;
                }
                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < 4; ++j)
                    {
                        int Button_index = (row + rowline[i]) * 10 + col + colline[j];
                        Visual childVisual = (Visual)VisualTreeHelper.GetChild(Pic_ListForm, Button_index);
                        int Button_Content = Convert.ToInt16((childVisual as Button).Content)-1;
                        if (Button_Content != -1 && Button_Content != -2)
                        {
                            if (bombButtons[Button_Content] == null)
                                bombButtons[Button_Content] = (childVisual as Button);
                            else
                            {
                                int font_y = Grid.GetColumn(bombButtons[Button_Content]) + 1;
                                int font_x = Grid.GetRow(bombButtons[Button_Content]) + 1;
                                int back_y = col + colline[j] + 1;
                                int back_x = row + rowline[i] + 1;
                                Pic_List[font_x, font_y] = 0;
                                Pic_List[back_x, back_y] = 0;
                                (childVisual as Button).Content = "0";
                                bombButtons[Button_Content].Content = "0";
                                (childVisual as Button).Visibility = Visibility.Hidden;
                                bombButtons[Button_Content].Visibility = Visibility.Hidden;
                                int addGamePoints = mygameData.gamePoints;
                                addGamePoints = addGamePoints + mygameData.GamePointsInc;
                                if (!ADmode)
                                {
                                    mygameData.gamePoints = @addGamePoints;
                                    mygameData.GamePointsInc++;
                                }
                                mentionPics.Clear();
                                if (!MentionFuc())
                                {
                                    Game_Information.Text = "进死胡同了，赶快重排吧~";
                                }
                                bombButtons[Button_Content] = null;
                            }
                        }
                    }
                }
            }
            //如果是点击的第一张图片，则存入
            else if (tempButtons.Count != 1)
            {
                tempButtons.Add(sender as Button);
                (sender as Button).BorderBrush = new SolidColorBrush(Color.FromRgb(255, 96, 0));
            }
            //如果两次点击的是同一张图片，则认定为是取消上次操作
            else if (tempButtons[0] == (sender as Button))
            {
                tempButtons[0].BorderBrush = new SolidColorBrush(Color.FromRgb(112, 112, 112));
                tempButtons.RemoveAt(0);
            }
            //如果是第二张，则进行判断
            else
            {
                //先判断两个图片是否一样
                if (Convert.ToInt16(tempButtons[0].Content) != Convert.ToInt16((sender as Button).Content))
                {
                    tempButtons[0].BorderBrush = new SolidColorBrush(Color.FromRgb(112, 112, 112));
                    tempButtons.RemoveAt(0);
                    mygameData.GamePointsInc = 1;
                }
                else if (Props_Crazystate)
                {
                    int col;
                    int row;

                    int[] sPoint;
                    int[] ePoint;

                    col = Grid.GetColumn(tempButtons[0]);
                    row = Grid.GetRow(tempButtons[0]);
                    //第一张图片的位置
                    sPoint = new int[] { row + 1, col + 1 };

                    col = Grid.GetColumn(sender as Button);
                    row = Grid.GetRow(sender as Button);
                    //第二张图片的位置
                    ePoint = new int[] { row + 1, col + 1 };

                    bool re;
                    if (Pic_List[sPoint[0], sPoint[1]] != -1)
                    {
                        Pic_List[sPoint[0], sPoint[1]] = 0;
                        Pic_List[ePoint[0], ePoint[1]] = 0;
                        tempButtons[0].Content = "0";
                        (sender as Button).Content = "0";
                        tempButtons[0].Visibility = Visibility.Hidden;
                        (sender as Button).Visibility = Visibility.Hidden;
                        int addGamePoints = mygameData.gamePoints;
                        addGamePoints = addGamePoints + mygameData.GamePointsInc;
                        if (!ADmode)
                        {
                            mygameData.gamePoints = @addGamePoints;
                            mygameData.GamePointsInc++;
                        }
                        mentionPics.Clear();
                        re = MentionFuc();
                        if (!re)
                        {
                            Game_Information.Text = "进死胡同了，赶快重排吧~";
                        }
                    }
                    tempButtons[0].BorderBrush = new SolidColorBrush(Color.FromRgb(112, 112, 112));
                    tempButtons.RemoveAt(0);                        
                }
                else
                {
                    int col;
                    int row;

                    int[] sPoint;
                    int[] ePoint;

                    col = Grid.GetColumn(tempButtons[0]);
                    row = Grid.GetRow(tempButtons[0]);
                    //第一张图片的位置 16 1 20 - outter + 1
                    sPoint = new int[] { row + 1, col + 1 };
                    if(ADmode)
                    sPoint = new int[] { GameHeight - outterLayer+ row + 1, col + 1 };


                    col = Grid.GetColumn(sender as Button);
                    row = Grid.GetRow(sender as Button);
                    //第二张图片的位置
                    ePoint = new int[] { row + 1, col + 1 };
                    if(ADmode)
                        ePoint = new int[] { GameHeight - outterLayer + row + 1, col + 1 };
                    bool re;

                    //依次进行直线相连，一个转角相连，两个转角相连的判断
                    bool StraightLine;
                    StraightRute(sPoint, ePoint, out StraightLine);
                    if (StraightLine || SingleCornerRute(sPoint, ePoint) || DoubleCornerRute(sPoint, ePoint))
                    {
                        if (Pic_List[sPoint[0], sPoint[1]] == -1)
                        {
                            int thePropskind = AddProps();
                            Visual childVisual = (Visual)VisualTreeHelper.GetChild(Props_Box, thePropskind - 1);
                            Visual innerVisual = (Visual)VisualTreeHelper.GetChild(childVisual, 0);
                            (innerVisual as Button).IsEnabled = true;
                        }
                            Pic_List[sPoint[0], sPoint[1]] = 0;
                            Pic_List[ePoint[0], ePoint[1]] = 0;
                            tempButtons[0].Content = "0";
                            (sender as Button).Content = "0";
                            tempButtons[0].Visibility = Visibility.Hidden;
                            (sender as Button).Visibility = Visibility.Hidden;
                        if(ADmode)
                        {
                            if (sPoint[0] < ePoint[0])
                            {
                                int tempPoint = sPoint[0];
                                int tempPoint_two = sPoint[1];
                                sPoint[0] = ePoint[0];
                                sPoint[1] = ePoint[1];
                                ePoint[0] = tempPoint;
                                ePoint[1] = tempPoint_two;
                            }

                            while (sPoint[0] < GameHeight + 1)
                            {
                                Pic_List[sPoint[0], sPoint[1]] = Pic_List[sPoint[0] + 1, sPoint[1]];
                                sPoint[0]++;
                            }
                            Pic_List[GameHeight + 1, sPoint[1]] = 0;
                            while (ePoint[0] < GameHeight + 1)
                            {
                                Pic_List[ePoint[0], ePoint[1]] = Pic_List[ePoint[0] + 1, ePoint[1]];
                                ePoint[0]++;
                            }
                            Pic_List[GameHeight + 1, ePoint[1]] = 0;
                            Pic_ListForm.Children.Clear();
                            int GridRow = 0;
                            for (int i = GameHeight - outterLayer + 1; i < GameHeight + 1; i++)
                            {
                                int GridCol = 0;
                                for (int j = 1; j < GameWidth + 1; j++)
                                {
                                    int contentTemp = 0;
                                    if (i > 0)
                                        contentTemp = Pic_List[i, j];

                                    if (contentTemp != 0)
                                    {
                                        Button myButton = new Button();

                                        myButton.Content = contentTemp.ToString();
                                        myButton.Cursor = Cursors.Hand;
                                        myButton.Click += Pic_ButtonClick;

                                        Pic_ListForm.Children.Add(myButton);
                                        Grid.SetColumn(myButton, GridCol);
                                        Grid.SetRow(myButton, GridRow);
                                    }
                                    GridCol++;
                                }
                                GridRow++;
                            }
                        }
                        tempButtons.RemoveAt(0);
                        int addGamePoints = mygameData.gamePoints;
                        addGamePoints = addGamePoints + mygameData.GamePointsInc;
                        mygameData.gamePoints = @addGamePoints;
                        if (!ADmode)
                        {
                            mygameData.GamePointsInc++;
                            
                            mytime = mytime + 2;
                        }
                        else if (mygameData.gamePoints >= mygameData.TargetPoints)
                        {
                            TimeClock.Stop();
                            outterLayer = 5;
                            int tempPoints = 0;
                            mygameData.gamePoints = tempPoints;
                            Stop_Button.IsEnabled = false;
                            Rerange_Button.IsEnabled = false;
                            Mention_Button.IsEnabled = false;
                            for (int ini = 0; ini < VisualTreeHelper.GetChildrenCount(Pic_ListForm); ini++)
                            {
                                Visual childVisual = (Visual)VisualTreeHelper.GetChild(Pic_ListForm, ini);
                                if (childVisual is Button)
                                {
                                    (childVisual as Button).IsEnabled = false;
                                }
                            }
                            
                            if (ADmodeLevel < 6)                            
                                GameDiffculty++;                          
                            else if (ADmodeLevel < 11)
                            {
                                int tempTarget = 50 + (ADmodeLevel - 5) * 10;
                                mygameData.TargetPoints = @tempTarget;
                            }
                            else if (ADmodeLevel < 16)
                            {
                                ADmodeDiffculyt--;
                                int tempTarget = 50;
                                mygameData.TargetPoints = @tempTarget;
                            }
                            else if (ADmodeLevel < 21)
                            {
                                int tempTarget = 50 + (ADmodeLevel - 15) * 10;
                                mygameData.TargetPoints = @tempTarget;
                            }
                            else if (ADmodeLevel < 24)
                            {
                                GameDiffculty++;
                                ADmodeDiffculyt--;
                                int tempTarget = 50;
                                mygameData.TargetPoints = @tempTarget;
                            }
                            else if (ADmodeLevel < 29)
                            {
                                int tempTarget = 50 + (ADmodeLevel - 23) * 10;
                                mygameData.TargetPoints = @tempTarget;
                            }
                            ADmodeLevel++;
                            int LevelClockCount = 5;
                            DispatcherTimer LevelClock = new System.Windows.Threading.DispatcherTimer();
                            LevelClock.Interval = TimeSpan.FromSeconds(1.0);
                            LevelClock.Tick += new EventHandler(delegate(object s, EventArgs a)
                            {
                                if (LevelClockCount > 0)
                                {
                                    LevelClockCount--;
                                    Game_Information.Text = "过关啦啦~还有" + LevelClockCount + "秒进入第" + ADmodeLevel + "关，加油哦~";
                                }
                                else
                                {
                                    LevelClock.Stop();
                                    
                                    StartGame();
                                    Game_Information.Text = "第"+ADmodeLevel+"关";
                                    Pic_ListForm.Children.Clear();
                                    int GridRow = 0;
                                    for (int i = GameHeight - outterLayer + 1; i < GameHeight + 1; i++)
                                    {
                                        int GridCol = 0;
                                        for (int j = 1; j < GameWidth + 1; j++)
                                        {
                                            int contentTemp = Pic_List[i, j];

                                            if (contentTemp != 0)
                                            {
                                                Button myButton = new Button();

                                                myButton.Content = contentTemp.ToString();
                                                myButton.Cursor = Cursors.Hand;
                                                myButton.Click += Pic_ButtonClick;

                                                Pic_ListForm.Children.Add(myButton);
                                                Grid.SetColumn(myButton, GridCol);
                                                Grid.SetRow(myButton, GridRow);
                                            }
                                            GridCol++;
                                        }
                                        GridRow++;
                                    }
                                }
                            });
                            LevelClock.Start();
                            Game_Information.Text = "过关啦啦~还有" + LevelClockCount + "秒进入第" + ADmodeLevel + "关，加油哦~";
                        }
                        mentionPics.Clear();
                        re = MentionFuc();
                        if (!re)
                        {
                            Game_Information.Text = "进死胡同了，赶快重排吧~";
                        }
                    }
                    /*
                                        else if (SingleCornerRute(sPoint, ePoint))
                                        {
                                            Pic_List[sPoint[0], sPoint[1]] = 0;
                                            Pic_List[ePoint[0], ePoint[1]] = 0;
                                            tempButtons[0].Visibility = Visibility.Hidden;
                                            (sender as Button).Visibility = Visibility.Hidden;
                                            tempButtons.RemoveAt(0);
                                            mytime = mytime + 2;
                                            mentionPics.Clear();
                                            re = MentionFuc();
                                            if (!re)
                                            {
                                                Game_Information.Text = "进死胡同了，赶快重排吧~";
                                            }
                                        }

                                        else if (DoubleCornerRute(sPoint, ePoint))
                                        {
                                            Pic_List[sPoint[0], sPoint[1]] = 0;
                                            Pic_List[ePoint[0], ePoint[1]] = 0;
                                            tempButtons[0].Visibility = Visibility.Hidden;
                                            (sender as Button).Visibility = Visibility.Hidden;
                                            tempButtons.RemoveAt(0);
                                            mytime = mytime + 2;
                                            mentionPics.Clear();
                                            re = MentionFuc();
                                            if (!re)
                                            {
                                                Game_Information.Text = "进死胡同了，赶快重排吧~";
                                            }
                                        }*/

                    //如果都不能相连，则删除存入的图片
                    else
                    {
                        tempButtons[0].BorderBrush = new SolidColorBrush(Color.FromRgb(112, 112, 112));
                        tempButtons.RemoveAt(0);
                        mygameData.GamePointsInc =1;
                    }
                }
            }
            if (EmptyMatrix() && !ADmode)
            {
                TimeClock.Stop();
                Stop_Button.IsEnabled = false;
                Rerange_Button.IsEnabled = false;
                Mention_Button.IsEnabled = false;
                Game_Information.Text = "游戏完成！";
            }
        }

        //炸弹触发状态
        private void Props_BombHover(object sender, RoutedEventArgs e)
        {
            int row;
            int col;
            row = Grid.GetRow((sender as Button));
            col = Grid.GetColumn((sender as Button));

            int[] rowline = new int[] { -1,0, 1, 2 };
            int[] colline = new int[] { -1,0, 1, 2 };

            if (row == 0)
            {
                rowline[0] = 3;
            }
            if (row == 8)
            {
                rowline[3] = -2;                
            }
            if (row == 9)
            {
                rowline[2] = -2;
                rowline[3] = -3;
            }
            if (col == 0)
            {
                colline[0] = 3;
            }
            if (col == 8)
            {
                colline[3] = -2;
            }
            if (col == 9)
            {
                colline[2] = -2;
                colline[3] = -3;
            }
            List<Button> Bomb_Buttons = new List<Button>();
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    int Button_index = (row + rowline[i]) * 10 + col + colline[j];
                    Visual childVisual = (Visual)VisualTreeHelper.GetChild(Pic_ListForm, Button_index);
                    Bomb_Buttons.Add((childVisual as Button));
                }
            }
            foreach (Button temp in Bomb_Buttons)
            {
                temp.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 96, 0));
                temp.BorderThickness = new Thickness(2);
            }

        }

        private void Props_BombLeave(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(Pic_ListForm); i++)
            {
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(Pic_ListForm, i);
                if (childVisual is Button)
                {
                    (childVisual as Button).BorderBrush = new SolidColorBrush(Color.FromRgb(112, 112, 112));
                    (childVisual as Button).BorderThickness = new Thickness(1);
                }
            }
        }

        //炸弹状态取消
        private void Props_BombCancel(object sender, RoutedEventArgs e)
        {
            Props_BombCancelButton.Visibility = Visibility.Hidden;
            Visual mychildVisual = (Visual)VisualTreeHelper.GetChild(Props_Box, 3);
            Visual buttonVisual = (Visual)VisualTreeHelper.GetChild(mychildVisual, 0);
            Visual textVisual = (Visual)VisualTreeHelper.GetChild(mychildVisual, 1);
            if ((textVisual as TextBlock).Text != "0")
                (buttonVisual as Button).IsEnabled = true;
            Props_Bombstate = false;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(Pic_ListForm); i++)
            {
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(Pic_ListForm, i);
                if (childVisual is Button)
                {
                    (childVisual as Button).MouseEnter -= Props_BombHover;
                    (childVisual as Button).MouseLeave -= Props_BombLeave;
                    (childVisual as Button).BorderBrush = new SolidColorBrush(Color.FromRgb(112, 112, 112));
                    (childVisual as Button).BorderThickness = new Thickness(1);
                }
            }
        }

        //冷冻
        private void Props_ColdClick(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            int Coldpropstime = mygameData.PropsCold;
            Coldpropstime--;
            mygameData.PropsCold = @Coldpropstime;
            TimeClock.Stop();
            DispatcherTimer ColdClock = new System.Windows.Threading.DispatcherTimer();
            ColdClock.Interval = TimeSpan.FromSeconds(10.0);
            ColdClock.Tick += new EventHandler(delegate(object s, EventArgs a)
                {
                    Visual childVisual = (Visual)VisualTreeHelper.GetChild(Props_Box, 0);
                    Visual buttonVisual = (Visual)VisualTreeHelper.GetChild(childVisual, 0);
                    Visual textVisual = (Visual)VisualTreeHelper.GetChild(childVisual, 1);
                    if((textVisual as TextBlock).Text != "0")
                        (buttonVisual as Button).IsEnabled = true;
                    ColdClock.Stop();
                    TimeClock.Start();
                });
            ColdClock.Start();            
        }

        //狂暴
        private void Props_CrazyClick(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            int Crazypropstime = mygameData.PropsCrazy;
            Crazypropstime--;
            mygameData.PropsCrazy = @Crazypropstime;
            Props_Crazystate = true;
            DispatcherTimer CrazyClock = new System.Windows.Threading.DispatcherTimer();
            CrazyClock.Interval = TimeSpan.FromSeconds(10.0);
            CrazyClock.Tick += new EventHandler(delegate(object s, EventArgs a)
            {
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(Props_Box, 1);
                Visual buttonVisual = (Visual)VisualTreeHelper.GetChild(childVisual, 0);
                Visual textVisual = (Visual)VisualTreeHelper.GetChild(childVisual, 1);
                if ((textVisual as TextBlock).Text != "0")
                    (buttonVisual as Button).IsEnabled = true;
                CrazyClock.Stop();
                Props_Crazystate = false;
            });
            CrazyClock.Start();    
        }

        //双倍
        private void Props_DoubleClick(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            int Doublepropstime = mygameData.PropsDouble;
            Doublepropstime--;
            mygameData.PropsDouble = Doublepropstime;
            mygameData.GamePointsInc = mygameData.GamePointsInc*2;
            DispatcherTimer DoubleClock = new System.Windows.Threading.DispatcherTimer();
            DoubleClock.Interval = TimeSpan.FromSeconds(10.0);
            DoubleClock.Tick += new EventHandler(delegate(object s, EventArgs a)
            {
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(Props_Box, 2);
                Visual buttonVisual = (Visual)VisualTreeHelper.GetChild(childVisual, 0);
                Visual textVisual = (Visual)VisualTreeHelper.GetChild(childVisual, 1);
                if ((textVisual as TextBlock).Text != "0")
                    (buttonVisual as Button).IsEnabled = true;
                DoubleClock.Stop();
                mygameData.GamePointsInc = mygameData.GamePointsInc/2;
            });
            DoubleClock.Start();   
        }
        
        //炸弹
        private void Props_BombClick(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            Props_BombCancelButton.Visibility = Visibility.Visible;
            Props_Bombstate = true;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(Pic_ListForm); i++)
            {
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(Pic_ListForm, i);
                if (childVisual is Button)
                {
                    (childVisual as Button).MouseEnter += Props_BombHover;
                    (childVisual as Button).MouseLeave += Props_BombLeave;
                }
            }
        }

        //加时
        private void Props_TimeClick(object sender, RoutedEventArgs e)
        {
            int Timepropstime = mygameData.PropsTime;
            Timepropstime--;
            mygameData.PropsTime = Timepropstime;
            mytime = mytime + 10;
            Visual childVisual = (Visual)VisualTreeHelper.GetChild(Props_Box, 4);
            Visual buttonVisual = (Visual)VisualTreeHelper.GetChild(childVisual, 0);
            Visual textVisual = (Visual)VisualTreeHelper.GetChild(childVisual, 1);
            if ((textVisual as TextBlock).Text == "0")
                (buttonVisual as Button).IsEnabled = false;
        }

        //结束游戏
        private void EndGame(object sender, RoutedEventArgs e)
        {
            TimeClock.Stop();
            StartPage mystart = new StartPage();
            mystart.ParentWindow = ParentWindow;
            ParentWindow.Content = mystart;
        }

        //初始化游戏
        public void StartGame()
        {
            Pic_List = new int[GameHeight + 2, GameWidth + 2];

            List<int> Data = new List<int>();
            Random raData = new Random();
            for (int i = 0; i < (GameWidth * GameHeight) / 2; ++i)
            {
                if (i < Propsnum)
                {
                    Data.Add(-1);
                    Data.Add(-1);
                }
                else
                {
                    int temp = 0;
                    temp = raData.Next(1, GameDiffculty);
                    Data.Add(temp);
                    Data.Add(temp);
                }
            }

            for (int i = 0; i < GameHeight + 2; i++)
            {
                for (int j = 0; j < GameWidth + 2; j++)
                {
                    int temp = 0;
                    if (!(i == 0 || i == GameHeight + 1 || j == 0 || j == GameWidth + 1))
                    {
                        int num = 0;
                        if (Data.Count != 1)
                            num = raData.Next(0, Data.Count);
                        temp = Data[num];
                        Data.RemoveAt(num);
                    }
                    Pic_List[i, j] = temp;
                }
            }
            coutTime = 0;
            TimeClock.Start();
        }

        //暂停游戏
        private void StopGame(object sender, RoutedEventArgs e)
        {
            TimeClock.Stop();
            Stop_Information.Visibility = Visibility.Visible;
            Stop_Button.Visibility = Visibility.Hidden;
            Continue_Button.Visibility = Visibility.Visible;
        }

        //继续游戏
        private void ContinueGame(object sender, RoutedEventArgs e)
        {
            TimeClock.Start();
            Stop_Information.Visibility = Visibility.Hidden;
            Stop_Button.Visibility = Visibility.Visible;
            Continue_Button.Visibility = Visibility.Hidden;
        }

        //重排游戏
        private void RerangeGame(object sender, RoutedEventArgs e)
        {
            RerangePicList();
            Pic_ListForm.Children.Clear();
            Game_Information.Text = "";
            mentionPics.Clear();

            for (int i = 1; i < GameHeight + 1; ++i)
            {
                for (int j = 1; j < GameWidth + 1; ++j)
                {
                    int contentTemp = Pic_List[i, j];

                    if (contentTemp != 0)
                    {
                        Button myButton = new Button();

                        myButton.Content = contentTemp.ToString();
                        myButton.Cursor = Cursors.Hand;
                        myButton.Click += Pic_ButtonClick;

                        Pic_ListForm.Children.Add(myButton);
                        Grid.SetColumn(myButton, j - 1);
                        Grid.SetRow(myButton, i - 1);
                    }
                }
            }
            bool re = MentionFuc();
            if (!re)
            {
                Game_Information.Text = "进死胡同了，赶快重排吧~";
            }
        }

        //提示路径
        private void MentionGame(object sender, RoutedEventArgs e)
        {
            if (mentionPics.Count != 2)
            {
                Game_Information.Text = "都进入死胡同了，还提示什么啊~重排吧，少年~";
            }
            else
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(Pic_ListForm); i++)
                {
                    Visual childVisual = (Visual)VisualTreeHelper.GetChild(Pic_ListForm, i);
                    if (childVisual is Button)
                    {
                        (childVisual as Button).BorderBrush = new SolidColorBrush(Color.FromRgb(112, 112, 112));
                        (childVisual as Button).BorderThickness = new Thickness(1);
                        if (Grid.GetColumn((childVisual as Button)) == (mentionPics[0][1] - 1))
                        {
                            if (Grid.GetRow((childVisual as Button)) == (mentionPics[0][0] - 1))
                            {
                                (childVisual as Button).BorderBrush = new SolidColorBrush(Color.FromRgb(255, 96, 0));
                                (childVisual as Button).BorderThickness = new Thickness(2);
                            }
                        }
                        if (Grid.GetColumn((childVisual as Button)) == (mentionPics[1][1] - 1))
                        {
                            if (Grid.GetRow((childVisual as Button)) == (mentionPics[1][0] - 1))
                            {
                                (childVisual as Button).BorderBrush = new SolidColorBrush(Color.FromRgb(255, 96, 0));
                                (childVisual as Button).BorderThickness = new Thickness(2);
                            }
                        }
                    }
                }
                mytime = mytime - 5;
            }
        }

        //添加道具
        public int AddProps()
        {
            Random raProps = new Random();
            int Propskind = raProps.Next(1, 6);
            int tempProps;
            switch (Propskind)
            {
                case 1:
                    tempProps = mygameData.PropsCold;
                    tempProps++;
                    mygameData.PropsCold = @tempProps;
                    break;
                case 2:
                    tempProps = mygameData.PropsCrazy;
                    tempProps++;
                    mygameData.PropsCrazy = @tempProps;
                    break;
                case 3:
                    tempProps = mygameData.PropsDouble;
                    tempProps++;
                    mygameData.PropsDouble = @tempProps;
                    break;
                case 4:
                    tempProps = mygameData.PropsBomb;
                    tempProps++;
                    mygameData.PropsBomb = @tempProps;
                    break;
                case 5:
                    tempProps = mygameData.PropsTime;
                    tempProps++;
                    mygameData.PropsTime = @tempProps;
                    break;
                default:  break;
            }
            return Propskind;
        }

        //重排矩阵
        public void RerangePicList()
        {
            List<int> tempPic_List = new List<int>();
            foreach (int temp in Pic_List)
            {
                if (temp != 0)
                {
                    tempPic_List.Add(temp);
                }
            }

            Random myrad = new Random();
            int num;
            for (int i = 1; i < GameHeight + 1; ++i)
            {
                for (int j = 1; j < GameWidth + 1; ++j)
                {
                    if (Pic_List[i, j] != 0 && tempPic_List.Count != 0)
                    {
                        num = myrad.Next(0, tempPic_List.Count );
                        Pic_List[i, j] = tempPic_List[num];
                        tempPic_List.RemoveAt(num);
                    }
                }
            }
        }

        //提示函数
        public bool FindRute(int sPointx, int sPointy, out int ePointx, out int ePointy, int key)
        {
            int temp = sPointx - 1;
            while (temp >= 0 && Pic_List[temp,sPointy] == 0)
            {
                PointQueue.Enqueue(new Pointnode(temp, sPointy));
                temp--;
            }
            if (temp >= 0 && Pic_List[temp,sPointy] == key)
            {
                ePointx = temp;
                ePointy = sPointy;
                return true;
            }
            temp = sPointy + 1;
            while (temp < GameWidth && Pic_List[sPointx,temp] == 0)
            {
                PointQueue.Enqueue(new Pointnode(sPointx, temp));
                temp++;
            }
            if (temp < GameWidth && Pic_List[sPointx,temp] == key)
            {
                ePointx = sPointx;
                ePointy = temp;
                return true;
            }
            temp = sPointx + 1;
            while (temp < GameHeight && Pic_List[temp,sPointy] == 0)
            {
                PointQueue.Enqueue(new Pointnode(temp, sPointy));
                temp++;
            }
            if (temp < GameHeight && Pic_List[temp,sPointy] == key)
            {
                ePointx = temp;
                ePointy = sPointy;
                return true;
            }
            temp = sPointy - 1;
            while (temp >= 0 && Pic_List[sPointx,temp] == 0)
            {
                PointQueue.Enqueue(new Pointnode(sPointx, temp));
                temp--;
            }
            if (temp >= 0 && Pic_List[sPointx,temp] == key)
            {
                ePointx = sPointx;
                ePointy = temp;
                return true;
            }
            ePointx = -1;
            ePointy = -1;
            return false;
        }
        public bool MentionRute(int sPointx, int sPointy, out int ePointx, out int ePointy, int key)
        {
            Pic_List[sPointx,sPointy] = 1000;
            if (FindRute(sPointx, sPointy, out ePointx, out ePointy, key) && Pic_List[ePointx, ePointy] == key)
            {
                Pic_List[sPointx, sPointy] = key;
                PointQueue.Clear();
                return true;
            }
            int count = 2;
            while (PointQueue.Count()!=0 && count != 0)
            {
                int t = PointQueue.Count();
                for (int i = 0; i < t; ++i)
                {
                    if (FindRute(PointQueue.Peek().x, PointQueue.Peek().y, out ePointx, out ePointy, key) && Pic_List[ePointx, ePointy] == key)
                    {
                        Pic_List[sPointx,sPointy] = key;
                        PointQueue.Clear();
                        return true;
                    }
                    PointQueue.Dequeue();
                }
                count--;
            }
            PointQueue.Clear();
            Pic_List[sPointx,sPointy] = key;
            return false;
        }

        public bool MentionFuc()
        {
            for(int i = 1;i<GameHeight+1;++i)
            {
                for (int j = 1; j < GameWidth + 1; ++j)
                {
                    if (Pic_List[i, j] != 0)
                    {
                        int tempx;
                        int tempy;
                        bool re;
                        re = MentionRute(i, j, out tempx, out tempy, Pic_List[i, j]);
                        if (re)
                        {
                            mentionPics.Add(new int[] { i, j });
                            mentionPics.Add(new int[] { tempx, tempy });
                            return true;
                        }
                    }
                }
            }
            return false;
        }



        //主要逻辑函数
        //判断点击的两个图片能否直接相连
        public void StraightRute(int[] sPoint, int[] ePoint, out bool re)
        {
            re = true;
            int GapWidth = sPoint[1] - ePoint[1];
            int GapHeight = sPoint[0] - ePoint[0];
            int aGapWidth = System.Math.Abs(GapWidth);
            int aGapHeight = System.Math.Abs(GapHeight);
            if (sPoint[0] != ePoint[0] && sPoint[1] != ePoint[1])
                re = false;
            else if (GapWidth == 0)
            {
                for (int i = 1; i < aGapHeight; ++i)
                {
                    if (!(Pic_List[sPoint[0] - (GapHeight / aGapHeight) * i, sPoint[1]] == 0))
                        re = false;
                }
            }
            else if (GapHeight == 0)
            {
                for (int i = 1; i < aGapWidth; ++i)
                {
                    if (!(Pic_List[sPoint[0], sPoint[1] - (GapWidth / aGapWidth) * i] == 0))
                        re = false;
                }
            }
            else
                re = true;
        }
        //判断点击的两个图片能否通过一个转角相连
        public bool SingleCornerRute(int[] sPoint, int[] ePoint)
        {
            bool SingleCornerLine;
            int[] Corner = new int[2];
            Corner[0] = sPoint[0];
            Corner[1] = ePoint[1];
            if (Pic_List[Corner[0], Corner[1]] == 0)
            {
                StraightRute(Corner, sPoint, out SingleCornerLine);
                if (SingleCornerLine)
                {
                    StraightRute(Corner, ePoint, out SingleCornerLine);
                    if (SingleCornerLine)
                        return true;
                }
            }
            Corner[0] = ePoint[0];
            Corner[1] = sPoint[1];
            if (Pic_List[Corner[0], Corner[1]] == 0)
            {
                StraightRute(Corner, sPoint, out SingleCornerLine);
                if (SingleCornerLine)
                {
                    StraightRute(Corner, ePoint, out SingleCornerLine);
                    if (SingleCornerLine)
                        return true;
                }
            }
            return false;
        }
        //判断点击的两个图片能否通过两个转角相连
        public bool DoubleCornerRute(int[] sPoint, int[] ePoint)
        {
            bool DoubleCornerLine;
            int[] Temp = new int[2];

            for (int i = 1; i <= sPoint[1]; ++i)
            {
                Temp[0] = sPoint[0];
                Temp[1] = sPoint[1] - i;
                if (Pic_List[Temp[0], Temp[1]] != 0)
                    break;
                DoubleCornerLine = SingleCornerRute(Temp, ePoint);
                if (DoubleCornerLine)
                    return true;
            }

            for (int i = 1; i <= Pic_List.GetLength(1) - sPoint[1] - 1; ++i)
            {
                Temp[0] = sPoint[0];
                Temp[1] = sPoint[1] + i;
                if (Pic_List[Temp[0], Temp[1]] != 0)
                    break;
                DoubleCornerLine = SingleCornerRute(Temp, ePoint);
                if (DoubleCornerLine)
                    return true;
            }

            for (int i = 1; i <= Pic_List.GetLength(0) - sPoint[0] - 1; ++i)
            {
                Temp[0] = sPoint[0] + i;
                Temp[1] = sPoint[1];
                if (Pic_List[Temp[0], Temp[1]] != 0)
                    break;
                DoubleCornerLine = SingleCornerRute(Temp, ePoint);
                if (DoubleCornerLine)
                    return true;
            }

            for (int i = 1; i <= sPoint[0]; ++i)
            {
                Temp[0] = sPoint[0] - i;
                Temp[1] = sPoint[1];
                if (Pic_List[Temp[0], Temp[1]] != 0)
                    break;
                DoubleCornerLine = SingleCornerRute(Temp, ePoint);
                if (DoubleCornerLine)
                    return true;
            }
            return false;
        }
        //判断是否还存在图片
        public bool EmptyMatrix()
        {
            for (int i = 0; i < Pic_List.GetLength(0); i++)
            {
                for (int j = 0; j < Pic_List.GetLength(1); j++)
                {
                    if (Pic_List[i, j] != 0)
                        return false;
                }
            }
            return true;
        }

    }
}
