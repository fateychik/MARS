using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    class Robot
    {

        public bool status; // 1-свободен 0-занят
        public (int x, int y) coordinates;
        (int x, int y) prevCoordinates;
        public List<string> path;

        public Robot()
        {
            path = new List<string>();
            coordinates.x = 0;
            coordinates.y = 0;
            prevCoordinates.x = prevCoordinates.y = 0;
            status = true;
        }


        public (int xCoord, int yCoord) GetCoordinates(bool type) //0-prev 1-current
        {
            if (type)
                return coordinates;
            else
                return prevCoordinates;
        }

    }
}
