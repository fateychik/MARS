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
            DrawMap();
        }

        int x, y; //значения размерности
		bool draw = false;
        //test line to see how GIT works

        //отрисовка элементов карты
        int sideSize = 10; //размер стороны квадрата
        static int lineWidth = 1; //ширина линии квадрата
        int borderShift = 0; //сдвиг от границы окна
        PictureBox globalMapPictureBox = new PictureBox();
        Bitmap globalMap;
        Graphics globalMapGraphics;
        Pen emptyRectPen = new Pen(Color.Gray, lineWidth); //линия пустой клетки
        Pen takenRectPen = new Pen(Color.Black, lineWidth); //линия клетки-припятствия
        //Pen pathRectPen = new Pen(Color.Red, lineWidth); //линия клетки-маршрута
        SolidBrush BlackRectBrush = new SolidBrush(Color.Black); //зарисовка занятой клетки
        //SolidBrush RedRectBrush = new SolidBrush(Color.Red); //зарисовка клетки маршрута
        //


        // начало отрисовки интерфейса и карты
        void DrawMap()
        {
			x = 100;
			y = 100;
			globalMapPictureBox.MouseMove += new MouseEventHandler(GlobalMapMouseMove);
			globalMapPictureBox.MouseUp += new MouseEventHandler(GlobalMapMouseUp);
			globalMapPictureBox.MouseDown += new MouseEventHandler(GlobalMapMouseDown);


            emptyRectPen.Alignment = PenAlignment.Inset;
            takenRectPen.Alignment = PenAlignment.Inset;
            this.Size = new Size(x * sideSize + 30, y * sideSize + 30); //окно программы
            globalMapPictureBox.Size = new Size(x * sideSize + 20, y * sideSize + 20); //окно карты
			Controls.Add(globalMapPictureBox);
            globalMap = new Bitmap(x * sideSize + 20, y * sideSize + 20);
			globalMapGraphics = Graphics.FromImage(globalMap);
            for (int i = 0; i < y; i++) //отрисовка пустой карты
            {
                for (int j = 0; j < x; j++)
                {
					globalMapGraphics.DrawRectangle(emptyRectPen, j * (sideSize) + borderShift, i * (sideSize) + borderShift, sideSize, sideSize);
				}
            }
			globalMapGraphics.FillRectangle (BlackRectBrush, 0,0,1,1);

			globalMapPictureBox.Image = globalMap;

			//globalMapPictureBox.MouseClick (ClickOnMap);
        } //отрисовка карты
        //конец отрисовки интерфейса и карты

		private void ClickOnMap(object sender, MouseEventArgs e)
		{
			int squareX = e.X / sideSize;
			int squareY = e.Y / sideSize;
			globalMapGraphics.FillRectangle (BlackRectBrush, squareX*sideSize, squareY*sideSize, sideSize, sideSize);
			globalMapPictureBox.Image = globalMap;
			Console.Write (e.X);
			Console.Write (" ");
			Console.WriteLine (e.Y);
			Console.Write (squareX*sideSize);
			Console.Write (" ");
			Console.WriteLine (squareY*sideSize);
		}

	

		private void GlobalMapMouseMove(object sender, MouseEventArgs e)
		{
			if (draw)
			{
				int squareX = e.X / sideSize;
				int squareY = e.Y / sideSize;
				globalMapGraphics.FillRectangle (BlackRectBrush, squareX*sideSize, squareY*sideSize, sideSize, sideSize);
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
