using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVOP_Ukol_2 {

    public enum Direction {
        up = 1, down = 2, right = 3, left = 4
    }
    /// <summary>
    /// Types of tiles that a strategy can use. Unknown can be either empty, or any of the ships.
    /// </summary>
    public enum Tile {
        unknown = 0, miss = 1, hit = 2
    }
    /// <summary>
    /// All the possible results from firing at a tile. Starting at 3 and higher are all the ships you might be sinking.
    /// </summary>
    public enum Result {
        miss = 0, hit = 1, sinkDestroyer = 3, sinkSubmarine = 4, sinkCruiser = 5, sinkBattleship = 6, sinkCarrier = 7
    }

    public class Player {

        private PlayerStrategy strategy;
        private Map map;

        public Player() {
        }

        public Player(PlayerStrategy s) {
            strategy = s;
        }

        public void ChangeStrategy(PlayerStrategy s) {
            strategy = s;
        }
        private void InitNewGame(Map m) {
            map = m;
            strategy.Initialize();
        }

        private void Play() {
            int c = map.shotCounter;
            map.NextTurn();
            strategy.MakeAMove(map);
            if (c == map.shotCounter)
                throw new Exception("You didn't shoot in this round!");
        }

        /// <summary>
        /// Has the player run some N number of games with it's current strategy.
        /// </summary>
        /// <param name="numberOfGames">How many games to simulate</param>
        /// <param name="gameLogAmount">How many games do you want to print BEFORE/AFTER states to the console</param>
        /// <param name="mapSize">How big should the played map be</param>
        /// <returns></returns>
        public float TestStrategy(int numberOfGames, int gameLogAmount, int mapSize) {

            if (strategy.GetType() == typeof(ExampleStrategy)) {
                strategy.MakeAMove(new Map(0, mapSize));
                return float.NaN;
            }

            int moveCounter = 0;

            for (int i = 0; i < numberOfGames; i++) {
                Map m = new Map(i, mapSize);

                InitNewGame(m);

                do {
                    Play();
                } while (!m.finished);
                moveCounter += m.shotCounter;

                if (i < gameLogAmount) {
                    Console.WriteLine(new Map(i, mapSize));
                    Console.WriteLine("      /\\-Before/After-\\/");
                    Console.WriteLine(m);
                    Console.WriteLine();
                    Console.WriteLine();
                }

            }
            return (float)moveCounter / numberOfGames;
        }

    }
    /// <summary>
    /// In case you need to represent a 2D position more comfortably with one variable.
    /// </summary>
    public class Pos {
        public int x, y;
        public Pos() {
            x = 0;
            y = 0;
        }
        public Pos(int iy, int ix) {
            x = ix;
            y = iy;
        }
    }
    /// <summary>
    /// The map class represents a single game instance.
    /// </summary>
    public class Map {

        private enum TileData {
            empty = 0, miss = 1, hit = 2, destroyer = 3, submarine = 4, cruiser = 5, battleship = 6, carrier = 7, unknown = 8
        }
        Pos lastShot = new Pos(0, 0);

        int[] shipLengths = { 2, 3, 3, 4, 5 };

        private TileData[,] data;
        public int size { get { return _size; } }
        private int _size;
        public int shotCounter = 0;

        bool ready = true;

        /// <summary>
        /// Returns true if there are no more ships left to destroy
        /// </summary>
        public bool finished {
            get {
                for (int y = 0; y < size; y++) {
                    for (int x = 0; x < size; x++) {
                        if ((int)data[y, x] > 2)
                            return false;
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// Checks if the Y,X position is within the map.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns>Returns true if the input is on the map. Returns false if it isn't.</returns>
        public bool WithinBounds(int y, int x) {
            return (y >= 0 && y < size && x >= 0 && x < size);

        }

        /// <summary>
        /// Checks if the ship will fit on this map without collision with other ships. This method is private because it considers true map data.
        /// </summary>
        /// <param name="x">X position on the map of the leftest/lowest ship point</param>
        /// <param name="y">Y position on the map of the leftest/lowest ship point</param>
        /// <param name="l">Length of the ship</param>
        /// <param name="dir"> Direction of the ship</param>
        /// <returns></returns>
        private bool CheckAvailability(int y, int x, int l, Direction dir) {

            for (int i = 0; i < l; i++) {
                int diy = dir == Direction.up ? -i : (dir == Direction.down ? i : 0);
                int dix = dir == Direction.right ? i : (dir == Direction.left ? -i : 0);
                if (!WithinBounds(y + diy, x + dix) || data[y + diy, x + dix] != TileData.empty)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Checks if the ship will fit on Y,X tile given what the player knows about hits and misses. This method is public because it doesn't consider true map data.
        /// </summary>
        /// <param name="x">X position on the map of the leftest/lowest ship point</param>
        /// <param name="y">Y position on the map of the leftest/lowest ship point</param>
        /// <param name="l">Length of the ship</param>
        /// <param name="dir"> Direction of the ship</param>
        /// <returns></returns>
        public bool CheckForAvailability(int y, int x, int l, Direction dir) {

            for (int i = 0; i < l; i++) {
                int diy = dir == Direction.up ? -i : (dir == Direction.down ? i : 0);
                int dix = dir == Direction.right ? i : (dir == Direction.left ? -i : 0);
                if (!WithinBounds(y + diy, x + dix) || data[y + diy, x + dix] == TileData.hit || data[y + diy, x + dix] == TileData.miss)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns an instance of a map with random ships based on the INT seed
        /// </summary>
        public Map(int seed, int iSize) {

            _size = iSize;
            Random r = new Random(seed);
            data = new TileData[size, size];
            ready = true;
            //init empty map
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    data[y, x] = TileData.empty;
                }
            }

            //for each of the 5 ships
            for (int i = 0; i < 5; i++) {

                int x, y;
                int l = shipLengths[i]; // length of this ship
                List<Direction> availableDirections = new List<Direction>();

                do { // pick random positions untill it can fit in at least one direction
                    x = r.Next(0, size);
                    y = r.Next(0, size);
                    availableDirections.Clear();

                    for (int d = 1; d < 5; d++) {
                        if (CheckAvailability(y, x, l, (Direction)d))
                            availableDirections.Add((Direction)d);
                    }
                } while (availableDirections.Count == 0);

                //pick random direction
                Direction selectedDirection = availableDirections[r.Next(0, availableDirections.Count)];

                //write the indexed ship to the map with the randomly selected direction
                for (int s = 0; s < l; s++) {
                    //Don't try this at home.
                    data[y + (selectedDirection == Direction.up ? -s : (selectedDirection == Direction.down ? s : 0)),
                        x + (selectedDirection == Direction.right ? s : (selectedDirection == Direction.left ? -s : 0))] = (TileData)(i + 3);
                }
            }
        }


        public override string ToString() {
            string s = "";
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    switch (data[y, x]) {
                        case TileData.empty:
                            s += "[ ]";
                            break;
                        case TileData.hit:
                            s += lastShot.x == x && lastShot.y == y ? ">X<" : "[x]";
                            break;
                        case TileData.miss:
                            s += lastShot.x == x && lastShot.y == y ? ">O<" : "[o]";
                            break;
                        default:
                            s += $"[{(int)data[y, x]}]"; //writes out the ships ENUM number to identify it
                            break;
                    }

                }
                if (y != size - 1)
                    s += '\n';

            }
            return s;
        }
        /// <summary>
        /// Make sure you only shoot once per turn.
        /// </summary>
        public void NextTurn() {
            ready = true;
        }
        /// <summary>
        /// Shoots at the Y, X position and returns what is on that tile. Throws exceptions if you are trying to shoot outside the map, or shooting twice in one round, or if your strategy is shooting waay to much.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns>Returns Result.miss if you missed. Result.hit if you hit. Returns a ship type if you sink as hip.</returns>
        public Result Shoot(int y, int x) {
            if (!WithinBounds(y,x))
                throw new Exception("You are trying to shoot something outside the map!");

            if (!ready)
                throw new Exception("You are trying to shoot twice in the same round!");

            shotCounter++;

            if (shotCounter > size * size * 30)
                throw new Exception("You have tried way too many times.");
            ready = false;

            lastShot.y = y;
            lastShot.x = x;

            switch (data[y, x]) {
                case TileData.miss:
                case TileData.empty:
                    data[y, x] = TileData.miss;
                    return Result.miss;

                case TileData.hit:
                    return Result.hit;
                default:
                    Result res;
                    int hitShipIndex = (int)data[y, x] - 3;

                    shipLengths[hitShipIndex]--;
                    if (shipLengths[hitShipIndex] == 0) {
                        shipLengths[hitShipIndex]--;
                        res = (Result)data[y, x];
                    } else {
                        res = Result.hit;
                    }

                    data[y, x] = TileData.hit;
                    return res;
            }


        }
        /// <summary>
        /// Tells the player what is on Y,X position. Doesn't give away anything about the player shouldn't know.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns>Returns Tile.miss if the player already missed on this tile, Tile.hit if the player already hit on this tile and Tile.unknown in all other cases.</returns>
        public Tile Look(int y, int x) {
            TileData t = data[y, x];
            if ((int)t == 1 || (int)t == 2)
                return (Tile)(int)data[y, x];
            else
                return Tile.unknown;

        }




    }
}
