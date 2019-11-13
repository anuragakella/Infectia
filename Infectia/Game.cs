using System;
using System.Collections.Generic;
using System.Text;

namespace Infectia
{
    class Game
    {
        // All the variables needed for the game to run.
        //the play area itself, will be rendered using ASCII chars using Console.Write()
        public char[,] gameMatrix;
        // the size of gameMatrix ^;
        public int playArea = 10;
        // the next 2 vars track the infection area, they refer to the 'layer' number [the index]
        // changing this would 'technically' shrink the play area, which is what we want to do, to inflict damage.
        public int infectedAreaRD;
        public int infectedAreaLU = 0;
        // default player cursor, an asterisk
        public char defaultCursor = '*';
        // used for rendering the 'nodes' of the game matrix
        public char backgroundChars = '`';
        // a function checks for edges and stores them in this bool array. prevents an out of bound exception.
        public Boolean[] allowedDirections;
        // var to check if the game needs to render the main menu.
        public bool mainmenu = true;
        // max healers per game
        public static int healerCount = 5;
        // total no of healers collected
        public int healersOwned = 0;
        // vars for disabling directions for the edges
        public int UP = 0;
        public int DOWN = 1;
        public int LEFT = 2;
        public int RIGHT = 3;
        // vars used to SetCursorPosision
        public int drawY;
        public int drawX;
        //colors
        ConsoleColor BLACK = ConsoleColor.Black;
        ConsoleColor DARK_MAGENTA = ConsoleColor.DarkMagenta;
        ConsoleColor DARK_RED = ConsoleColor.DarkRed;
        ConsoleColor WHITE = ConsoleColor.White;
        ConsoleColor CYAN = ConsoleColor.Cyan;
        ConsoleColor DARK_GRAY = ConsoleColor.DarkGray;
        // current position of the player.
        public int currentX, currentY;
        // healer objecs, will be rendered into the game when created.
        Healer[] healers = new Healer[healerCount];
        // The Game manager tracks health, checks the spread of the 'infection', checks healers [in short, manages the game logic, quite simple.]
        GameManager gameManager;
        //the main game loop

        // Initializes the Game, creates Healers, Attaches a GameManager, and generates a blank matrix for rendering.
        public void initGame()
        {
            infectedAreaRD = playArea;
            drawY = (Console.WindowWidth) / 2 - (playArea);
            drawX = Console.WindowHeight / 2 - (playArea / 2);
            generateHealers(healerCount);
            gameManager = new GameManager(healerCount, playArea);
            for (int i = 0; i < playArea; i++)
            {
                for (int j = 0; j < playArea; j++)
                {
                    gameMatrix[i, j] = '\'';
                }
            }
            gameMatrix[currentY, currentX] = defaultCursor;
            allowedDirections = checkBounds(currentY, currentX);
            draw(gameMatrix, currentY, currentX);
            gameMatrix[currentY, currentX] = '\'';
            infectedAreaRD = playArea;
            infectedAreaLU = 0;
        }

        // this function stores an array of healers in the healers array, which will be accessed later for rendering and game logic.
        public void generateHealers(int max)
        {
            for (int i = 0; i < max; i++)
            {
                Healer tempHealer = new Healer(3, playArea - 3);
                healers[i] = tempHealer;
            }
        }

