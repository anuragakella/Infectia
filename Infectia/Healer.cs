using System;
using System.Collections.Generic;
using System.Text;

namespace Infectia
{
    // Spawns a healer randomly on the matrix. the draw method checks for healers while rendering and draws them accordingly.
    class Healer
    {
        public int X;
        public int Y;
        public int max, min;
        Random rand = new Random();
        public Healer(int mn, int mx)
        {
            max = mx;
            min = mn;
            spawn();
        }
        public void spawn()
        {
            X = rand.Next(min, max);
            Y = rand.Next(min, max);
        }
    }
}
