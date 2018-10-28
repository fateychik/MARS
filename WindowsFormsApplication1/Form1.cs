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

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            DrawMap();
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
        Pen emptyRectPen = new Pen(Color.Gray, lineWidth); //линия пустой клетки
        Pen takenRectPen = new Pen(Color.Black, lineWidth); //линия клетки-припятствия
        SolidBrush BlackRectBrush = new SolidBrush(Color.Black); //зарисовка занятой клетки

        // начало отрисовки интерфейса и карты
        void DrawMap()
        {
			x = 50;
			y = 50;
            globalMapPictureBox.MouseClick += new MouseEventHandler(GlobalMapMouseClick);
            globalMapPictureBox.MouseMove += new MouseEventHandler(GlobalMapMouseMove);
			globalMapPictureBox.MouseUp += new MouseEventHandler(GlobalMapMouseUp);
			globalMapPictureBox.MouseDown += new MouseEventHandler(GlobalMapMouseDown);


            emptyRectPen.Alignment = PenAlignment.Inset;
            takenRectPen.Alignment = PenAlignment.Inset;
            this.Size = new Size(x * sideSize + 30, y * sideSize + 50); //окно программы
            globalMapPictureBox.Size = new Size(x * sideSize + 1, y * sideSize + 1); //окно карты
			Controls.Add(globalMapPictureBox);
            globalMap = new Bitmap(x * sideSize + 1, y * sideSize + 1);
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

        }//отрисовка карты
        //конец отрисовки интерфейса и карты

		private void GlobalMapMouseClick(object sender, MouseEventArgs e)
		{
			int squareX = e.X / sideSize;
			int squareY = e.Y / sideSize;
			globalMapGraphics.FillRectangle (BlackRectBrush, squareX*sideSize, squareY*sideSize, sideSize, sideSize);
			globalMapPictureBox.Image = globalMap;

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
