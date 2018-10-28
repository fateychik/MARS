using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    class OpSystem
    {
        Queue<(int x, int y)> tasks; // те задача обозначается через координаты цели
        Robot[] robots;
        int[,] map; // массив, содержащий карту
        Graph graph;
        List<int> free;
        List<int> busy;

        public OpSystem(int robotsNumber, int[,] map)
        {
            robots = new Robot[robotsNumber];
            tasks = new Queue<(int x, int y)>();
            graph = new Graph();
            free = new List<int>();
            busy = new List<int>();
        }

        private void MapBuild()
        {
            foreach (Robot robot in robots)
            {
                (int x, int y) robotCoordinates = robot.GetCoordinates(true);

                int[] robotSurroundings = robot.GetSurroundings();

                int a = 0;
                for (int i = robotCoordinates.y - 1; i <= robotCoordinates.y + 1; i++)
                {
                    for (int j = robotCoordinates.x - 1; j <= robotCoordinates.x + 1; j++)
                    {
                        map[i, j] = robotSurroundings[a++];
                    }
                }

                (int x, int y) robotPrevCoordinates = robot.GetCoordinates(false);
                
            }
        }
    }
}