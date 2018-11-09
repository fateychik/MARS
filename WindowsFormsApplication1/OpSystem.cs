using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    class OpSystem
    {
        List<(int x, int y)> tasks; // те задача обозначается через координаты цели
        Robot[] robots;
        int[,] fullMap;              // массив, содержащий полную карту пользователя(для локации роботов)
        int[,] robotMap;             // массив, содержащий карту от роботов
        Graph graph;                 // добавления вершин, как строки graph_.AddVertex($"{i}_{j}");
        List<int> free;              // список свободных роботов
        List<int> busy;              // список занятых роботов

        Dictionary<int, string> sorted;

        public OpSystem(int robotsNumber, int[,] fullMap, (int x, int y) point)
        {
            robots = new Robot[robotsNumber];
            for (int i = 0; i < robots.Length; i++)
                robots[i] = new Robot(point);

            this.fullMap = fullMap;
            robotMap = new int[fullMap.GetLength(0), fullMap.GetLength(1)];
            tasks = new List<(int x, int y)>();
            graph = new Graph();
            graph.AddVertex($"{point.x}_{point.y}");
            free = new List<int>();
            for (int i = 1; i < robotsNumber; i++)  // объяевение всех роботов, как свободных
                free.Add(i);

            busy = new List<int>();
            busy.Add(0);                            // для того, чтобы начать первый цикл работы

            sorted = new Dictionary<int, string>(); // hz
        }

        public int[,] Start()
        {
            //while (true)
            //{
                TerritoryInvestigation();
                GiveTask();
                CoordIncrement();
            return robotMap;
            //}
        }

        private void TerritoryInvestigation()                                                 //дополнение карты окружением роботов
        {
            var buf = new List<int>();

            foreach (int num in busy)
            {
                if (robots[num].path.Any()) continue;                                         // опрашиваем только граничных роботов

                (int x, int y) coordinates = robots[num].GetCoordinates(true);

                for (int n = -1; n < 2; n++)
                {
                    for (int m = - 1; m < 2; m++)
                    {
                        if ((n + m) % 2 == 0) continue;
                        int i = coordinates.x + n <= 0 ? 0 : coordinates.x + n;
                        int j = coordinates.y + m <= 0 ? 0 : coordinates.y + m;

                        if ((i, j) != robots[num].GetCoordinates(false) && fullMap[i, j] != 0)
                        {
                            robotMap[i, j] = fullMap[i, j];                                   // отрисовываем карту
                            graph.AddVertex($"{i}_{j}");                                      // добавление вершины в граф лабиринта
                            graph.AddEdge($"{i}_{j}", $"{coordinates.x}_{coordinates.y}", 1); // добавение ребра
                            tasks.Add((i, j));                                                // добавление задачи
                        }
                    }
                }
                robots[num].status = true;
                buf.Add(num);
            }
            foreach (int i in buf)
            {
                free.Add(i);
                busy.Remove(i);
            }
        }
                
        private void GiveTask()
        {
            foreach ((int x, int y) j in tasks)
            {
                var goal = $"{j.x}_{j.y}";

                foreach (int i in free)
                { 
                    var start = $"{robots[i].coordinates.x}_{robots[i].coordinates.y}";
                    graph.WideWidthSearch(start, goal, out int distance);
                    robots[i].ratingTasks.Add(goal, distance);
                }
            }
            if (tasks.Any())
            {
                foreach (int i in free)
                {
                    Sort(i, robots[i].MaxRatingTask);
                }

                foreach (int i in free)
                {
                    robots[i].ratingTasks.Clear();
                }

                foreach (var i in sorted.Keys)
                {
                    var start = $"{robots[i].coordinates.x}_{robots[i].coordinates.y}";
                    robots[i].path = graph.WideWidthSearch(start, sorted[i], out int distance);

                    var o = sorted[i].Split('_');
                    tasks.Remove((int.Parse(o[0]), int.Parse(o[1])));
                    robots[i].status = false;
                    free.Remove(i);
                    busy.Add(i);
                }
            }
            sorted.Clear();

        }

        private void Sort(int i, string task)
        {
            if (!sorted.ContainsValue(task) && !sorted.ContainsKey(i))
            {
                sorted.Add(i, task);
            }
            else
            {
                var temp = FindRobot(task);
                if (robots[i].ratingTasks[task] >= robots[temp].ratingTasks[task])
                {
                    robots[i].ratingTasks[task] = int.MaxValue;

                    if (robots[i].ratingTasks[robots[i].MaxRatingTask] != int.MaxValue)
                        Sort(i, robots[i].MaxRatingTask);
                }
                else
                {
                    sorted[i] = task;
                    sorted.Remove(temp);
                    robots[temp].ratingTasks[task] = int.MaxValue;

                    if (robots[temp].ratingTasks[robots[i].MaxRatingTask] != int.MaxValue)
                        Sort(temp, robots[temp].MaxRatingTask);
                }
            }
        }

        private int FindRobot(string value)
        {
            int Key = 0;
            foreach (int i in sorted.Keys)
            {
                if (value == sorted[i])
                    Key = i;
            }
            return Key;
        }

        private void CoordIncrement()
        {
            foreach (int i in busy)
            {
                var path = robots[i].path[0].Split('_');
                robots[i].prevCoordinates = robots[i].coordinates;
                robots[i].coordinates = (int.Parse(path[0]), int.Parse(path[1]));
                robots[i].path.RemoveAt(0);
            }
        }
    }
}