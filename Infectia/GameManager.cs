using System;
using System.Collections.Generic;
using System.Text;

namespace Infectia
{

    // The Game Logic, that handles math, health and infection.
    // Sounds complicated, but is actually very simple.
    class GameManager
    {
        public int health = 100;
        public bool DEAD = false;
        public int healersLeft;
        public int totalHealers;
        public int infAreaUL = 0;
        public int playArea;
        public int infAreaRD;

        // Constructor to get max size and healer Count.
        public GameManager(int h, int play)
        {
            totalHealers = h;
            healersLeft = h;
            playArea = play;
            infAreaRD = play;
        }

        // inflicts 20% damage
        public void infect()
        {
            health -= 20;
            if (health <= 0)
            {
                DEAD = true;
            }
        }
        // 'heals' by 10%
        public void heal()
        {
            health += 10;
            if (health >= 90)
            {
                health = 90;
            }
        }

        // checks if the player collided with an infeciton site.
        public char checkInfection(int X, int Y)
        {
            if (X >= infAreaRD - 1)
            {
                infect();
                return 'L';

            }
            else if (X <= infAreaUL)
            {
                infect();
                return 'R';
            }
            else if (Y >= infAreaRD - 1)
            {

                infect();
                return 'U';
            }
            else if (Y <= infAreaUL)
            {
                infect();
                return 'D';
            }
            else
            {
                return '-';
            }

        }

        // if a healer is picked up, the infection sprads by one layer, meaning the play area us reduced by one layer.
        // you need to pick up healers before the infection kills you.
        public Healer[] pickupHealer(int X, int Y, Healer[] helarr, int max)
        {
            for (int i = 0; i < max; i++)
            {
                if (helarr[i].X == X && helarr[i].Y == Y)
                {
                    helarr[i].X = -1;
                    helarr[i].Y = -1;
                    healersLeft--;
                    infAreaRD--;
                    infAreaUL++;
                    if (health < 100)
                    {
                        heal();
                    }
                }
            }
            return helarr;
        }
    }
}