        //the main game loop. To avoid flickers and lag, it only re-renders/ draws when theres an update, oterwise the game does nothing. This probably is a bad approach and will need to fix it later,
        // to efficiently re-render without depending on I/O.
        public void run()
        {
            char infectDir;
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                allowedDirections = checkBounds(currentY, currentX);
                // checking if the player died, if dead, show a game over menu, if not, continue.
                if (gameManager.DEAD == true)
                {
                    mainmenu = true;
                    Console.Clear();
                    Console.SetCursorPosition((Console.WindowWidth / 2) - ("YOU DIED".Length / 2), (Console.WindowHeight / 2) - 2);
                    setTextColor(DARK_MAGENTA);
                    Console.WriteLine("YOU DIED");
                    Console.SetCursorPosition((Console.WindowWidth / 2) - ("[press enter to quit to the main menu]".Length / 2), (Console.WindowHeight / 2) - 1);
                    Console.WriteLine("[press enter to quit to the main menu]");
                    setTextColor(DARK_GRAY);
                    ConsoleKey KEY2 = Console.ReadKey(true).Key;
                    if (KEY2 == ConsoleKey.Enter)
                    {
                        Console.Clear();
                        mainMenu();
                    }
                }
                mainmenu = false;
                // checks for infection and inflicts damage if the player touches the infection.
                infectDir = gameManager.checkInfection(currentX, currentY);
                // infectDir shows where the player collided with the infection layer and 'bounces' the player one step away.
                switch (infectDir)
                {
                    case 'U':
                        currentY--;
                        break;
                    case 'D':
                        currentY++;
                        break;
                    case 'L':
                        currentX--;
                        break;
                    case 'R':
                        currentX++;
                        break;
                    default:
                        break;
                }
                // renders after infeciton.
                gameMatrix[currentY, currentX] = defaultCursor;
                draw(gameMatrix, currentY, currentX);
                gameMatrix[currentY, currentX] = '\'';
                // if NOT DEAD or DONE, it reads key input.
                if (gameManager.DEAD == false && gameManager.healersLeft != 0)
                {
                    ConsoleKeyInfo KEY = Console.ReadKey(true);
                    // all of the key combos to move the char, each key adds one to the current pos,the boolean array allowedDirections checks for edges and 
                    // prevents exceptions.
                    if ((KEY.Key == ConsoleKey.RightArrow || KEY.Key == ConsoleKey.D) && allowedDirections[RIGHT])

                    {
                        if ((KEY.Modifiers & ConsoleModifiers.Shift) != 0)
                        {
                            currentX += 2;
                            if (currentX >= playArea)
                            {
                                currentX -= 3;
                            }
                        }
                        else
                        {
                            currentX++;
                        }
                        gameMatrix[currentY, currentX] = defaultCursor;
                        draw(gameMatrix, currentY, currentX);
                        gameMatrix[currentY, currentX] = '\'';
                    }
                    if ((KEY.Key == ConsoleKey.LeftArrow || KEY.Key == ConsoleKey.A) && allowedDirections[LEFT])
                    {
                        if ((KEY.Modifiers & ConsoleModifiers.Shift) != 0)
                        {
                            currentX -= 2;
                            if (currentX <= 0)
                            {
                                currentX += 3;
                            }
                        }
                        else
                        {
                            currentX--;
                        }
                        gameMatrix[currentY, currentX] = defaultCursor;
                        draw(gameMatrix, currentY, currentX);
                        gameMatrix[currentY, currentX] = '\'';

                    }
                    if ((KEY.Key == ConsoleKey.UpArrow || KEY.Key == ConsoleKey.W) && allowedDirections[UP])
                    {
                        if ((KEY.Modifiers & ConsoleModifiers.Shift) != 0)
                        {
                            currentY -= 2;
                            if (currentY <= 0)
                            {
                                currentY += 3;
                            }
                        }
                        else
                        {
                            currentY--;
                        }
                        gameMatrix[currentY, currentX] = defaultCursor;
                        draw(gameMatrix, currentY, currentX);
                        gameMatrix[currentY, currentX] = '\'';

                    }
                    if ((KEY.Key == ConsoleKey.DownArrow || KEY.Key == ConsoleKey.S) && allowedDirections[DOWN])
                    {
                        if ((KEY.Modifiers & ConsoleModifiers.Shift) != 0)
                        {
                            currentY += 2;
                            if (currentY >= playArea)
                            {
                                currentY -= 3;
                            }
                        }
                        else
                        {
                            currentY++;
                        }
                        gameMatrix[currentY, currentX] = defaultCursor;
                        draw(gameMatrix, currentY, currentX);
                        gameMatrix[currentY, currentX] = '\'';
                    }
                    if (KEY.Key == ConsoleKey.P)
                    {
                        // if a healer is picked upm it's placed outside the grid at -1, -1 and the healersOwned count is incrementd.
                        // placing them outside the grid makes them un-renderable, when the draw method is called.
                        gameManager.pickupHealer(currentX, currentY, healers, healerCount);
                        if (gameManager.healersLeft == 0)
                        {
                            mainmenu = true;
                            Console.Clear();
                            Console.SetCursorPosition((Console.WindowWidth / 2) - ("YOU SAVED THEM ALL!".Length / 2), (Console.WindowHeight / 2) - 2);
                            setTextColor(DARK_MAGENTA);
                            Console.WriteLine("YOU SAVED THEM ALL!");
                            Console.SetCursorPosition((Console.WindowWidth / 2) - ("[press enter to quit to the main menu]".Length / 2), (Console.WindowHeight / 2) - 1);
                            Console.WriteLine("[press enter to quit to the main menu]");
                            setTextColor(DARK_GRAY);
                            ConsoleKey KEY2 = Console.ReadKey(true).Key;
                            if (KEY2 == ConsoleKey.Enter)
                            {
                                Console.Clear();
                                mainMenu();
                            }
                        }

                    }
                    if (KEY.Key == ConsoleKey.Q)
                    {
                        // Quit menu, ask for confirmation.
                        // for N, it redrawst the current state,
                        // for Y it returns to the main menu and re-initializes the vars and the game.
                        // if you press anything other than Y, it will return to the current state and resume the game.
                        mainmenu = true;
                        Console.Clear();
                        Console.SetCursorPosition((Console.WindowWidth / 2) - ("Are you sure about that(Y/N)?".Length / 2), (Console.WindowHeight / 2) - 2);
                        setTextColor(DARK_MAGENTA);
                        Console.WriteLine("Are you sure about that(Y/N)?");
                        setTextColor(DARK_GRAY);
                        ConsoleKey KEY2 = Console.ReadKey(true).Key;
                        if (KEY2 == ConsoleKey.Y)
                        {
                            Console.Clear();
                            mainMenu();
                        }
                        else if (KEY2 == ConsoleKey.N)
                        {
                            Console.Clear();
                            draw(gameMatrix, currentY, currentX);
                            continue;
                        }
                        else
                        {
                            Console.Clear();
                            draw(gameMatrix, currentY, currentX);
                            continue;
                        }
                    }
                }
            }
        }

