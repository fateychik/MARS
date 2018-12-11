using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    class OpSystem
    {
        List<(int x, int y)> tasks;  // те задача обозначается через координаты цели
        Robot[] robots;
        int[,] fullMap;              // массив, содержащий полную карту пользователя(для локации роботов)
        int[,] robotMap;             // массив, содержащий карту от роботов
        Graph graph;                 // добавления вершин, как строки graph_.AddVertex($"{i}_{j}");
        List<int> free;              // список свободных роботов
        List<int> busy;              // список занятых роботов
        int busyCount = 0;           // 

        Dictionary<int, string> sorted;

        #region Graph

        List<(string name, int robNum)> discovered;
        List<(string name, int robNum)> visited;
        List<(string name, int robNum)> newDiscovered;
        List<string[]> edges;

        System.Diagnostics.Process process;
        string programmExecute = "\"C:\\Program Files (x86)\\Graphviz2.38\\bin\\dot\"";
        string programmArguments = "-Tpng \"C:\\MARS maps\\graph.txt\" -o\"C:\\MARS maps\\graph.png\"";
        string graphFile = @"C:\MARS maps\graph.txt";
        List<string> fileText;

        private void GraphFileStart(string startPoint)
        {
            string startPointString = String.Format("\"{0}\" [label=\"{0}\\nStart\\npoint\"];", startPoint, "}");
            string[] startLines = { "digraph MapGraph {", startPointString };
            fileText.Add(@"digraph MapGraph {");
            fileText.Add(startPointString);
            //System.IO.File.WriteAllLines(graphFile, startLines);
        }

        private void GraphFileUpdate()
        {
            //var lines = System.IO.File.ReadAllLines(graphFile);
            //System.IO.File.WriteAllLines(graphFile, lines.Take(lines.Length - 1).ToArray());

            for (int i = 0; i < edges.Count; i++)
            {
            //file.WriteLine("\"{0}\"->\"{1}\";", edges[i][0], edges[i][1]);
                fileText.Add(String.Format("\"{0}\"->\"{1}\";", edges[i][0], edges[i][1]));
            }

            for (int i = 0; i < newDiscovered.Count; i++)
            {
            //file.WriteLine("\"{0}\" [label=\"{0}\\nfound: {1}\\ndiscovered\"];", newDiscovered[i].name, newDiscovered[i].robNum);
                fileText.Add(String.Format("\"{0}\" [label=\"{0}\\nfound: {1}\\ndiscovered\"];", newDiscovered[i].name, newDiscovered[i].robNum));
            }

            for (int i = 0; i < visited.Count; i++)
            {
                //file.WriteLine("\"{0}\" [label=\"{0}\\nfound: {1}\\ndiscovered: {2}\"];", visited[i].name, discovered.Find(item => item.name == visited[i].name).robNum, visited[i].robNum);
                fileText.Add(String.Format("\"{0}\" [label=\"{0}\\nfound: {1}\\ndiscovered: {2}\"];", visited[i].name, discovered.Find(item => item.name == visited[i].name).robNum, visited[i].robNum));
            }

            //file.WriteLine("}");
            System.IO.File.WriteAllLines(graphFile, fileText);
            System.IO.File.AppendAllText(graphFile, "}");
            /*System.IO.File.Create(graphFile);
            System.IO.File.
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(graphFile, true))
            {

                for (int i = 0; i < fileText.Count; i++)
                {
                    file.WriteLine(fileText[i]);
                }
                file.WriteLine("}");
            }*/
            process.Start();
            process.WaitForExit();
        }
        #endregion

        public OpSystem(int robotsNumber, int[,] fullMap, (int x, int y) point)
        {
            robots = new Robot[robotsNumber];
            for (int i = 0; i < robots.Length; i++)
                robots[i] = new Robot(point);

            this.fullMap = fullMap;
            robotMap = new int[fullMap.GetLength(0), fullMap.GetLength(1)];
            tasks = new List<(int x, int y)>();
            graph = new Graph();
            discovered = new List<(string name, int robNum)>();
            graph.AddVertex($"{point.x}_{point.y}");
            //GraphFileStart($"{point.x}_{point.y}");
            free = new List<int>();
            for (int i = 1; i < robotsNumber; i++)  // объяевение всех роботов, как свободных
                free.Add(i);

            busy = new List<int>();
            busy.Add(0);                            // для того, чтобы начать первый цикл работы

            sorted = new Dictionary<int, string>(); // hz

            fileText = new List<string>();

            GraphFileStart($"{point.x}_{point.y}");

            process = new System.Diagnostics.Process();
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = programmExecute;
            process.StartInfo.Arguments = programmArguments;
        }

        public List<(int, int)> CalculationStep(out bool end)
        {
            TerritoryInvestigation();
            GiveTasks();
            UpdateGiveTasks();
            CoordIncrement();
            GraphFileUpdate();

            List<(int, int)> temp = new List<(int, int)>();
            foreach (var i in robots)
                temp.Add(i.coordinates);

            end = busy.Any() || tasks.Any();
            return temp;
        }

        public int Start(out List<int> distance)
        {
            int counter = 0;
            while (busy.Any() || tasks.Any())
            {
                TerritoryInvestigation();
                GiveTasks();
                UpdateGiveTasks();
                CoordIncrement();
                counter++;
            }

            distance = new List<int>();
            foreach (var i in robots)
                distance.Add(i.distance);

            return counter;
        }

        private void TerritoryInvestigation()                                                 //дополнение карты окружением роботов
        {
            var buf = new List<int>();

            visited = new List<(string name, int robNum)>();
            edges = new List<string[]>();
            newDiscovered = new List<(string name, int robNum)>();

            foreach (int num in busy)
            {
                if (robots[num].path.Any()) continue;                                         // опрашиваем только граничных роботов

                (int x, int y) coordinates = robots[num].GetCoordinates(true);

                visited.Add(($"{coordinates.x}_{coordinates.y}", num));                       //обновление описания точки инфомацией о роботе, посетившим её


                for (int n = -1; n < 2; n++)
                {
                    for (int m = - 1; m < 2; m++)
                    {
                        if ((n + m) % 2 == 0) continue;
                        if (coordinates.x + n < 0 || coordinates.y + m < 0) continue;
                        if (coordinates.x + n >= fullMap.GetLength(0) || coordinates.y + m >= fullMap.GetLength(1)) continue;

                        int i = coordinates.x + n <= 0 ? 0 : coordinates.x + n;
                        int j = coordinates.y + m <= 0 ? 0 : coordinates.y + m;

                        if ((i, j) != robots[num].GetCoordinates(false) && fullMap[i, j] != 0)

                        {
                            if (robotMap[i, j] == 1)                                          // если мы обнаружили ранее найденную вершину, к которой нет связи
                            {
                                graph.AddEdge($"{i}_{j}", $"{coordinates.x}_{coordinates.y}", 1);
                                if (tasks.Contains((i, j))) tasks.Remove((i, j));
                                continue;
                            }

                            robotMap[i, j] = fullMap[i, j];                                   // отрисовываем карту
                            graph.AddVertex($"{i}_{j}");                                      // добавление вершины в граф лабиринта
                            graph.AddEdge($"{i}_{j}", $"{coordinates.x}_{coordinates.y}", 1); // добавение ребра
                            tasks.Add((i, j));                                                // добавление задачи
                            newDiscovered.Add(($"{i}_{j}", num));
                            string[] edgeCoords = {$"{coordinates.x}_{coordinates.y}", $"{i}_{j}"};
                            edges.Add(edgeCoords);
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

            discovered.AddRange(newDiscovered);
        }
                
        private void GiveTasks()
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

        private void UpdateGiveTasks()
        {
            var newBusyCount = busy.Count();
            if (busyCount > newBusyCount)
            {
                foreach (var i in busy)
                {
                    var temp = robots[i].path.Last().Split('_');
                    tasks.Add((int.Parse(temp[0]), int.Parse(temp[1])));
                }
                busy.Clear();
                free.Clear();

                for (int i = 0; i < robots.Length; i++)  // объяевение всех роботов, как свободных
                    free.Add(i);

                GiveTasks();
            }
            busyCount = newBusyCount;
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

                    if (robots[temp].ratingTasks[robots[temp].MaxRatingTask] != int.MaxValue)
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
                robots[i].distance++;
            }
        }
    }
}