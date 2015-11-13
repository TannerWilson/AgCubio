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
        private Object MakeCubeLock = new Object();

        private string Part;

        // All cubes in the world object
        public Dictionary<string, Cube> DictionaryOfCubes;

        // Stored all cubes controlled by the player
        public Cube PlayerCubes;

        private bool FirstPlayer;

        /// <summary>
        /// Constructs a new world object given a height width and collection of cubes
        /// </summary>
        public World(int height, int width)
        {
            this.Height = height;
            this.Width = width;
            this.DictionaryOfCubes = new Dictionary<string, Cube>();
            
            FirstPlayer = true;
        }


        /// <summary>
        /// Takes an input string, splits it at the '\n' character and 
        /// then creats cube objects based on the split strings.
        /// </summary>
        public void makeCube(string input)
        {

            // Lock for thread safety
            lock (MakeCubeLock)
            {
                
                // Split input buffer at new lines
                string[] SplitString = input.Split('\n');
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
                            // Save string to be appended next iteration
                            Part = Item;
                        else // Complete valid item
                            FinalString = Item;

                        // Add cube to set
                        Cube adding;
                        if (FinalString != null && Changed == false)
                        {
                            adding = JsonConvert.DeserializeObject<Cube>(FinalString);

                            // Ensure we only add actual cubes
                            if (adding != null)
                            {
                                // Add first entry to player dictionary
                                if (FirstPlayer)
                                {
                                    PlayerCubes = adding;
                                    FirstPlayer = false;
                                }
                                else
                                {
                                    UpdateCube(adding);
                                }

                                
                            }
                        }

                    }
                }

            }

        }



        /// <summary>
        /// Updates values or adds cubes to DictionaryOfCubes
        /// </summary>
        /// <param name="NewCube"></param>
        void UpdateCube(Cube NewCube)
        {
            // Check if cube is in DictionaryOfCubes.
            if (!DictionaryOfCubes.ContainsKey(NewCube.GetUID()))
                DictionaryOfCubes.Add(NewCube.GetUID(), NewCube);
            else
            {
                Cube NewCubeValue;
                //Update values
                if (DictionaryOfCubes.TryGetValue(NewCube.GetUID(), out NewCubeValue))
                {
                    // Update Mass
                    if (NewCubeValue.GetMass() == 0) // Delete if cube has 0 mass.
                        DictionaryOfCubes.Remove(NewCube.GetUID());
                    else
                        NewCubeValue.Mass = NewCube.Mass;
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
        public int Color;
        // Mass of the cube
        [JsonProperty]
        public double Mass;

        //public double Hours
        //{
        //    get { return seconds / 3600; }
        //    set { seconds = value * 3600; }
        //}
        // If cube is food or not
        [JsonProperty]
        private bool FoodStatus;
        // Cubes X and Y positions in space
        [JsonProperty]
        public double X;
        [JsonProperty]
        public double Y;

        /// <summary>
        /// Constructor used to initialize each member variable
        /// </summary>
        [JsonConstructor]
        public Cube(String uid, string name, int color, double mass, bool food, double loc_x, double loc_y)
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

        public string GetUID()
        {
            return UID;
        }

        public double GetMass()
        {
            return Mass;
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

