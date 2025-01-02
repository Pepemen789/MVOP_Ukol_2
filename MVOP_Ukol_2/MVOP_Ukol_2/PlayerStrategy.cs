using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVOP_Ukol_2 {

    /// <summary>
    /// Abstract class can not be instantiated. This servers only as a reference what ALL the other strategies look like. (What methods or variables they must have)
    /// </summary>
    public abstract class PlayerStrategy {
        public abstract void Initialize();
        public abstract void MakeAMove(Map map);

        protected static Random rng = new Random();

        protected bool CanIShootHere(int y, int x, Map map) {
        return map.Look(y, x) == Tile.unknown;
        }

        protected void TargetMode(Map map, int lastX, int lastY) {
            List<(int y, int x)> directions = new List<(int, int)>
            {
                (lastX - 1, lastY),
                (lastX + 1, lastY),
                (lastX, lastY - 1),
                (lastX, lastY + 1)
            };

            foreach (var dir in directions) {
                int x = dir.x, y = dir.y;
                if (CanIShootHere(y, x,map)) {
                    map.Shoot(y, x);
                    if (map.Look(y, x) == Tile.hit) {
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// This is an example how your strategies should be implemented. 
    /// </summary>
    public class ExampleStrategy : PlayerStrategy {

        int myVariable;

        // Every strategy is initialized at the beginning of each game
        public override void Initialize() {
            myVariable = 3;

        }

        //This is the method that will be called every time you have to make a move.
        public override void MakeAMove(Map map) {

            if (CanIShootHere(myVariable, map)) {
                map.Shoot(myVariable, myVariable);                      // map.Shoot(Y,X) will shoot at the given [Y,X] position
            }
            Console.WriteLine(map);                                     //I can use this to write the game board to the console each round

        }

        // You can and should create helping methods
        private bool CanIShootHere(int num, Map map) {

            // map.Look(Y,X) will give me either Tile.miss or Tile.hit or Tile.unkown 
            if (map.Look(myVariable, myVariable) == Tile.unknown)
                return true;
            else
                return false;
        }


    }

    //This strategy should shoot randomly.
    public class RandomStrategy : PlayerStrategy {
        
        public override void Initialize() {

        }

        public override void MakeAMove(Map map) {

            int x = rng.Next(0, map.size);
            int y = rng.Next(0, map.size);

            map.Shoot(y, x);
        }
    }

    //This strategy should shoot randomly, but only where it hasn't shot at before.
    public class NoDuplicateRandomStrategy : PlayerStrategy {

        public override void Initialize() {

        }

        public override void MakeAMove(Map map) {
            
            int x,y;
            do
            {
                x = rng.Next(0, map.size);
                y = rng.Next(0, map.size);
            }
            while(map.Look(y, x) == Tile.unknown);

            map.Shoot(y, x);
        }
    }

    //This strategy should sequentially shoot at every tile starting (for example) at the top left corner, going to the right and then the next row.
    public class LinearStrategy : PlayerStrategy {
        private int x;
        private int y;
        

        public override void Initialize() {
            x = 0;
            y = 0;
        }

        public override void MakeAMove(Map map) {
            if (CanIShootHere(y, x, map)) {
            map.Shoot(y, x);
        }

        MoveToNextPosition(map);
        }

        private void MoveToNextPosition(Map map) {
           
            if (x >= map.size - 1)
            {
                x = 0;
                y++;
            }
            else
            {
                x++;
            }
        }
    }

    //This strategy should shoot sequentially from top left to bottom, but when it hits a ship, it should be shooting (hunting) near that hit untill it sinks a ship.
    public class LinearHuntingStrategy : PlayerStrategy {


        public override void Initialize() {
            //todo:
        }

        public override void MakeAMove(Map map) {
            //todo:
        }


    }
    //This strategy should shoot randomly, but when it hits a ship, it should be shooting (hunting) near that hit untill it sinks a ship.
    public class NoDuplicateRandomHuntingStrategy : PlayerStrategy {

        public override void Initialize() {
            //todo:
        }

        public override void MakeAMove(Map map) {
            //todo:
        }


    }

    //This strategy should shoot sequentially from top left to bottom, but only every other tile and when it hits a ship, it should be shooting (hunting) near that hit untill it sinks a ship.
    public class DitheredHuntingStrategy : PlayerStrategy {

        public override void Initialize() {
            //todo:
        }

        public override void MakeAMove(Map map) {
            //todo:
        }



    }

    //This strategy should consider the probability of any ship being at every tile of the map. Then pick the one with the highest probability and hunt the ship if it hits something.
    public class WeightedHuntingStrategy : PlayerStrategy {

        public override void Initialize() {
            //todo:
        }
        public override void MakeAMove(Map map) {
            //todo:
        }
    }

    //Do whatever 
    public class CustomStrategy : PlayerStrategy {

        public override void Initialize() {
            //todo:
        }
        public override void MakeAMove(Map map) {
            //todo:
        }
    }

}
