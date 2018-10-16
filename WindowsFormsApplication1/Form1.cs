using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace WindowsFormsApplication1
{

    class Graph
    {
        private SortedDictionary<string, int> verticies = new SortedDictionary<string, int>();

        private int[][] adjecencyMatrix = new int[0][];

        public void AddVertex(string vertex)
        {
            if (verticies.ContainsKey(vertex))
            {
                Console.WriteLine("Vertex already exists");
                return;
            }

            verticies.Add(vertex, verticies.Count);
            Array.Resize(ref adjecencyMatrix, verticies.Count);
            for (int i = 0; i < verticies.Count; i++)
            {
                Array.Resize(ref adjecencyMatrix[i], verticies.Count);
            }
            adjecencyMatrix[verticies.Count - 1] = new int[verticies.Count];
            //Console.WriteLine("Add {0} : {1}", vertex, verticies[vertex]);
        }

        public void RemoveVertex(string vertex)
        {
            if (!verticies.ContainsKey(vertex))
            {
                Console.WriteLine("No such vertex");
            }
            else
            {
                //Console.WriteLine("Remove {0}", vertex);
                for (int i = 0; i < verticies.Count; i++)
                {
                    RemoveAt(ref adjecencyMatrix[i], verticies[vertex]);
                }

                for (int i = 0; i < verticies.Count; i++)
                {
                    if (verticies.ElementAt(i).Value > verticies[vertex])
                    {
                        verticies[verticies.ElementAt(i).Key]--;
                    }
                }
                verticies.Remove(vertex);
            }
        }

        public static void RemoveAt<T>(ref T[] arr, int index)
        {
            for (int a = index; a < arr.Length - 1; a++)
            {
                arr[a] = arr[a + 1];
            }
            Array.Resize(ref arr, arr.Length - 1);
        }

        public void AddEdge(string vertex1, string vertex2, int weight = 1)
        {
            if (!verticies.ContainsKey(vertex1) || !verticies.ContainsKey(vertex2))
            {
                Console.WriteLine("Vertex doesn't exist");
                return;
            }

            adjecencyMatrix[verticies[vertex1]][verticies[vertex2]] = weight;
            adjecencyMatrix[verticies[vertex2]][verticies[vertex1]] = weight;
        }

        public void RemoveEdge(string vertex1, string vertex2)
        {
            AddEdge(vertex1, vertex2, 0);
        }

        public T GetAdjecency<T>(string vertex1, string vertex2)
        {
            if (!verticies.ContainsKey(vertex1) || !verticies.ContainsKey(vertex2))
            {
                Console.WriteLine("Vertex doesn't exist");
                return default(T);
            }

            return (T)Convert.ChangeType(adjecencyMatrix[verticies[vertex1]][verticies[vertex2]], typeof(T));
        }

        public List<string> GetNeighbours(string vertex)
        {
            if (!verticies.ContainsKey(vertex))
            {
                Console.WriteLine("Vertex doesn't exist");
                return new List<string>();
            }

            var output = new List<string>();
            var inverseDictionary = GetInverseVertexDictionary();
            for (int i = 0; i < verticies.Count; i++)
            {
                if (adjecencyMatrix[verticies[vertex]][i] != 0)
                {
                    output.Add(inverseDictionary[i]);
                }
            }
            //Console.WriteLine("Neighbours of {0}:", vertex);
            //output.ForEach(Console.WriteLine);
            //Console.WriteLine();
            return output;
        }

        public void PrintMatrix()
        {
            var inverseDictionary = GetInverseVertexDictionary();
            Console.Write(" \t");
            for (int i = 0; i < verticies.Count; i++)
            {
                Console.Write(inverseDictionary[i] + "\t");
            }
            Console.WriteLine();
            for (int i = 0; i < verticies.Count; i++)
            {
                Console.Write(inverseDictionary[i] + "\t");
                for (int j = 0; j < verticies.Count; j++)
                {
                    Console.Write("{0}\t", adjecencyMatrix[i][j]);
                }
                Console.WriteLine();
            }
        } //1=есть связь, 0=нет связи

        public void PrintVerticies()
        {
            foreach (var kvp in verticies)
            {
                Console.WriteLine(kvp);
            }
        }

        private Dictionary<int, string> GetInverseVertexDictionary()
        {
            var output = new Dictionary<int, string>();
            foreach (var kvp in verticies)
            {
                output[kvp.Value] = kvp.Key;
            }

            return output;
        }
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            GetMode();
        }

        int x, y; //значения размерности
		bool draw = false;

        //отрисовка элементов карты
        int sideSize = 10; //размер стороны квадрата
        static int lineWidth = 1; //ширина линии квадрата
        int borderShift = 0; //сдвиг от границы окна

        PictureBox globalMapPictureBox = new PictureBox();
        Bitmap globalMap;
        Graphics globalMapGraphics;

		Button saveButton = new Button ();
		Button loadButton = new Button ();
		Button createButton = new Button ();
		Label xTrackBarLabel = new Label ();
		Label yTrackBarLabel = new Label ();
		TrackBar xTrackBar = new TrackBar ();
		TrackBar yTrackBar = new TrackBar ();
		Size buttonSize = new Size (100, 25);

        Pen emptyRectPen = new Pen(Color.Gray, lineWidth); //линия пустой клетки
        Pen takenRectPen = new Pen(Color.Black, lineWidth); //линия клетки-припятствия
        SolidBrush takenRectBrush = new SolidBrush(Color.Black); //зарисовка занятой клетки
        //


		void GetMode()
		{
			this.Size = new Size(500, 350); //окно программы

			loadButton.Location = new Point (10, 10);
			loadButton.Size = buttonSize;
			loadButton.Text = "Загрузить";
			loadButton.MouseClick += new MouseEventHandler (LoadButtonClick);
			Controls.Add (loadButton);

			saveButton.Location = new Point (125, 10);
			saveButton.Size = buttonSize;
			saveButton.Text = "Сохранить";
			saveButton.MouseClick += new MouseEventHandler (SaveButtonClick);
			Controls.Add (saveButton);

			createButton.Location = new Point (240, 10);
			createButton.Size = buttonSize;
			createButton.Text = "Создать";
			createButton.MouseClick += new MouseEventHandler (CreateButtonClick);
			Controls.Add (createButton);

			xTrackBarLabel.Location = new Point (10, 50);
			xTrackBarLabel.Size = new Size(40,20);
			Controls.Add (xTrackBarLabel);

			xTrackBar.Location = new Point (50, 50);
			xTrackBar.Minimum = 10;
			xTrackBar.Maximum = 100;
			xTrackBar.TickFrequency = 10;
			xTrackBar.Scroll += TrackBarScroll;
			Controls.Add (xTrackBar);

			yTrackBarLabel.Location = new Point (170, 50);
			yTrackBarLabel.Size = new Size(40,20);
			Controls.Add (yTrackBarLabel);

			yTrackBar.Location = new Point (210, 50);
			yTrackBar.Minimum = 10;
			yTrackBar.Maximum = 100;
			yTrackBar.TickFrequency = 10;
			yTrackBar.Scroll += TrackBarScroll;
			Controls.Add (yTrackBar);
		}

		void TrackBarScroll (object sender, EventArgs e)
		{
			x = xTrackBar.Value;
			y = yTrackBar.Value;
			xTrackBarLabel.Text = String.Format ("X: {0}", xTrackBar.Value);
			yTrackBarLabel.Text = String.Format ("Y: {0}", yTrackBar.Value);
		}

		void LoadButtonClick(object sender, EventArgs e)
		{

		}

		void SaveButtonClick(object sender, EventArgs e)
		{
		
		}
	
		void CreateButtonClick(object sender, EventArgs e)
		{
			Controls.Clear();
			DrawMap ();
		}

        // начало отрисовки интерфейса и карты
        void DrawMap()
        {

			globalMapPictureBox.MouseMove += new MouseEventHandler(GlobalMapMouseMove);
			globalMapPictureBox.MouseUp += new MouseEventHandler(GlobalMapMouseUp);
			globalMapPictureBox.MouseDown += new MouseEventHandler(GlobalMapMouseDown);


            emptyRectPen.Alignment = PenAlignment.Inset;
            takenRectPen.Alignment = PenAlignment.Inset;

            this.Size = new Size(x * sideSize + 30, y * sideSize + 30); //окно программы

            globalMapPictureBox.Size = new Size(x * sideSize+1, y * sideSize+1); //окно карты
			Controls.Add(globalMapPictureBox);
            globalMap = new Bitmap(x * sideSize+1, y * sideSize+1);
			globalMapGraphics = Graphics.FromImage(globalMap);

            for (int i = 0; i < y; i++) //отрисовка пустой карты
            {
                for (int j = 0; j < x; j++)
                {
					globalMapGraphics.DrawRectangle(emptyRectPen, j * (sideSize) + borderShift, i * (sideSize) + borderShift, sideSize, sideSize);
				}
            }

			globalMapPictureBox.Image = globalMap;

        } //отрисовка карты
        //конец отрисовки интерфейса и карты

		private void GlobalMapMouseMove(object sender, MouseEventArgs e)
		{
			if (draw)
			{
				//Console.Write (e.X);
				//Console.Write ("\t");
				//Console.WriteLine (e.Y);
				int squareX = e.X / sideSize;
				int squareY = e.Y / sideSize;
				globalMapGraphics.FillRectangle (takenRectBrush, squareX*sideSize, squareY*sideSize, sideSize, sideSize);
				globalMapPictureBox.Image = globalMap;
			}
		}

		private void GlobalMapMouseUp(object sender, MouseEventArgs e)
		{
			draw = false;
		}

		private void GlobalMapMouseDown(object sender, MouseEventArgs e)
		{
			draw = true;
		}
    }
}
