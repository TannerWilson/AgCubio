using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Model
{

    /// <summary>
    /// Class to represent the world object 
    /// </summary>
    public class World
    {
        // Height and width of the world view screen
        private readonly int Height;
        private readonly int Width;

        
        private string Part;
        // Set of cubes in the world
        private HashSet<Cube> WorldCubes;

        /// <summary>
        /// Constructs a new world object given a height width and collection of cubes
        /// </summary>
        public World(int height, int width, HashSet<Cube> cubeset)
        {
            this.Height = height;
            this.Width = width;
            this.WorldCubes = cubeset;
        }


        /// <summary>
        /// Takes an input string, splits it at the '\n' character and 
        /// then creats cube objects based on the split strings.
        /// </summary>
        public void makeCube(string input)
        {
            // Split input buffer at new lines
            string[] SplitString = input.Split('\n');
            Debug.WriteLine(WorldCubes.Count);
            foreach (string Item in SplitString)
            {
                bool Changed = false; // Used to 
                string FinalString = null; // Used to append broken data

                // Ensure no error strings are accepted
                if (!Item.StartsWith("\0\0") && Item != string.Empty)
                {
                    // Broken item at first index (no valid start)
                    if (!Item.StartsWith("{"))
                    {
                        // append this item to the broken string from last iteration
                        FinalString = Part + Item;
                        Changed = true;
                    }
                    // Broken item and last index (no valid ending)
                    if (!Item.EndsWith("}"))
                    {
                        // Save string to be appended next iteration
                        Part = Item;
                    }
                    else // Complete valid item
                    {
                        FinalString = Item;
                    }
                    // Add cube to set
                    Cube adding;
                    if (FinalString != null && Changed == false)
                    {
                        adding = JsonConvert.DeserializeObject<Cube>(FinalString);
                        // Ensure we only add actual cubes
                        if(adding != null)
                            WorldCubes.Add(adding);
                    }
                }
            }
        }

    }

    /// <summary>
    ///  Helper class to "world" used to store all information associated with a given cube
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Cube
    {
        // Unique ID for cube
        [JsonProperty]
        private string UID;
        // Name of the cube
        [JsonProperty]
        private string Name;
        // Color of the cube
        [JsonProperty]
        private ConsoleColor Color;
        // Mass of the cube
        [JsonProperty]
        private double Mass;
        // If cube is food or not
        [JsonProperty]
        private bool FoodStatus;
        // Cubes X and Y positions in space
        [JsonProperty]
        private double X;
        [JsonProperty]
        private double Y;

        /// <summary>
        /// Constructor used to initialize each member variable
        /// </summary>
        [JsonConstructor]
        public Cube(String uid, string name, ConsoleColor color, double mass, bool food, double loc_x, double loc_y)
        {
            UID = uid;
            this.Name = name;
            this.Color = color;
            this.Mass = mass;
            FoodStatus = food;
            this.X = loc_x;
            this.Y = loc_y;
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
        public double Top()
        {
            return Y - (double)this.getWidth();
        }

        /// <summary>
        /// Returns the X position of the right edge of the cube
        /// </summary>
        public double Right()
        {
            return X + (double)this.getWidth();
        }

        /// <summary>
        /// Returns the X position of the left edge of the cube
        /// </summary>
        public double Left()
        {
            return X - (double)this.getWidth();
        }

        /// <summary>
        /// Returns the Y position of the bottom edge of the cube
        /// </summary>
        public double Bottom()
        {
            return Y + (double)this.getWidth();
        }
    }
}

