using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    class Robot
    {
        bool status; // 1-свободен 0-занят
        (int x, int y) coordinates;
        (int x, int y) prevCoordinates;
        List<int> path;

        public Robot()
        {
            path = new List<int>();
            coordinates.x = 0;
            coordinates.y = 0;
            prevCoordinates.x = prevCoordinates.y = 0;
            status = true;
        }

        public int[] GetSurroundings(Form1 Form1) //клетки окружения робота
        {
            return Form1.Surroundings(coordinates.x, coordinates.y);
        }

        public (int xCoord, int yCoord) GetCoordinates(bool type) //0-prev 1-current
        {
            if (type)
                return coordinates;
            else
                return prevCoordinates;
        }

        internal int[] GetSurroundings()
        {
            throw new NotImplementedException();
        }

    }
}