        // a samll function used to reset the whole row on the console.
        // sometimes, a sentence/ string can be shorter than what was drawn in it's place before. This method clears ONE line,
        // depending on where it was called.
        // a screen refresh for a line only.. prints a large string of spaces.
        public void resetLine()
        {
            Console.WriteLine("                                                                                                   ");
        }

        // the loop that handles graphics
        // checks for various game variables and draws the health, the play area, etc.
        // loops through the matrix and if the indices are greater than the infection layer, they are painted RED.
        // else cyan or white, depending on the character.
        // also checks for health, total healers and current X, Y coords and renders them onto the screen.
        public void draw(char[,] arr, int Y, int X)
        {
            Console.CursorVisible = false;
            //for (int i = 0; i < drawY; i++)
            //{
            //    Console.WriteLine();
            //}
            if (gameManager.DEAD == false && gameManager.healersLeft != 0)
            {
                for (int i = 0; i < playArea; i++)
                {
                    for (int j = 0; j < playArea; j++)
                    {
                        if (i <= gameManager.infAreaUL || j <= gameManager.infAreaUL)
                        {
                            setTextColor(DARK_RED);
                        }
                        else if (i > gameManager.infAreaRD - 2 || j > gameManager.infAreaRD - 2)
                        {
                            setTextColor(DARK_RED);
                        }
                        else
                        {
                            setTextColor(BLACK);
                        }
                        for (int k = 0; k < healerCount; k++)
                        {
                            if (healers[k].X == j && healers[k].Y == i)
                            {
                                setTextColor(WHITE);
                            }
                        }
                        if (i == Y && j == X)
                        {
                            setTextColor(CYAN);
                        }
                        int w = (Console.WindowWidth / 2) - playArea;
                        int h = (Console.WindowHeight / 2) - playArea;
                        if (w < 0) w = 0;
                        if (h < 0) h = 0;
                        Console.SetCursorPosition(w + (j + j), h + (i + 1));
                        Console.Write(arr[i, j] + " ");
                    }
                }
                setTextColor(DARK_GRAY);
                Console.SetCursorPosition(1, Console.WindowHeight - 6);
                resetLine();
                Console.SetCursorPosition(1, Console.WindowHeight - 7);
                resetLine();
                Console.SetCursorPosition(1, Console.WindowHeight - 7);
                Console.WriteLine("Health: {0}", getHealth());
                Console.SetCursorPosition(1, Console.WindowHeight - 6);
                Console.WriteLine("Healers Picked-Up: {0}", healerCount - gameManager.healersLeft);
                Console.SetCursorPosition(1, Console.WindowHeight - 5);
                Console.WriteLine("Current Coordinates: {0}, {1}", X, Y);
                Console.SetCursorPosition(1, Console.WindowHeight - 4);
                Console.WriteLine("Press 'Q' to exit to the main menu.");
            }

        }

