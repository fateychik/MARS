using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Windows.Forms.DataVisualization.Charting;




namespace WindowsFormsApplication1
{
	
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
            InitializeComponents();
            InterfaceDraw();
            EmptyEverything();
        }

        int x = 10;                                             // значения размерности
        int y = 10;
		bool draw = false;
        bool rightMouseButton = false;
        bool startPointIsSet = false;
        (int x, int y) startPoint;
		int[,] mapArray;
        int[,] robotMapArray;

        List<(int x, int y)> prevCoordList;

        Thread os;                                              // поток для вычисления OS
        Thread map;                                             // поток для отрисовки карты с роботов
        ConcurrentQueue<List<(int x, int y)>> robotsCoordinate; // очередь для передачи координат робота между потоками
        delegate void RobotMap(Bitmap bmp);                     // для изменения пикчербокса из стороннег потока
        delegate void resultLabelDelegate(string resStr);
        int sleepTime = 200;

        int sideSize = 10;                                      // размер стороны квадрата
        static int lineWidth = 1;                               // ширина линии квадрата

        int robotNum = 1;
        int[] robotNums;
        int steps;
        int[] stepsArray;
        List<int> distances;
        List<int>[] distancesArray;

		PictureBox globalMapPictureBox = new PictureBox();
        PictureBox robotMapPictureBox = new PictureBox();
		Bitmap globalMap;
		Graphics globalMapGraphics;
        Bitmap robotMap;
        Graphics robotMapGraphics;

		Button saveButton = new Button ();
		Button loadButton = new Button ();
		Button createButton = new Button ();
        Button savedMapsButton = new Button();
        Button startButton = new Button();
        Label xTrackBarLabel = new Label ();
		Label yTrackBarLabel = new Label ();
        Label robotNumBarLabel = new Label();
        Label resultLabel = new Label();
		TrackBar xTrackBar = new TrackBar ();
		TrackBar yTrackBar = new TrackBar ();
        TrackBar robotNumBar = new TrackBar ();
        TextBox robotNumTextBox = new TextBox();

        CheckedListBox savedMaps = new CheckedListBox();

        int formXBorderShift = 50;
        int formYBorderShift = 180;
        Size buttonSize = new Size (100, 25);
        int buttonShift = 115;
        int buttonLocationY = 10;
        int buttonNum = 5;
        Size labelSize = new Size(40, 40);
        Size bigLabelSize = new Size(90, 40);
        int labelShift = 160;
        int labelLocationY = 50;
        int barShift = 40;
        Size textBoxSize = new Size(185, 40);
        Size chartSize = new Size(400, 400);
        Size smallChartSize = new Size(200, 200);
        int chartShift = 10;

        Pen emptyRectPen = new Pen(Color.Gray, lineWidth);       //линия пустой клетки
		SolidBrush takenRectBrush = new SolidBrush(Color.Black); //зарисовка занятой клетки
        SolidBrush emptyRectBrush = new SolidBrush(Color.LightGray);
        SolidBrush unknownRectBrush = new SolidBrush(Color.LightBlue);
        SolidBrush robotRectBrush = new SolidBrush(Color.Red);
        SolidBrush startPointRectBrush = new SolidBrush(Color.Orange);

        void InitializeComponents()
        {
            this.Size = new Size(buttonShift * 5, 350); //окно программы

            loadButton.Location = new Point(10, buttonLocationY);
            loadButton.Size = buttonSize;
            loadButton.Text = "Загрузить";
            loadButton.MouseClick += new MouseEventHandler(LoadButtonClick);

            saveButton.Location = new Point(10 + buttonShift, buttonLocationY);
            saveButton.Size = buttonSize;
            saveButton.Text = "Сохранить";
            saveButton.MouseClick += new MouseEventHandler(SaveButtonClick);

            createButton.Location = new Point(10 + buttonShift * 2, buttonLocationY);
            createButton.Size = buttonSize;
            createButton.Text = "Создать";
            createButton.MouseClick += new MouseEventHandler(CreateButtonClick);

            startButton.Location = new Point(10 + buttonShift * 3, buttonLocationY);
            startButton.Size = buttonSize;
            startButton.Text = "Начать";
            startButton.MouseClick += new MouseEventHandler(StartButtonClick);

            savedMapsButton.Location = new Point(230, 100);
            savedMapsButton.Size = buttonSize;
            savedMapsButton.Text = "Загрузить";
            savedMapsButton.MouseClick += new MouseEventHandler(SavedMapChoiceButtonClick);

            xTrackBarLabel.Location = new Point(10, labelLocationY);
            xTrackBarLabel.Size = labelSize;
            xTrackBarLabel.Text = String.Format("X: {0}", x);

            xTrackBar.Location = new Point(xTrackBarLabel.Location.X + barShift, labelLocationY);
            xTrackBar.Minimum = 10;
            xTrackBar.Maximum = 100;
            xTrackBar.TickFrequency = 10;
            xTrackBar.Scroll += SizeTrackBarScroll;

            yTrackBarLabel.Location = new Point(xTrackBarLabel.Location.X + labelShift, labelLocationY);
            yTrackBarLabel.Size = labelSize;
            yTrackBarLabel.Text = String.Format("Y: {0}", y);

            yTrackBar.Location = new Point(yTrackBarLabel.Location.X + barShift, labelLocationY);
            yTrackBar.Minimum = 10;
            yTrackBar.Maximum = 100;
            yTrackBar.TickFrequency = 10;
            yTrackBar.Scroll += SizeTrackBarScroll;

            robotNumBarLabel.Location = new Point(yTrackBarLabel.Location.X + labelShift, labelLocationY);
            robotNumBarLabel.Size = bigLabelSize;
            robotNumBarLabel.Text = String.Format("Количество {0}\nроботов:", robotNum);

            robotNumBar.Location = new Point(robotNumBarLabel.Location.X +robotNumBarLabel.Size.Width, labelLocationY);
            robotNumBar.Minimum = 1;
            robotNumBar.Maximum = 15;
            robotNumBar.TickFrequency = 1;
            robotNumBar.Scroll += RobotNumTrackBarScroll;

            robotNumTextBox.Location = new Point(robotNumBarLabel.Location.X, robotNumBarLabel.Location.Y + robotNumBarLabel.Size.Height + 5);
            robotNumTextBox.Size = textBoxSize;

            resultLabel.Location = new Point(loadButton.Location.X, robotNumTextBox.Location.Y);
            resultLabel.Size = bigLabelSize;

            savedMaps.CheckOnClick = true;
            savedMaps.SelectionMode = SelectionMode.One;
            savedMaps.Location = new Point(10, 100);
            savedMaps.Size = new Size(200, 200);

            globalMapPictureBox.MouseMove += new MouseEventHandler(GlobalMapMouseMove);
            globalMapPictureBox.MouseUp += new MouseEventHandler(GlobalMapMouseUp);
            globalMapPictureBox.MouseDown += new MouseEventHandler(GlobalMapMouseDown);
            globalMapPictureBox.Location = new Point(10, 130);

            emptyRectPen.Alignment = PenAlignment.Inset; //закрашивание внутри контура
        }

        void InterfaceDraw()
        {
            Controls.Add(loadButton);
            Controls.Add(saveButton);
            Controls.Add(createButton);
            Controls.Add(xTrackBarLabel);
            Controls.Add(xTrackBar);
            Controls.Add(yTrackBarLabel);
            Controls.Add(yTrackBar);
            Controls.Add(robotNumBarLabel);
            Controls.Add(robotNumBar);
            Controls.Add(startButton);
            Controls.Add(globalMapPictureBox);
            Controls.Add(robotNumTextBox);
            Controls.Add(resultLabel);
        } //отрисовка элементов интерфейса

        void SizeTrackBarScroll(object sender, EventArgs e)
		{
			x = xTrackBar.Value;
			y = yTrackBar.Value;
			xTrackBarLabel.Text = String.Format ("X: {0}", x);
			yTrackBarLabel.Text = String.Format ("Y: {0}", y);
            EmptyEverything();
        } //сдвиг ползунка

        void RobotNumTrackBarScroll(object sender, EventArgs e)
        {
            robotNum = robotNumBar.Value;
            robotNumBarLabel.Text = String.Format("Количество {0}\nроботов:", robotNum);
            //robotNumTextBox.Text = "";
        }

        void LoadButtonClick(object sender, EventArgs e)
		{
            Controls.Remove(globalMapPictureBox);
            Controls.Remove(robotMapPictureBox);
            Controls.Remove(resultLabel);

            string[] filesNames = Directory.GetFiles(@"C:\MARS maps");

            savedMaps.Items.AddRange(filesNames);
            Controls.Add(savedMaps);

            Controls.Add(savedMapsButton);

        } //нажание на кнопку загрузки

        void SavedMapChoiceButtonClick(object sender, EventArgs e) //нажатие на кнопку выбора сохранённой карты
        {
            var file = File.ReadAllLines((string)savedMaps.SelectedItem);
            mapArray = new int[file.Length, file[0].Length / 2];
            robotMapArray = new int[file.Length, file[0].Length / 2];

            y = file.Length;
            x = file[0].Length / 2;
            SetFormMaps();

            for (int i = 0; i < file.Length; i++)
            {
                var temp = file[i].Split(' ');
                for (int j = 0; j < temp.Length - 1; j++) //последний элемент строки - символ окончания строки
                {
                    mapArray[i, j] = int.Parse(temp[j]);
                    if (mapArray[i, j] == 2)
                    {
                        startPoint = (i, j);
                    }
                }
            }

            EmptyRobotMap();
            DrawGlobalMap();
            DrawRobotMap();
            savedMaps.Items.Clear();
            Controls.Remove(savedMaps);
            Controls.Remove(savedMapsButton);
            Controls.Add(globalMapPictureBox);
            Controls.Add(robotMapPictureBox);
            Controls.Add(resultLabel);
            xTrackBar.Value = x;
            yTrackBar.Value = y;
            xTrackBarLabel.Text = String.Format("X: {0}", x);
            yTrackBarLabel.Text = String.Format("Y: {0}", y);
            //yTrackBar.Enabled = false;
            //xTrackBar.Enabled = false;
        }

        void SaveButtonClick(object sender, EventArgs e)
		{
            string fileName = System.IO.Path.Combine(@"c:\MARS maps", System.IO.Path.GetRandomFileName());

            using (StreamWriter map = new StreamWriter(fileName + ".txt", true, System.Text.Encoding.Default))
            {
                for (int i = 0; i < mapArray.GetLength(0); i++)
                {
                    for (int j = 0; j < mapArray.GetLength(1); j++)
                        map.Write(mapArray[i, j] + " ");
                    map.WriteLine();
                }
            }
            
        } //нажание на кнопку сохранения

        void StartButtonClick(object sender, EventArgs e)
        {
            if (robotNumTextBox.Text == "") //первый режим
            {
                robotsCoordinate = new ConcurrentQueue<List<(int x, int y)>>();
                EmptyRobotMap();
                if (prevCoordList != null)
                    prevCoordList.Clear();
                os = new Thread(StepOS);
                os.Start();
                map = new Thread(DrawingRobotsMap);
                map.Start();
            }
            else //второй режим
            {
                //sleepTime = 0;
                RobotNumStringParse();
                stepsArray = new int[robotNums.Count()];
                distancesArray = new List<int>[robotNums.Count()];
                for (int i = 0; i < robotNums.Count(); i++)
                {
                    robotNum = robotNums[i];
                    os = new Thread(StartOS);
                    os.Start();
                    while (os.IsAlive)
                    {

                    }
                    stepsArray[i] = steps;
                    distancesArray[i] = distances;
                    Console.WriteLine(distances.Count());
                }
                StepChartDraw();
            }
        }

        void StepChartDraw()
        {
            Form ChartForm = new Form();
            ChartForm.Size = new Size(1000, 800);
            ChartForm.Text = "Статистика";
            ChartForm.Show();

            Chart stepChart = new Chart();
            stepChart.Location = new Point(0, 0);
            stepChart.Size = chartSize;

            ChartArea stepChartArea = new ChartArea();
            stepChart.ChartAreas.Add(stepChartArea);

            Series stepChartSeries = new Series();
            stepChartSeries.Name = "Step chart";
            stepChartSeries.ChartType = SeriesChartType.Column;
            stepChartSeries.Points.DataBindXY(robotNums, stepsArray);
            stepChartSeries.IsValueShownAsLabel = true;
            stepChart.Series.Add(stepChartSeries);
            ChartForm.Controls.Add(stepChart);
            stepChart.Invalidate();

            /*Chart optimalPointChart = new Chart();
            optimalPointChart.Location = new Point(chartSize.Width + 10, 0);
            optimalPointChart.Size = chartSize;

            ChartArea optimalPointChartArea = new ChartArea();
            optimalPointChart.ChartAreas.Add(optimalPointChartArea);
            Series ratioSeries1 = new Series();
            ratioSeries1.ChartType = SeriesChartType.FastLine;
            ratioSeries1.Points.DataBindXY(robotNums, stepsArray);
            optimalPointChart.Series.Add(ratioSeries1);
            Series ratioSeries2 = new Series();
            ratioSeries2.ChartType = SeriesChartType.FastLine;
            for (int i = 0; i < robotNums.Count(); i++)
            {
                ratioSeries2.Points.AddXY(robotNums[i], stepsArray[i] / robotNums[i]);
            }
            optimalPointChart.Series.Add(ratioSeries2);
            ChartForm.Controls.Add(optimalPointChart);
            optimalPointChart.Invalidate();*/

            Chart[] distancesCharts = new Chart[distancesArray.Count()];
            for (int i = 0; i < distancesCharts.Count(); i++)
            {
                distancesCharts[i] = new Chart();
                distancesCharts[i].Location = new Point((i<3?i:i-3)*(smallChartSize.Width) + chartSize.Width, i < 3 ? 0 : smallChartSize.Height);
                distancesCharts[i].Size = smallChartSize;

                ChartArea distanceArea = new ChartArea();
                distanceArea.AxisX.Minimum = 0;
                distanceArea.AxisX.Maximum = robotNums.Max() + 1;
                distancesCharts[i].ChartAreas.Add(distanceArea);


                int[] robotSeries = new int[robotNums[i]];
                for (int j = 0; j < robotNums[i]; j++)
                {
                    robotSeries[j] = j + 1;
                }
                Series distanceSeries = new Series();
                distanceSeries.ChartType = SeriesChartType.Column;
                distanceSeries.IsValueShownAsLabel = true;
                distanceSeries.Points.DataBindXY(robotSeries, distancesArray[i]);
                distancesCharts[i].Series.Add(distanceSeries);
                ChartForm.Controls.Add(distancesCharts[i]);
                distancesCharts[i].Invalidate();
            }

            /*Chart distanceCharts = new Chart();
            distanceCharts.Location = new Point(chartSize.Width + chartShift, 0);
            ChartArea[] distanceChartsAreas = new ChartArea[robotNums.Count()];
            Series[] distanceChartsSeries = new Series[robotNums.Count()];
            for (int i = 0; i < robotNums.Count(); i++)
            {
                distanceChartsAreas[i] = new ChartArea();

                distanceChartsSeries[i] = new Series();
                distanceChartsSeries[i].ChartType = SeriesChartType.Column;
                int[] robotSeries = new int[robotNums[i]];
                for (int j = 0; j < robotNums[i]; j++)
                {
                    robotSeries[j] = j+1;
                }
                distanceChartsSeries[i].Points.DataBindXY(robotSeries, distancesArray[i]);

                distanceCharts.ChartAreas.Add(distanceChartsAreas[i]);
                distanceCharts.Series.Add(distanceChartsSeries[i]);
            }
            ChartForm.Controls.Add(distanceCharts);
            distanceCharts.Invalidate();*/
        }

		void CreateButtonClick(object sender, EventArgs e)
		{
            EmptyEverything();
        } //нажатие на кнопку создания карты

        void EmptyEverything()
        {
            mapArray = new int[y, x];
            robotMapArray = new int[y, x];
            startPointIsSet = false;
            if (prevCoordList != null)
                prevCoordList.Clear();
            SetFormMaps();
            EmptyGlobalMap();
            EmptyRobotMap();
            DrawGlobalMap();
            DrawRobotMap();
        }

        /*void CreateMap()
        {
            this.Size = new Size(x * sideSize * 2 + 100 < buttonShift * 5 ? buttonShift * 5 : x * sideSize * 2 + 100, y * sideSize + 200);
            globalMapPictureBox.Size = new Size(x * sideSize + 1, y * sideSize + 1);
            globalMap = new Bitmap(x * sideSize + 1, y * sideSize + 1);
            robotMapPictureBox.Size = new Size(x * sideSize + 1, y * sideSize + 1);
            robotMapPictureBox.Location = new Point(globalMapPictureBox.Location.X + x * sideSize + sideSize, globalMapPictureBox.Location.Y);
            robotMap = new Bitmap(x * sideSize + 1, y * sideSize + 1);
            Controls.Add(robotMapPictureBox);
            globalMapGraphics = Graphics.FromImage(globalMap);
            robotMapGraphics = Graphics.FromImage(robotMap);

            for (int i = 0; i < y; i++) //заполнение массивов карты
            {
                for (int j = 0; j < x; j++)
                {
                    mapArray[i, j] = 0;
                    robotMapArray[i, j] = 2;
                }
            }
            DrawMap();
        }*/

        void SetFormMaps()
        {
            this.Size = new Size(x * sideSize * 2 + formXBorderShift < buttonShift * buttonNum ? buttonShift * buttonNum : x * sideSize * 2 + formXBorderShift, y * sideSize + formYBorderShift);
            globalMapPictureBox.Size = new Size(x * sideSize + 1, y * sideSize + 1);
            globalMap = new Bitmap(x * sideSize + 1, y * sideSize + 1);
            robotMapPictureBox.Size = new Size(x * sideSize + 1, y * sideSize + 1);
            robotMapPictureBox.Location = new Point(globalMapPictureBox.Location.X + x * sideSize + sideSize, globalMapPictureBox.Location.Y);
            robotMap = new Bitmap(x * sideSize + 1, y * sideSize + 1);
            Controls.Add(robotMapPictureBox);
            globalMapGraphics = Graphics.FromImage(globalMap);
            robotMapGraphics = Graphics.FromImage(robotMap);
        }

        void EmptyGlobalMap()
        {
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    mapArray[i, j] = 0;
                }
            }
        }

        void EmptyRobotMap()
        {
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    robotMapArray[i, j] = 2;
                }
            }
        }

        void RobotMapArrayUpdate(List <(int x, int y)> coordList)
        {
            //ConsoleOutput();
            if (prevCoordList != null)
            {
                for (int l = 0; l < prevCoordList.Count(); l++)
                {
                    robotMapArray[prevCoordList[l].x, prevCoordList[l].y] = 1;
                }
            }

            for (int l = 0; l < coordList.Count(); l++)
            {
                for (int n = -1; n < 2; n++)
                {
                    for (int m = -1; m < 2; m++)
                    {
                        int i = coordList[l].x + n;
                        int j = coordList[l].y + m;

                        if (i < 0 || i == y || j < 0 || j == x)
                            continue;
                        if (n== 0 && m == 0)
                            robotMapArray[i, j] = 3;
                        else if (robotMapArray[i, j] != 3)
                            robotMapArray[i, j] = mapArray[i, j];
                    }
                }
            }
            prevCoordList = coordList;
        } //обновление карты новыми данными от роботов

        /*void DrawMap()
        {
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    if (mapArray[i, j] == 0)
                        globalMapGraphics.FillRectangle(takenRectBrush, j * (sideSize), i * (sideSize), sideSize + 1, sideSize + 1);
                    if (mapArray[i, j] == 1)
                        globalMapGraphics.FillRectangle(emptyRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    if (mapArray[i, j] == 2)
                        globalMapGraphics.FillRectangle(startPointRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    globalMapGraphics.DrawRectangle(emptyRectPen, j * (sideSize), i * (sideSize), sideSize, sideSize);

                    if (robotMapArray[i, j] == 0)
                        robotMapGraphics.FillRectangle(takenRectBrush, j * (sideSize), i * (sideSize), sideSize + 1, sideSize + 1);
                    if (robotMapArray[i, j] == 1)
                        robotMapGraphics.FillRectangle(emptyRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    if (robotMapArray[i, j] == 2)
                        robotMapGraphics.FillRectangle(unknownRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    if (robotMapArray[i, j] == 3)
                        robotMapGraphics.FillRectangle(robotRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    if (mapArray[i, j] == 2)
                        robotMapGraphics.FillRectangle(startPointRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    robotMapGraphics.DrawRectangle(emptyRectPen, j * (sideSize), i * (sideSize), sideSize, sideSize);
                }
            }

            GlobalMapInForm(globalMap);
            RobotMapInForm(robotMap);
        } //отрисовка карты*/

        void DrawGlobalMap()
        {
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    if (mapArray[i, j] == 0)
                        globalMapGraphics.FillRectangle(takenRectBrush, j * (sideSize), i * (sideSize), sideSize + 1, sideSize + 1);
                    if (mapArray[i, j] == 1)
                        globalMapGraphics.FillRectangle(emptyRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    if (mapArray[i, j] == 2)
                        globalMapGraphics.FillRectangle(startPointRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    globalMapGraphics.DrawRectangle(emptyRectPen, j * (sideSize), i * (sideSize), sideSize, sideSize);
                }
            }
            GlobalMapInForm(globalMap);
        }

        void DrawRobotMap()
        {
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    if (robotMapArray[i, j] == 0)
                        robotMapGraphics.FillRectangle(takenRectBrush, j * (sideSize), i * (sideSize), sideSize + 1, sideSize + 1);
                    if (robotMapArray[i, j] == 1)
                        robotMapGraphics.FillRectangle(emptyRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    if (robotMapArray[i, j] == 2)
                        robotMapGraphics.FillRectangle(unknownRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    if (robotMapArray[i, j] == 3)
                        robotMapGraphics.FillRectangle(robotRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    if (mapArray[i, j] == 2)
                        robotMapGraphics.FillRectangle(startPointRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    robotMapGraphics.DrawRectangle(emptyRectPen, j * (sideSize), i * (sideSize), sideSize, sideSize);
                }
            }
            RobotMapInForm(robotMap);
        }

        void RobotNumStringParse()
        {
            string[] robotNumsStrings = robotNumTextBox.Text.Split(' ');
            robotNums = new int[robotNumsStrings.Count()];
            for (int i = 0; i < robotNumsStrings.Count(); i++)
            {
                robotNums[i] = int.Parse(robotNumsStrings[i]);
            }
        }

        private void GlobalMapMouseMove(object sender, MouseEventArgs e)
		{
			if (draw)
			{
				int squareX = e.X / sideSize;
				int squareY = e.Y / sideSize;
                if (squareX < x && squareY < y && squareX >= 0 && squareY >= 0)
                {
                    if (!rightMouseButton)
                    {
                        if (!startPointIsSet)
                        {
                            mapArray[squareY, squareX] = 2;
                            globalMapGraphics.FillRectangle(startPointRectBrush, squareX * sideSize, squareY * sideSize, sideSize + 1, sideSize + 1);
                            globalMapGraphics.DrawRectangle(emptyRectPen, squareX * (sideSize), squareY * (sideSize), sideSize, sideSize);
                            startPoint = (squareY, squareX);
                            startPointIsSet = !startPointIsSet;
                        }
                        else if (mapArray[squareY, squareX] != 2)
                        {
                            mapArray[squareY, squareX] = 1;
                            globalMapGraphics.FillRectangle(emptyRectBrush, squareX * sideSize, squareY * sideSize, sideSize + 1, sideSize + 1);
                            globalMapGraphics.DrawRectangle(emptyRectPen, squareX * (sideSize), squareY * (sideSize), sideSize, sideSize);
                        }
                    }
                    else
                    {
                        mapArray[squareY, squareX] = 0;
                        globalMapGraphics.FillRectangle(takenRectBrush, squareX * sideSize, squareY * sideSize, sideSize + 1, sideSize + 1);
                        globalMapGraphics.DrawRectangle(emptyRectPen, squareX * (sideSize), squareY * (sideSize), sideSize, sideSize);
                    }

                    GlobalMapInForm(globalMap);
                }
			}
		}

		private void GlobalMapMouseUp(object sender, MouseEventArgs e)
		{
			draw = false;
		}

        private void GlobalMapMouseDown(object sender, MouseEventArgs e)
		{
			draw = true;
            if (e.Button == MouseButtons.Right)
            {
                rightMouseButton = true;
            }
            else
            {
                rightMouseButton = false;
            }
		}

        private void StartOS()
        {
            OpSystem OS = new OpSystem(robotNum, mapArray, startPoint);
            steps = OS.Start(out List<int> dist);
            distances = dist;
            os.Abort();
        }

        private void StepOS()
        {
            OpSystem OS = new OpSystem(robotNum, mapArray, startPoint);
            bool end = true;
            while (end)
            {
                robotsCoordinate.Enqueue(OS.CalculationStep(out end));
                Thread.Sleep(sleepTime);
            }
            List<(int, int)> temp = new List<(int, int)>();
            robotsCoordinate.Enqueue(temp); // отправляем пустой список, в качестве индикатора об окончании работы алгоритма
            os.Abort();
        }

        private void DrawingRobotsMap()
        {
            steps = -1;
            while (true)
            {
                if (!robotsCoordinate.TryDequeue(out var coorList)) continue; // проверка очереди
                if (!coorList.Any()) break; // проверка на окончание передачи
                RobotMapArrayUpdate(coorList);
                DrawRobotMap();
                steps++;
            }
            ResultOutput(String.Format("Роботов: {0} Тактов: {1}", robotNum, steps));
            map.Abort();
        }

        public void ResultOutput(string s)
        {
            if (resultLabel.InvokeRequired)
            {
                resultLabelDelegate tB = new resultLabelDelegate(ResultOutput);
                resultLabel.Invoke(tB, new object[] { s });
            }
            else
            {
                resultLabel.Text = s;
            }
        }

        private void RobotMapInForm(Bitmap bmp)
        {
            if (robotMapPictureBox.InvokeRequired)
            {
                RobotMap a = new RobotMap(RobotMapInForm);
                robotMapPictureBox.Invoke(a, new object[] { bmp });
            }
            else
            {
                robotMapPictureBox.Image = bmp;
            }
        }

        private void GlobalMapInForm(Bitmap bmp)
        {
            if (robotMapPictureBox.InvokeRequired)
            {
                RobotMap a = new RobotMap(GlobalMapInForm);
                globalMapPictureBox.Invoke(a, new object[] { bmp });
            }
            else
            {
                globalMapPictureBox.Image = bmp;
            }
        }

        /*void ConsoleDebugOutput(string s)
        {
            if (s.Equals("coordinates"))
            {
                Console.WriteLine("Robot coordinates");
                for (int i = 0; i < robots.Count(); i++)
                {
                    Console.WriteLine("{0}: {1} {2}", i, robots[i].GetCoordinates(true).yCoord, robots[i].GetCoordinates(true).xCoord);
                }
            }
            else if (s.Equals("global map"))
            {
                Console.WriteLine("Global map\n");

                for (int i = 0; i < y; i++)
                {
                    for (int j = 0; j < x; j++)
                    {
                        Console.Write(mapArray[i, j]);
                    }
                    Console.WriteLine();
                }
            }
            else if (s.Equals("Robot map"))
            {
                Console.WriteLine("Robot map\n");
                for (int i = 0; i < y; i++)
                {
                    for (int j = 0; j < x; j++)
                    {
                        Console.Write(robotMapArray[i, j]);
                    }
                    Console.WriteLine();
                }
            }
        }*/

        private void Form1_Load(object sender, EventArgs e)
        {

        }


    }
}
