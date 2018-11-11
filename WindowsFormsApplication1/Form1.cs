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

namespace WindowsFormsApplication1
{
	
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
            InitializeComponents();
            InterfaceDraw();
        }

        OpSystem OS;
        Robot[] robots;

        int x = 10; //значения размерности
        int y = 10;
		bool draw = false;
        bool rightMouseButton = false;
		int[,] mapArray;
        int[,] robotMapArray;
        int robotNum = 1;

		int sideSize = 10; //размер стороны квадрата
		static int lineWidth = 1; //ширина линии квадрата

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
		TrackBar xTrackBar = new TrackBar ();
		TrackBar yTrackBar = new TrackBar ();
        TrackBar robotNumBar = new TrackBar ();

        CheckedListBox savedMaps = new CheckedListBox();

        Size buttonSize = new Size (100, 25);
        int buttonShift = 115;
        int buttonLocationY = 10;
        Size labelSize = new Size(40, 40);
        int labelShift = 160;
        int labelLocationY = 50;
        int barShift = 40;

        Pen emptyRectPen = new Pen(Color.Gray, lineWidth); //линия пустой клетки
		SolidBrush takenRectBrush = new SolidBrush(Color.Black); //зарисовка занятой клетки
        SolidBrush emptyRectBrush = new SolidBrush(Color.LightGray);
        SolidBrush unknownRectBrush = new SolidBrush(Color.LightBlue);
        SolidBrush robotRectBrush = new SolidBrush(Color.Red);

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
            xTrackBar.Scroll += TrackBarScroll;

            yTrackBarLabel.Location = new Point(xTrackBarLabel.Location.X + labelShift, labelLocationY);
            yTrackBarLabel.Size = labelSize;
            yTrackBarLabel.Text = String.Format("Y: {0}", y);

            yTrackBar.Location = new Point(yTrackBarLabel.Location.X + barShift, labelLocationY);
            yTrackBar.Minimum = 10;
            yTrackBar.Maximum = 100;
            yTrackBar.TickFrequency = 10;
            yTrackBar.Scroll += TrackBarScroll;

            robotNumBarLabel.Location = new Point(yTrackBarLabel.Location.X + labelShift, labelLocationY);
            robotNumBarLabel.Size = new Size(labelSize.Width * 2 + 10, labelSize.Height);
            robotNumBarLabel.Text = String.Format("Количество {0}\nроботов:", robotNum);

            robotNumBar.Location = new Point(robotNumBarLabel.Location.X +robotNumBarLabel.Size.Width, labelLocationY);
            robotNumBar.Minimum = 1;
            robotNumBar.Maximum = 15;
            robotNumBar.TickFrequency = 1;
            robotNumBar.Scroll += TrackBarScroll;

            savedMaps.CheckOnClick = true;
            savedMaps.SelectionMode = SelectionMode.One;
            savedMaps.Location = new Point(10, 100);
            savedMaps.Size = new Size(200, 200);

            globalMapPictureBox.MouseMove += new MouseEventHandler(GlobalMapMouseMove);
            globalMapPictureBox.MouseUp += new MouseEventHandler(GlobalMapMouseUp);
            globalMapPictureBox.MouseDown += new MouseEventHandler(GlobalMapMouseDown);
            globalMapPictureBox.Location = new Point(10, 100);

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
        } //отрисовка элементов интерфейса

        void TrackBarScroll(object sender, EventArgs e)
		{
			x = xTrackBar.Value;
			y = yTrackBar.Value;
            robotNum = robotNumBar.Value;
			xTrackBarLabel.Text = String.Format ("X: {0}", x);
			yTrackBarLabel.Text = String.Format ("Y: {0}", y);
            robotNumBarLabel.Text = String.Format("Количество {0}\nроботов:", robotNum);
        } //сдвиг ползунка

		void LoadButtonClick(object sender, EventArgs e)
		{
            Controls.Remove(globalMapPictureBox);

            string[] filesNames = Directory.GetFiles(@"c:\MARS maps");

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
            CreateMap();

            for (int i = 0; i < file.Length; i++)
            {
                var temp = file[i].Split(' ');
                for (int j = 0; j < temp.Length - 1; j++) //последний элемент строки - символ окончания строки
                    mapArray[i, j] = int.Parse(temp[j]);
            }

            DrawMap();
            savedMaps.Items.Clear();
            Controls.Remove(savedMaps);
            Controls.Remove(savedMapsButton);
            Controls.Add(globalMapPictureBox);
            xTrackBar.Value = x;
            yTrackBar.Value = y;
            xTrackBarLabel.Text = String.Format("X: {0}", x);
            yTrackBarLabel.Text = String.Format("Y: {0}", y);
            yTrackBar.Enabled = false;
            xTrackBar.Enabled = false;
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
            OS = new OpSystem(robotNum, mapArray, (0, 1));
            robots = OS.Start();
            RobotMapArrayUpdate();
            DrawMap();
            startButton.Text = "Шаг";
            startButton.MouseClick -= StartButtonClick;
            startButton.MouseClick += new MouseEventHandler(StepButtonClick);
        }

        void StepButtonClick(object sender, EventArgs e)
        {
            robots = OS.Start();
            RobotMapArrayUpdate();
            DrawMap();
        }

		void CreateButtonClick(object sender, EventArgs e)
		{
			mapArray = new int[y, x];
            robotMapArray = new int[y, x];
            CreateMap();
/*<<<<<<< HEAD
		} //нажатие на кнопку создания карты

		void CreateMap()
		{
			this.Size = new Size(x * sideSize + 1000, y * sideSize + 150); //окно программы

			globalMapPictureBox.Size = new Size(x * sideSize+1, y * sideSize+1); //окно карты

			globalMap = new Bitmap(x * sideSize+1, y * sideSize+1);

=======*/
        } //нажатие на кнопку создания карты

        void CreateMap()
        {
            this.Size = new Size(x * sideSize * 2 + 100 < buttonShift * 5 ? buttonShift * 5 : x * sideSize * 2 + 100, y * sideSize + 150);
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
        }

        void RobotMapArrayUpdate()
        {
            //ConsoleOutput();
            for (int l = 0; l < robots.Count(); l++)
            {
                robotMapArray[robots[l].GetCoordinates(false).xCoord, robots[l].GetCoordinates(false).yCoord] = 1;
            }

            for (int l = 0; l < robots.Count(); l++)
            {
                for (int n = -1; n < 2; n++)
                {
                    for (int m = -1; m < 2; m++)
                    {
                        int i = robots[l].GetCoordinates(true).xCoord + n;
                        int j = robots[l].GetCoordinates(true).yCoord + m;
                        if (i < 0 || i == y || j < 0 || j == x)
                            continue;
                        if (n== 0 && m == 0)
                            robotMapArray[i, j] = 3;
                        else if (robotMapArray[i, j] != 3)
                            robotMapArray[i, j] = mapArray[i, j];
                    }
                }
            }

        }

        void ConsoleOutput()
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


        void DrawMap()
        {
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    if (mapArray[i, j] == 0)
                        globalMapGraphics.FillRectangle(takenRectBrush, j * (sideSize), i * (sideSize), sideSize + 1, sideSize + 1);
                    else
                        globalMapGraphics.FillRectangle(emptyRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    globalMapGraphics.DrawRectangle(emptyRectPen, j * (sideSize), i * (sideSize), sideSize, sideSize);

                    if (robotMapArray[i, j] == 0)
                        robotMapGraphics.FillRectangle(takenRectBrush, j * (sideSize), i * (sideSize), sideSize + 1, sideSize + 1);
                    if (robotMapArray[i, j] == 1)
                        robotMapGraphics.FillRectangle(emptyRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    if (robotMapArray[i, j] == 2)
                        robotMapGraphics.FillRectangle(unknownRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);
                    if (robotMapArray[i, j] == 3)
                        robotMapGraphics.FillRectangle(robotRectBrush, j * sideSize, i * sideSize, sideSize + 1, sideSize + 1);

                    robotMapGraphics.DrawRectangle(emptyRectPen, j * (sideSize), i * (sideSize), sideSize, sideSize);
                    

                }
            }

            globalMapPictureBox.Image = globalMap;
            robotMapPictureBox.Image = robotMap;
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
                        mapArray[squareY, squareX] = 1;
//<<<<<<< HEAD

//=======
//>>>>>>> origin/master
                        globalMapGraphics.FillRectangle(emptyRectBrush, squareX * sideSize, squareY * sideSize, sideSize + 1, sideSize + 1);
                        globalMapGraphics.DrawRectangle(emptyRectPen, squareX * (sideSize), squareY * (sideSize), sideSize, sideSize);
                    }
                    else
                    {
                        mapArray[squareY, squareX] = 0;
//<<<<<<< HEAD

//=======
//>>>>>>> origin/master
                        globalMapGraphics.FillRectangle(takenRectBrush, squareX * sideSize, squareY * sideSize, sideSize + 1, sideSize + 1);
                        globalMapGraphics.DrawRectangle(emptyRectPen, squareX * (sideSize), squareY * (sideSize), sideSize, sideSize);
                    }

                    globalMapPictureBox.Image = globalMap;
                }
			}
		}

		private void GlobalMapMouseUp(object sender, MouseEventArgs e)
		{
			draw = false;
		}

        private void Form1_Load(object sender, EventArgs e)
        {

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
	}
}
