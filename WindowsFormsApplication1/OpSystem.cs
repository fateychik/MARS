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

        private void MapBuild() //дополнение карты окружением роботов
        {
            int robotNum = 0;
            foreach (Robot robot in robots)
            {
                (int x, int y) robotCoordinates = robot.GetCoordinates(true);

                for (int i = robotCoordinates.y - 1; i <= robotCoordinates.y + 1; i++)
                {
                    for (int j = robotCoordinates.x - 1; j <= robotCoordinates.x + 1; j++)
                    {
                        robotMap[i, j] = fullMap[i,j];
                    }
                }

                List<(int x, int y)> freePathList = new List<(int x, int y)>();

                if (robotMap[robotCoordinates.y, robotCoordinates.x - 1] == 0)
                    freePathList.Add((robotCoordinates.x-1, robotCoordinates.y));
                if (robotMap[robotCoordinates.y - 1, robotCoordinates.x] == 0)
                    freePathList.Add((robotCoordinates.x, robotCoordinates.y-1));
                if (robotMap[robotCoordinates.y, robotCoordinates.x + 1] == 0)
                    freePathList.Add((robotCoordinates.x + 1, robotCoordinates.y));
                if (robotMap[robotCoordinates.y + 1, robotCoordinates.x] == 0)
                    freePathList.Add((robotCoordinates.x, robotCoordinates.y+1));

                if (freePathList.Count == 1) // достиг конца коридора
                {
                    robot.status = true;
                    free.Add(robotNum);
                    busy.Remove(robotNum);
                }

                else
                {
                    robot.path.Add($"{freePathList[0].x}_{freePathList[0].y}");
                    freePathList.RemoveAt(0);
                    while (freePathList.Any())
                    {
                        tasks.Enqueue(freePathList[0]);
                        freePathList.RemoveAt(0);
                    }
                }

                robotNum++;
            }
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