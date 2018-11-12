using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    class Robot
    {

        public bool status;                         // 1-свободен 0-занят
        public int distance;
        public (int x, int y) coordinates;
        public (int x, int y) prevCoordinates;
        public List<string> path;
        public Dictionary<string, int> ratingTasks; // task / dist

        public Robot((int x, int y) poit)
        {
            path = new List<string>();
            coordinates.x = poit.x;
            coordinates.y = poit.y;
            prevCoordinates.x = poit.x;
            prevCoordinates.y = poit.y;
            status = true;
            distance = 0;

            ratingTasks = new Dictionary<string, int>();
        }


        public (int xCoord, int yCoord) GetCoordinates(bool type) //0-prev 1-current
        {
            if (type)
                return coordinates;
            else
                return prevCoordinates;
        }

        public string MaxRatingTask
        {
            get { return ratingTasks.First(s => s.Value == ratingTasks.Values.Min()).Key; }
        }
    }
}
