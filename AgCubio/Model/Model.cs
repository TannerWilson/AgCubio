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



        // All cubes in the world object
        public Dictionary<string, Cube> DictionaryOfCubes;

        // Stored all cubes controlled by the player
        public Dictionary<string, Cube> PlayerCubes;

        private bool FirstPlayer;

        private string PlayersName;

        /// <summary>
        /// Constructs a new world object given a height width and collection of cubes
        /// </summary>
        public World(int height, int width)
        {
            this.Height = height;
            this.Width = width;
            this.DictionaryOfCubes = new Dictionary<string, Cube>();
            this.PlayerCubes = new Dictionary<string, Cube>();

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
                Cube adding = JsonConvert.DeserializeObject<Cube>(input);

                // Ensure we only add actual cubes
                if (adding != null)
                {
                    // Add first entry to player dictionary
                    if (FirstPlayer)
                    {
                        PlayerCubes.Add(adding.GetUID(), adding);
                        PlayersName = adding.Name;

                        FirstPlayer = false;
                    }
                    else
                        UpdateCube(adding);
                }
            }
        }

        /// <summary>
        /// Returns all other cubes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Cube> GetCubeValues()
        {
            lock (MakeCubeLock)
            {
                foreach (Cube item in DictionaryOfCubes.Values)
                    yield return item;
            }
        }

        /// <summary>
        /// Returns the players cubes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Cube> GetPlayerCubes()
        {
            lock (MakeCubeLock)
            {
                foreach (Cube item in PlayerCubes.Values)
                    yield return item;
            }
        }

        /// <summary>
        /// Updates values or adds cubes to DictionaryOfCubes
        /// </summary>
        /// <param name="NewCube"></param>
        void UpdateCube(Cube NewCube)
        {
            // Check if cube is in DictionaryOfCubes.
            if (!DictionaryOfCubes.ContainsKey(NewCube.GetUID()) && !PlayerCubes.ContainsKey(NewCube.GetUID()))
            {
                if (NewCube.Name != PlayersName)
                    DictionaryOfCubes.Add(NewCube.GetUID(), NewCube);
                else
                    PlayerCubes.Add(NewCube.GetUID(), NewCube);
            }
            else
            {
                Cube NewCubeValue;
                //Update values
                if (DictionaryOfCubes.TryGetValue(NewCube.GetUID(), out NewCubeValue))
                {
                    // Update Mass
                    if (NewCube.Mass == 0) // Delete if cube has 0 mass.
                        DictionaryOfCubes.Remove(NewCube.GetUID());
                    else
                    {
                        NewCubeValue.Mass = NewCube.Mass;
                        NewCubeValue.X = NewCube.X;
                        NewCubeValue.Y = NewCube.Y;
                    }
                }

                if (PlayerCubes.TryGetValue(NewCube.GetUID(), out NewCubeValue))
                {
                    // Update Mass
                    if (NewCube.Mass == 0) // Delete if cube has 0 mass.
                        PlayerCubes.Remove(NewCube.GetUID());
                    else
                    {
                        NewCubeValue.Mass = NewCube.Mass;
                        NewCubeValue.X = NewCube.X;
                        NewCubeValue.Y = NewCube.Y;
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
        public string Name;
        // Color of the cube
        [JsonProperty]
        public int argb_color;
        // Mass of the cube
        [JsonProperty]
        public float Mass;

        // If cube is food or not
        [JsonProperty]
        private bool FoodStatus;
        // Cubes X and Y positions in space
        [JsonProperty]
        public float X;
        [JsonProperty]
        public float Y;

        [JsonProperty]
        public int team_id;
        /// <summary>
        /// Constructor used to initialize each member variable
        /// </summary>
        [JsonConstructor]
        public Cube(String uid, string name, int argb_color, float mass, bool food, float loc_x, float loc_y, int Team_id)
        {
            UID = uid;
            this.Name = name;
            this.argb_color = argb_color;
            this.Mass = mass;
            FoodStatus = food;
            this.X = loc_x;
            this.Y = loc_y;
            this.team_id = Team_id;
        }

        /// <summary>
        /// Returns the cube's width
        /// </summary>
        public float getWidth()
        {

            // Return square root of the mass
            return (float)Math.Pow(Mass, 0.65);
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

