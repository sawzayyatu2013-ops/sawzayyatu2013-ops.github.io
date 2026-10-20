qusing System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static int width;
    static int height;
    static int score = 0;
    static int tick = 0;

    static List<(int x, int y)> snake = new List<(int x, int y)>();
    static int dirX = 1, dirY = 0; 
    
    static int foodX, foodY;
    static Random rng = new Random();

    // ANSI Colors
    const string RESET = "\u001b[0m";
    const string WALL_COLOR = "\u001b[34m";     // Blue
    const string SNAKE_HEAD = "\u001b[92m";     // Bright Green
    const string SNAKE_BODY = "\u001b[32m";     // Dark Green
    const string EYE_COLOR = "\u001b[97m";      // White
    const string TONGUE_COLOR = "\u001b[91m";   // Red
    const string FOOD_COLOR = "\u001b[91m";     // Red

    static void Main()
    {
        Console.CursorVisible = false;
        Console.Clear(); 

        // Get full screen dimensions dynamically
        width = Console.WindowWidth;
        height = Console.WindowHeight;

        // Start snake in the middle
        snake.Add((width / 2, height / 2));
        snake.Add((width / 2 - 1, height / 2));
        snake.Add((width / 2 - 2, height / 2));
        PlaceFood();

        while (true)
        {
            // Handle window resizing dynamically
            if (Console.WindowWidth != width || Console.WindowHeight != height)
            {
                width = Console.WindowWidth;
                height = Console.WindowHeight;
                Console.Clear();
            }

            Draw();
            HandleInput();
            Update();

            tick++;
            Thread.Sleep(80); 
        }
    }

    static void Draw()
    {
        Console.SetCursorPosition(0, 0);
        
        // Using StringBuilder for better performance on full screen
        var screen = new System.Text.StringBuilder(width * height);

        for (int y = 0; y < height - 1; y++) // -1 to leave room for the score text
        {
            for (int x = 0; x < width; x++)
            {
                // Draw Walls
                if (x == 0 || x == width - 1 || y == 0 || y == height - 2)
                {
                    screen.Append(WALL_COLOR + "█" + RESET);
                    continue;
                }

                // Draw Snake Head (with Eyes)
                if (x == snake[0].x && y == snake[0].y)
                {
                    string headChar = "■";
                    
                    // Change eyes based on direction
                    if (dirX == 1) headChar = EYE_COLOR + ">" + RESET; // Right
                    else if (dirX == -1) headChar = EYE_COLOR + "<" + RESET; // Left
                    else if (dirY == -1) headChar = EYE_COLOR + "^" + RESET; // Up
                    else if (dirY == 1) headChar = EYE_COLOR + "v" + RESET; // Down
                    
                    screen.Append(SNAKE_HEAD + headChar + RESET);
                    continue;
                }

                // Draw Snake Body
                bool isBody = false;
                for (int i = 1; i < snake.Count; i++)
                {
                    if (x == snake[i].x && y == snake[i].y)
                    {
                        screen.Append(SNAKE_BODY + "■" + RESET);
                        isBody = true;
                        break;
                    }
                }
                if (isBody) continue;

                // Draw Food
                if (x == foodX && y == foodY)
                {
                    screen.Append(FOOD_COLOR + "●" + RESET);
                    continue;
                }

                // Draw Tongue flickering in front of the head
                if (tick % 2 == 0)
                {
                    if (x == snake[0].x + dirX && y == snake[0].y + dirY)
                    {
                        screen.Append(TONGUE_COLOR + "~" + RESET);
                        continue;
                    }
                }

                screen.Append(" ");
            }
            screen.Append("\n");
        }
        
        Console.Write(screen.ToString());
        
        // Status bar at the bottom
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"Score: {SNAKE_HEAD}{score}{RESET}   |   WASD or Arrows to move");
    }

    static void HandleInput()
    {
        if (!Console.KeyAvailable) return;
        var key = Console.ReadKey(true).Key;

        switch (key)
        {
            case ConsoleKey.UpArrow: case ConsoleKey.W: if (dirY != 1) { dirX = 0; dirY = -1; } break;
            case ConsoleKey.DownArrow: case ConsoleKey.S: if (dirY != -1) { dirX = 0; dirY = 1; } break;
            case ConsoleKey.LeftArrow: case ConsoleKey.A: if (dirX != 1) { dirX = -1; dirY = 0; } break;
            case ConsoleKey.RightArrow: case ConsoleKey.D: if (dirX != -1) { dirX = 1; dirY = 0; } break;
        }
    }

    static void Update()
    {
        var head = snake[0];
        var newHead = (x: head.x + dirX, y: head.y + dirY);

        // Check Wall Collision (using dynamic height-2)
        if (newHead.x <= 0 || newHead.x >= width - 1 || newHead.y <= 0 || newHead.y >= height - 2)
            GameOver();

        // Check Self Collision
        foreach (var segment in snake)
            if (segment == newHead) GameOver();

        // Move Snake
        snake.Insert(0, newHead);

        // Check if food was eaten
        if (newHead.x == foodX && newHead.y == foodY)
        {
            score += 10;
            PlaceFood();
        }
        else
        {
            snake.RemoveAt(snake.Count - 1); 
        }
    }

    static void PlaceFood()
    {
        do
        {
            foodX = rng.Next(1, width - 1);
            foodY = rng.Next(1, height - 2);
        } while (snake.Contains((foodX, foodY))); 
    }

    static void GameOver()
    {
        Console.SetCursorPosition(width / 2 - 10, height / 2);
        Console.WriteLine($"{FOOD_COLOR}GAME OVER!{RESET}");
        Console.SetCursorPosition(width / 2 - 10, height / 2 + 1);
        Console.WriteLine($"Final Score: {SNAKE_HEAD}{score}{RESET}");
        
        // Move cursor to bottom so text doesn't overlap
        Console.SetCursorPosition(0, height - 1);
        Console.ReadKey();
        Environment.Exit(0);
    }
}