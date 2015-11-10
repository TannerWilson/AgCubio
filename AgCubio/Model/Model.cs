using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Model
    {
        public class World
        {
            // Height and width of the world view screen
            private readonly int Height;
            private readonly int Width;

            // Set of cubes in the world
            private HashSet<Cube> cubes;

            /// <summary>
            /// Constructs a new world object given a height width and collection of cubes
            /// </summary>
            public World(int height, int width, HashSet<Cube> cubeset)
            {
                this.Height = height;
                this.Width = width;
                this.cubes = cubeset;
            }

        }

        /// <summary>
        ///  Helper class to "world" used to store all information associated with a given cube
        /// </summary>
        public class Cube
        {
            // Unique ID for cube
            private string UID;
            // Name of the cube
            private string Name;
            // Color of the cube
            private ConsoleColor Color;
            // Mass of the cube
            private double Mass;
            // If cube is food or not
            private bool FoodStatus;
            // Cubes X and Y positions in space
            private int X;
            private int Y;

            /// <summary>
            /// Constructor used to initialize each member variable
            /// </summary>
            public Cube(String id, string name, ConsoleColor color, double mass, bool status, int x, int y)
            {
                UID = id;
                this.Name = name;
                this.Color = color;
                this.Mass = mass;
                FoodStatus = status;
                this.X = x;
                this.Y = y;
            }

            /// <summary>
            /// Returns the cube's width
            /// </summary>
            public double getWidth()
            {
                // Return square root of the mass
                return Math.Sqrt(Mass);
            }

            /// <summary>
            /// Returns the Y position of the top of the cube
            /// </summary>
            public int Top()
            {
                return Y - (int)this.getWidth();
            }

            /// <summary>
            /// Returns the X position of the right edge of the cube
            /// </summary>
            public int Right()
            {
                return X + (int)this.getWidth();
            }

            /// <summary>
            /// Returns the X position of the left edge of the cube
            /// </summary>
            public int Left()
            {
                return X - (int)this.getWidth();
            }

            /// <summary>
            /// Returns the Y position of the bottom edge of the cube
            /// </summary>
            public int Bottom()
            {
                return Y + (int)this.getWidth();
            }
        }
    }
}
