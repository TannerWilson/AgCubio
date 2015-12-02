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

        // Stored all cubes controlled by the player
        public Dictionary<string, Cube> FoodCubes;

        private bool FirstPlayer;

        private string PlayersName;

        // Used to represent cube specific IDs
        public int UIDCount = 0;
        int TeamIDCount = 0;

        //*********************
        // Default Values
        //*********************
        private int MaxSpeed;
        private int MinSpeed;
        private float AttritionRate;
        private int FoodValue;
        private float StartingMassValue = 1000;
        private int MaxFood = 5000;
        private float MinSplitMass;
        private float MinSplitDistance;
        private int MaxSplits;
        private float AbsorbDistance;

        public static  Random Rand;


        /// <summary>
        /// Constructs a new world object given a height width and collection of cubes
        /// </summary>
        public World(int height, int width)
        {
            this.Height = height;
            this.Width = width;
            this.DictionaryOfCubes = new Dictionary<string, Cube>();
            this.PlayerCubes = new Dictionary<string, Cube>();
            this.FoodCubes = new Dictionary<string, Cube>();
            FirstPlayer = true;
            Rand = new Random();
        }

        /// <summary>
        /// Creates a unique UID for each cube
        /// </summary>
        /// <returns></returns>
        private int CreateUID()
        {
            UIDCount++;
            return UIDCount;
        }

        /// <summary>
        /// Makes a team ID for player cubes
        /// </summary>
        /// <returns></returns>
        private int CreateTeamID()
        {
            TeamIDCount++;
            return TeamIDCount;
        }

        /// <summary>
        /// Creates a random integer RGB value used to create colors.
        /// </summary>
        /// <returns></returns>
        private int CreateRandomARGBColor()
        {
            Random rand = new Random();
            return rand.Next(-1600, 1600);
        }

        /// <summary>
        /// Creates a random integer RGB value used to create colors.
        /// </summary>
        /// <returns></returns>
        private static int ToArgb()
        {
 
            int a = 255;
            int r = Rand.Next(0, 255);
            int g = Rand.Next(0, 255);
            int b = Rand.Next(0, 255);

            return a << 24 | r << 16 | g << 8 | b;
        }

        /// <summary>
        /// Generates a random location to place cubes on the screen
        /// (Used by the server)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void GetRandomLocation(out int x, out int y)
        {
            
            x = Rand.Next(0, Width);
            y = Rand.Next(0, Height);
        }


        public void ActionCommand(string Action, int x, int y)
        {
            if(Action == "Move" || Action == "move")
            {
                Console.WriteLine("Move to "+ x +", " + y );
            }
            else if(Action == "Split" || Action == "split")
            {
                Console.WriteLine("Split to " + x + ", " + y);
            }
        }


        /// <summary>
        /// Creates the player cube (Used by server)
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public string MakePlayer(string Name)
        {
            // Find random location
            int x, y;           
            GetRandomLocation(out x, out y);

            // Create players cube
            Cube NewPlayerCube = new Cube(CreateUID().ToString(), Name, ToArgb(), StartingMassValue, false, 0, 0, CreateTeamID());

            // Generate and return JSON string.
            ServerMakeCube(JsonConvert.SerializeObject(NewPlayerCube));
            return JsonConvert.SerializeObject(NewPlayerCube);
        }

        /// <summary>
        /// Takes an input string, splits it at the '\n' character and 
        /// then creats cube objects based on the split strings.
        /// </summary>
        public void makeCube(string input)
        {
            Cube adding = JsonConvert.DeserializeObject<Cube>(input);

            //Add first entry to player dictionary
            if (FirstPlayer)
            {
                PlayerCubes.Add(adding.GetUID(), adding);
                PlayersName = adding.Name;
                FirstPlayer = false;
            }
            else
            {
                // Add to player cubes
                if (adding.Name == PlayersName)
                {
                    if (adding.Mass == 0)
                        PlayerCubes.Remove(adding.GetUID());
                    else
                        PlayerCubes[adding.GetUID()] = adding;
                }
                else // Add to all other cubes
                {
                    if (adding.Mass == 0)
                        DictionaryOfCubes.Remove(adding.GetUID());
                    else
                        DictionaryOfCubes[adding.GetUID()] = adding;
                }
            }
        }

        /// <summary>
        /// Creates and adds given cube "input" to our dictionary of cubes
        /// </summary>
        /// <param name="input"></param>
        public void ServerMakeCube(string input)
        {
            Cube adding = JsonConvert.DeserializeObject<Cube>(input);

            if (adding.Mass == 0)
                DictionaryOfCubes.Remove(adding.GetUID());
            else
                DictionaryOfCubes[adding.GetUID()] = adding;
        }

        /// <summary>
        /// Creates food cubes until the max food count is reached.
        /// </summary>
        public void CreateFood()
        {
            int CurrentFoodCount = FoodCubes.Count;

            for (int i = 0; i < MaxFood - CurrentFoodCount; i++)
            {
                int x, y;
                GetRandomLocation(out x, out y);

                Cube NewFoodCube = new Cube(CreateUID().ToString(), "", ToArgb(), 1, true, x, y, -1);

                FoodCubes.Add(NewFoodCube.GetUID(), NewFoodCube);
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
        public bool FoodStatus;
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

