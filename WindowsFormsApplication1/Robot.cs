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
        List<int> path;

        public Robot()
        {
            path = new List<int>();
            coordinates.x = 0;
            coordinates.y = 0;
            status = true;
        }
    }
}