        // this function concatenates a string representing the health bar. 
        // used a loop and a method, because of the same over-write problem that resetLine() was used to solve.
        // that doesnt work here, since health updates a lot quicker than menu items.
        public string getHealth()
        {
            string h = "";
            for (int i = 0; i < gameManager.health; i++)
            {
                if (gameManager.health <= 0) break;
                if (i % 2 == 0)
                    h += "|";
            }
            if (gameManager.health <= 0)
            {
                h += " " + 0 + "%";

            }
            else
            {
                h += " " + gameManager.health + "%";
            }
            return h;
        }

        //constructor to initialize matrix and run game
        // just takes in the initial values of the player.
        // edges can be used to initialize.. but based on game rules that would be a problem and reduce health.
        // so starting at (1, 1) or the center or any other place seems 'logical'
        public Game(int X, int Y)
        {
            currentX = X;
            currentY = Y;
        }


        // This method disaplya the main menu, with the ASCII art with multple WriteLines.
        // has multiple while loops to manage the initial settings, like choosing grids, cursors and showing the rules.
        public void mainMenu()
        {
            char[] cursorChoice = { '*', '^', '>', '<' };
            int[] gridChoice = { 20, 30 };
            int currenCur = 0;
            int currentGrid = 0;
            setTextColor(DARK_MAGENTA);
            Console.SetCursorPosition(30, 0);
            Console.WriteLine("::::::::::: ::::    ::: :::::::::: ::::::::::  ::::::::  ::::::::::: :::::::::::     :::           ");
            Console.SetCursorPosition(30, 1);
            Console.WriteLine("    :+:     :+:+:   :+: :+:        :+:        :+:    :+:     :+:         :+:       :+: :+:         ");
            Console.SetCursorPosition(30, 2);
            Console.WriteLine("    +:+     :+:+:+  +:+ +:+        +:+        +:+            +:+         +:+      +:+   +:+        ");
            Console.SetCursorPosition(30, 3);
            Console.WriteLine("    +#+     +#+ +:+ +#+ :#::+::#   +#++:++#   +#+            +#+         +#+     +#++:++#++:       ");
            Console.SetCursorPosition(30, 4);
            Console.WriteLine("    +#+     +#+  +#+#+# +#+        +#+        +#+            +#+         +#+     +#+     +#+       ");
            Console.SetCursorPosition(30, 5);
            Console.WriteLine("    #+#     #+#   #+#+# #+#        #+#        #+#    #+#     #+#         #+#     #+#     #+#       ");
            Console.SetCursorPosition(30, 6);
            Console.WriteLine("########### ###    #### ###        ##########  ########      ###     ########### ###     ###       ");
            Console.SetCursorPosition(30, 7);
            Console.WriteLine();
            Console.SetCursorPosition(30, 8);
            Console.WriteLine("Infectia v1.1.0");
            setTextColor(WHITE);
            bool print = true;
            Console.CursorVisible = false;
            string strt = "Press Enter to Start";
            // loop for blinking the start sign, like many retro games do.
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter))
            {

                if (print == true)
                {
                    Console.SetCursorPosition(Console.WindowWidth / 2 - (strt.Length / 2) - 1, 12);
                    setTextColor(WHITE);
                    Console.WriteLine(strt);
                }
                else
                {
                    setTextColor(BLACK);
                    Console.SetCursorPosition(Console.WindowWidth / 2 - (strt.Length / 2) - 1, 12);
                    Console.WriteLine(strt);
                }
                System.Threading.Thread.Sleep(750);
                print = !print;
            }
            Console.SetCursorPosition(Console.WindowWidth / 2 - (strt.Length / 2) - 1, 12);
            resetLine();
            Console.SetCursorPosition(Console.WindowWidth / 2, 10);
            // loop for player seletion
            while (true)
            {
                Console.SetCursorPosition(30, 9);
                setTextColor(DARK_GRAY);
                Console.WriteLine("Select a cursor(<-, -> arrow keys to move selection, enter to confirm): ");
                setTextColor(DARK_GRAY);
                Console.SetCursorPosition(30, 10);
                Console.CursorVisible = false;
                for (int i = 0; i < cursorChoice.Length; i++)
                {
                    if (currenCur == i)
                    {
                        setTextColor(DARK_MAGENTA);
                    }
                    else
                    {
                        setTextColor(DARK_GRAY);
                    }
                    Console.Write(cursorChoice[i] + " ");
                }
                ConsoleKey KEY = Console.ReadKey(true).Key;
                if ((KEY == ConsoleKey.RightArrow || KEY == ConsoleKey.D) && currenCur != 3)
                {
                    currenCur++;
                }
                if ((KEY == ConsoleKey.LeftArrow || KEY == ConsoleKey.A) && currenCur != 0)
                {
                    currenCur--;
                }
                if ((KEY == ConsoleKey.Enter))
                {
                    defaultCursor = cursorChoice[currenCur];
                    break;
                }
            }
            // loop for grid size.
            // smaller grid kills you faster,
            // larger grid - would take you a lot of time to navigate through it.
            while (true)
            {
                Console.SetCursorPosition(30, 9);
                setTextColor(DARK_GRAY);
                Console.WriteLine("Select a Grid Size(<-, -> arrow keys to move selection, enter to confirm): ");
                setTextColor(DARK_GRAY);
                Console.SetCursorPosition(30, 10);
                resetLine();
                Console.SetCursorPosition(30, 10);
                Console.CursorVisible = false;
                for (int i = 0; i < gridChoice.Length; i++)
                {
                    if (currentGrid == i)
                    {
                        setTextColor(DARK_MAGENTA);
                    }
                    else
                    {
                        setTextColor(DARK_GRAY);
                    }
                    Console.Write(gridChoice[i] + " ");
                }
                ConsoleKey KEY = Console.ReadKey(true).Key;
                if ((KEY == ConsoleKey.RightArrow || KEY == ConsoleKey.D) && currentGrid != gridChoice.Length - 1)
                {
                    currentGrid++;
                }
                if ((KEY == ConsoleKey.LeftArrow || KEY == ConsoleKey.A) && currentGrid != 0)
                {
                    currentGrid--;
                }
                if ((KEY == ConsoleKey.Enter))
                {
                    playArea = gridChoice[currentGrid];
                    break;
                }
            }
            // Basic Rules.
            Console.SetCursorPosition(30, 9);
            setTextColor(DARK_GRAY);
            resetLine();
            Console.SetCursorPosition(30, 9);
            Console.WriteLine("Rules: ");
            Console.SetCursorPosition(30, 10);
            Console.WriteLine("1. You control the blue/ cyan player.");
            Console.SetCursorPosition(30, 11);
            Console.WriteLine("2. The White 'dots' are called healers. ");
            Console.SetCursorPosition(30, 12);
            Console.WriteLine("3. The red area (which will expand during gameplay) is the infected area.");
            Console.SetCursorPosition(30, 13); Console.WriteLine("   If you go into or collide with it, you lose 20 % of your health.");
            Console.SetCursorPosition(30, 14);
            Console.WriteLine("4. Your objective is to save all the healers from the infection, which in turn, makes you invincible.");
            Console.SetCursorPosition(30, 15);
            Console.WriteLine("5. Picking up a healer will add 10% to your health, but it will also spread the infection by 1 'layer' ");
            Console.SetCursorPosition(30, 16);
            Console.WriteLine("   GOOD LUCK!");
            Console.SetCursorPosition(30, 18);
            Console.WriteLine("   Press Enter to start ->");
            Console.SetCursorPosition(30, 19);
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)) { }
            Console.Clear();
            gameMatrix = new char[playArea, playArea];
            mainmenu = false;
            initGame();
        }
        // the method that changes colors.
        public void setTextColor(ConsoleColor color)
        {
            Console.ForegroundColor = color;
        }
        // method that checks player position to prevent an outofbounds exception.
        public Boolean[] checkBounds(int Y, int X)
        {
            Boolean[] d = { true, true, true, true };
            if (Y == 0) d[UP] = false;
            if (Y == playArea - 1) d[DOWN] = false;
            if (X == 0) d[LEFT] = false;
            if (X == playArea - 1) d[RIGHT] = false;
            return d;
        }
        // Main mathod to create a game object and run it.
        // create and run the game.
        public static void Main(string[] args)
        {
            Console.SetWindowSize(150, 40);
            Game game = new Game(1, 1);
            if (game.mainmenu == true)
            {
                game.mainMenu();
            }
            game.initGame();
            game.run();
        }
    }
}
