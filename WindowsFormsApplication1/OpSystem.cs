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
        int[,] fullMap;              // массив, содержащий полную карту пользователя(для локации роботов)
        int[,] robotMap;             // массив, содержащий карту от роботов
        Graph graph;                 // добавления вершин, как строки graph_.AddVertex($"{i}_{j}");
        List<int> free;              // список свободных роботов
        List<int> busy;              // список занятых роботов

        public OpSystem(int robotsNumber, int[,] fullMap)
        {
            robots = new Robot[robotsNumber];
            this.fullMap = fullMap;
            robotMap = new int[fullMap.GetLength(0), fullMap.GetLength(1)];
            tasks = new Queue<(int x, int y)>();
            graph = new Graph();
            free = new List<int>();
            busy = new List<int>();
        }

        public void GiveTask()
        {
            int robot = int.MaxValue;
            int bufDistance = int.MaxValue;
            List<string> path = new List<string>();

            while (tasks.Any() || free.Any())
            {
                var task = tasks.Dequeue();
                var goal = $"{task.x}_{task.y}";

                foreach (int i in free)
                {
                    var start = $"{robots[i].coordinates.x}_{robots[i].coordinates.y}";
                    
                    path = graph.WideWidthSearch(start, goal, out float distance);
                    robot = distance < bufDistance ? i : robot;
                }

                robots[robot].path = path;
                free.Remove(robot);
                busy.Add(robot);
                robots[robot].status = false;
            }
        }

        public void CoordIncrement()
        {
            foreach (int i in busy)
            {
                var path = robots[i].path[0].Split('_');
                robots[i].coordinates = (int.Parse(path[0]), int.Parse(path[1]));
                robots[i].path.RemoveAt(0);
            }
        }
    }
}
