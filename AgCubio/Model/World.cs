using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Model
{

    /// <summary>
    ///  Helper class to "world" used to store all information associated with a given cube
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Cube
    {
        // Unique ID for cube
        [JsonProperty]
        public string uid;
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
        public bool food;
        // Cubes X and Y positions in space
        [JsonProperty]
        public float loc_x;
        [JsonProperty]
        public float loc_y;

        public float Speed;

        public float NewX;
        public float NewY;

        private float AttritionRate;

        public bool Changed;

        [JsonProperty]
        public int team_id;
        /// <summary>
        /// Constructor used to initialize each member variable
        /// </summary>
        [JsonConstructor]
        public Cube(String uid, string name, int argb_color, float mass, bool food, float loc_x, float loc_y, int Team_id)
        {
            this.uid = uid;
            this.Name = name;
            this.argb_color = argb_color;
            this.Mass = mass;
            this.food = food;
            this.loc_x = loc_x;
            this.loc_y = loc_y;
            this.team_id = Team_id;

            Speed = 1;
            Changed = false;
        }

        /// <summary>
        /// Returns the cube's width
        /// </summary>
        public float getWidth()
        {

            // Return square root of the mass
            return (float)Math.Pow(Mass, 0.65);
        }

        /// <summary>
        /// Returns the Y position of the top of the cube
        /// </summary>
        public double Top()
        {
            return loc_y - (double)this.getWidth();
        }

        /// <summary>
        /// Returns the X position of the right edge of the cube
        /// </summary>
        public double Right()
        {
            return loc_x + (double)this.getWidth();
        }

        /// <summary>
        /// Returns the X position of the left edge of the cube
        /// </summary>
        public double Left()
        {
            return loc_x - (double)this.getWidth();
        }

        /// <summary>
        /// Returns the Y position of the bottom edge of the cube
        /// </summary>
        public double Bottom()
        {
            return loc_y + (double)this.getWidth();
        }

        /// <summary>
        /// Update cube loaction and size and collition data
        /// </summary>
        public string Update()
        {
            // Shrink the cube based on its attrition rate
            Shrink();

            if (NewX != null && NewY != null)
                Move(NewX, NewY);
            // If the cube changed, return JSON string
            if (Changed)
            {
                return JsonConvert.SerializeObject(this) + '\n';
                Changed = false;
            }
            else
                return null;


        }

        public void Move(float x, float y)
        {
            if (this.loc_x < x)
            {
                this.loc_x += 1 * Speed;
                Changed = true;
            }
            else if (this.loc_x > x)
            {
                this.loc_x -= 1 * Speed;
                Changed = true;
            }

            if (this.loc_y < y)
            {
                this.loc_y += 1 * Speed;
                Changed = true;
            }
            else if (this.loc_y > y)
            {
                this.loc_y -= 1 * Speed;
                Changed = true;
            }
        }

        /// <summary>
        /// Shrinks the cube
        /// </summary>
        public void Shrink()
        {
            if (Mass < 200)
                AttritionRate = 0;
            else if (Mass > 200 && Mass < 800)
                AttritionRate = 0.5f;
            else if (Mass > 200)
                AttritionRate = 1;

            this.Mass -= AttritionRate;
        }

        public void SetNewPosition(float x, float y)
        {
            this.NewX = x;
            this.NewY = y;
        }

    }

    public class World
    {
        public Dictionary<string, Cube> DictionaryOfCubes;
        public Dictionary<string, Cube> FoodCubes;

        private readonly int Height;
        private readonly int Width;

        private string PlayersName;

        // Used to represent cube specific IDs
        public int UIDCount = 0;
        int TeamIDCount = 0;

        //*********************
        // Default Values
        //*********************
        private int MaxSpeed;
        private int MinSpeed;

        private int FoodValue;
        private float StartingMassValue = 1000;
        private int MaxFood = 10;
        private float MinSplitMass;
        private float MinSplitDistance;
        private int MaxSplits;
        private float AbsorbDistance;

        public static Random Rand;


        public World(int width, int height)
        {
            this.Height = height;
            this.Width = width;

            DictionaryOfCubes = new Dictionary<string, Cube>();
            FoodCubes = new Dictionary<string, Cube>();
            Rand = new Random();

        }

        public void AddCube(string JsonCubeString)
        {
            Cube NewCube = JsonConvert.DeserializeObject<Cube>(JsonCubeString);

            if (NewCube.food != true)
            {
                if (NewCube.Mass != 0)
                    DictionaryOfCubes[NewCube.uid] = NewCube;
                else
                    DictionaryOfCubes.Remove(NewCube.uid);
            }
            else
            {
                if (NewCube.Mass != 0)
                    FoodCubes[NewCube.uid] = NewCube;
                else
                    FoodCubes.Remove(NewCube.uid);
            }
        }

        ////****

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

        /// <summary>
        /// Process client requests
        /// </summary>
        public void ActionCommand(string Action, int x, int y, string UID)
        {
            if (Action == "Move" || Action == "move")
            {
                if (DictionaryOfCubes[UID].NewX != x && DictionaryOfCubes[UID].NewY != y)
                {
                    DictionaryOfCubes[UID].SetNewPosition(x, y);
                }
            }
            else if (Action == "Split" || Action == "split")
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
            AddCube(JsonConvert.SerializeObject(NewPlayerCube));
            return JsonConvert.SerializeObject(NewPlayerCube);
        }

        /// <summary>
        /// Creates Initial food cubes until the max food count is reached.
        /// </summary>
        public void CreateInitFood()
        {
            int CurrentFoodCount = FoodCubes.Count;

            for (int i = 0; i < MaxFood - CurrentFoodCount; i++)
            {
                int x, y;
                GetRandomLocation(out x, out y);

                Cube NewFoodCube = new Cube(CreateUID().ToString(), "", ToArgb(), 1, true, x, y, -1);

                FoodCubes.Add(NewFoodCube.uid, NewFoodCube);
            }
        }

        /// <summary>
        /// Generates single food to update world
        /// </summary>
        /// <returns></returns>
        public string GenerateNewFood()
        {
            int CurrentFoodCount = FoodCubes.Count;
            // Generate food if max food isnt yet reached
            if (CurrentFoodCount != MaxFood)
            {
                int x, y;
                GetRandomLocation(out x, out y);

                Cube NewFoodCube = new Cube(CreateUID().ToString(), "", ToArgb(), 1, true, x, y, -1);
                FoodCubes.Add(NewFoodCube.uid, NewFoodCube);

                return JsonConvert.SerializeObject(NewFoodCube) + '\n';
            }

            return null;
        }




        ///*****
    }
}
