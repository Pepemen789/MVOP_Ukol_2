﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MVOP_Ukol_2
{

    /// <summary>
    /// Abstract class can not be instantiated. This servers only as a reference what ALL the other strategies look like. (What methods or variables they must have)
    /// </summary>
    public abstract class PlayerStrategy
    {
        public abstract void Initialize();
        public abstract void MakeAMove(Map map);
        public int y;
        public int x;
        public Result result;
        protected static Random rng = new Random();
        protected (int y, int x)? lastHit = null;
        public bool isTargetMode;
        public List<(int, int)> next_targets = new List<(int, int)>();
        public bool hasShot;
        public List<(int, int)> TargetMode(Map map, int lastX, int lastY)
        {
            List<(int y, int x)> validDirections = new List<(int, int)>();
            List<(int y, int x)> directions = new List<(int, int)>
        {
            (lastX - 1, lastY),
            (lastX + 1, lastY),
            (lastX, lastY - 1),
            (lastX, lastY + 1)
        };

            foreach (var dir in directions)
            {
                int tx = dir.x, ty = dir.y;

                if (map.WithinBounds(ty, tx) && map.Look(ty, tx) == Tile.unknown)
                {
                    validDirections.Add((ty, tx));
                }
            }
            return validDirections;
        }
        public void TargetMode2(Map map)
        {
            // list sousednich policek ktere neznam

            ////Console.WriteLine("next_targets: " + next_targets.Count);
            ////Console.WriteLine(next_targets[0].Item1 + " " + next_targets[0].Item2);

            result = map.Shoot(next_targets[0].Item1, next_targets[0].Item2);


            // pokud trefim, pridat dalsi do next_targets
            if (map.Look(next_targets[0].Item1, next_targets[0].Item2) == Tile.hit)
            {
                next_targets.AddRange(TargetMode(map, next_targets[0].Item1, next_targets[0].Item2));
            }
            next_targets.RemoveAt(0);

            if (next_targets.Count == 0)
            {
                isTargetMode = false;
            }
            ////Console.WriteLine(y + " " + x);
            ////Console.WriteLine(map);
            ////Console.WriteLine();
            //Console.ReadKey();
        }
        public (int y, int x) GetRandomCoordinates(Map map)
        {
            x = rng.Next(0, map.size);
            y = rng.Next(0, map.size);
            return (y, x);
        }
        public void MoveToNextPosition(Map map)
        {

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
        public void DitheredMoveToNextPosition(Map map)
        {

            if (x >= map.size - 2)
            {
                if (y % 2 == 0)
                {
                    x = 1;
                }
                else
                {
                    x = 0;
                }
                y++;
            }
            else
            {

                x += 2;

            }
        }
        public void TargetModeOn(Map map)
        {
            if (map.Look(y, x) == Tile.hit)
            {
                isTargetMode = true;
                lastHit = (y, x);
                next_targets = TargetMode(map, lastHit.Value.y, lastHit.Value.x);
            }
        }











    }

    /// <summary>
    /// This is an example how your strategies should be implemented. 
    /// </summary>
    public class ExampleStrategy : PlayerStrategy
    {

        int myVariable;

        // Every strategy is initialized at the beginning of each game
        public override void Initialize()
        {
            myVariable = 3;

        }

        //This is the method that will be called every time you have to make a move.
        public override void MakeAMove(Map map)
        {

            if (CanIShootHere(myVariable, map))
            {
                map.Shoot(myVariable, myVariable);                      // map.Shoot(Y,X) will shoot at the given [Y,X] position
            }
            //Console.WriteLine(map);                                     //I can use this to write the game board to the console each round

        }

        // You can and should create helping methods
        private bool CanIShootHere(int num, Map map)
        {

            // map.Look(Y,X) will give me either Tile.miss or Tile.hit or Tile.unkown 
            if (map.Look(myVariable, myVariable) == Tile.unknown)
                return true;
            else
                return false;
        }


    }

    //This strategy should shoot randomly.
    public class RandomStrategy : PlayerStrategy
    {

        public override void Initialize()
        {

        }

        public override void MakeAMove(Map map)
        {

            GetRandomCoordinates(map);

            map.Shoot(y, x);
        }
    }

    //This strategy should shoot randomly, but only where it hasn't shot at before.
    public class NoDuplicateRandomStrategy : PlayerStrategy
    {

        public override void Initialize()
        {

        }

        public override void MakeAMove(Map map)
        {

            do
            {
                GetRandomCoordinates(map);
            }
            while (map.Look(y, x) != Tile.unknown);

            map.Shoot(y, x);



        }
    }

    //This strategy should sequentially shoot at every tile starting (for example) at the top left corner, going to the right and then the next row.
    public class LinearStrategy : PlayerStrategy
    {


        public override void Initialize()
        {
            x = 0;
            y = 0;
        }

        public override void MakeAMove(Map map)
        {

            map.Shoot(y, x);

            MoveToNextPosition(map);
        }


    }

    //This strategy should shoot sequentially from top left to bottom, but when it hits a ship, it should be shooting (hunting) near that hit untill it sinks a ship.
    public class LinearHuntingStrategy : PlayerStrategy
    {
        public override void Initialize()
        {
            x = 0;
            y = 0;
            isTargetMode = false;
        }

        public override void MakeAMove(Map map)
        {
            hasShot = false;

            // search mode
            if (!isTargetMode)
            {
                while (map.Look(y, x) != Tile.unknown)
                {
                    MoveToNextPosition(map);
                }


                if (map.Look(y, x) == Tile.unknown)
                {
                    map.Shoot(y, x);
                    hasShot = true;

                    TargetModeOn(map);
                }
            }
            // target mode
            if (isTargetMode && lastHit.HasValue && !hasShot)
            {
                TargetMode2(map);

            }







            ////Console.WriteLine("target mode: " + isTargetMode);
            //foreach ((int y, int x) in next_targets)
            //{
            //    //Console.WriteLine("target: " + y + " " + x);
            //}
            ////Console.WriteLine(y + " " + x);
            // //Console.WriteLine(map);
            // //Console.WriteLine();
            //Console.ReadKey();
        }



    }



    //This strategy should shoot randomly, but when it hits a ship, it should be shooting (hunting) near that hit untill it sinks a ship.
    public class NoDuplicateRandomHuntingStrategy : PlayerStrategy
    {

        public override void Initialize()
        {
            isTargetMode = false;

        }

        public override void MakeAMove(Map map)
        {

            hasShot = false;

            if (!isTargetMode)
            {
                do
                {
                    GetRandomCoordinates(map);
                }
                while (map.Look(y, x) != Tile.unknown);

                map.Shoot(y, x);
                hasShot = true;



                TargetModeOn(map);

            }
            ////Console.WriteLine("target mode: " + isTargetMode);
            ////Console.WriteLine(map);
            ////Console.WriteLine();
            //Console.ReadKey();

            if (isTargetMode && lastHit.HasValue && !hasShot)
            {
                TargetMode2(map);
            }
        }


    }

    //This strategy should shoot sequentially from top left to bottom, but only every other tile and when it hits a ship, it should be shooting (hunting) near that hit untill it sinks a ship.
    public class DitheredHuntingStrategy : PlayerStrategy
    {
        public override void Initialize()
        {
            y = 0;
            x = 0;
            isTargetMode = false;
        }

        public override void MakeAMove(Map map)
        {
            hasShot = false;

            if (!isTargetMode)
            {
                while (map.Look(y, x) != Tile.unknown)
                {
                    DitheredMoveToNextPosition(map);
                }


                if (map.Look(y, x) == Tile.unknown)
                {
                    map.Shoot(y, x);
                    hasShot = true;

                    TargetModeOn(map);
                    DitheredMoveToNextPosition(map);
                }
            }
            ////Console.WriteLine("target mode: " + isTargetMode);

            ////Console.WriteLine(map);
            ////Console.WriteLine();
            //Console.ReadKey();

            if (isTargetMode && lastHit.HasValue && !hasShot)
            {
                TargetMode2(map);


            }
        }




    }

    //This strategy should consider the probability of any ship being at every tile of the map. Then pick the one with the highest probability and hunt the ship if it hits something.
    public class WeightedHuntingStrategy : PlayerStrategy
    {
        private int[,] weightMap;
        int max;
        Pos bestShot;
        public Dictionary<string, int> shipLengths = new Dictionary<string, int>();

        public override void Initialize()
        {
            shipLengths = new Dictionary<string, int>()
            {
                { "destroyer", 2 },
                { "submarine", 3 },
                { "cruiser", 3 },
                { "battleship", 4 },
                { "carrier", 5 }
            };
            result = 0;
        }
        public override void MakeAMove(Map map)
        {
            hasShot = false;
            weightMap = new int[map.size, map.size];
            for (int i = 0; i < map.size; i++)
            {
                for (int j = 0; j < map.size; j++)
                {
                    weightMap[i, j] = 0;
                }
            }
            CalculateProbabilities(map);
            for (int i = 0; i < weightMap.GetLength(0); i++)
            {
                //Console.WriteLine();
                for (int j = 0; j < weightMap.GetLength(1); j++)
                {
                    //Console.Write(weightMap[i, j] + " ");
                }
            }
            GetHighestProbability(map);
            //Console.WriteLine(max);
            if (!isTargetMode)
            {
                map.Shoot(bestShot.y, bestShot.x);
                hasShot = true;
                if (map.Look(bestShot.y, bestShot.x) == Tile.hit)
                {
                    isTargetMode = true;
                    lastHit = (bestShot.y, bestShot.x);
                    next_targets = TargetMode(map, lastHit.Value.y, lastHit.Value.x);
                }
            }


            if (isTargetMode && lastHit.HasValue && !hasShot)
            {
                TargetMode2(map);
            }

            switch (result)
            {
                case Result.sinkDestroyer:
                    shipLengths.Remove("destroyer");
                    //Console.WriteLine("Loď Destroyer potopená");
                    break;
                case Result.sinkSubmarine:
                    shipLengths.Remove("submarine");
                    //Console.WriteLine("Loď Submarine potopená");
                    break;
                case Result.sinkCruiser:
                    shipLengths.Remove("cruiser");
                    //Console.WriteLine("Loď cruiser potopená");
                    break;
                case Result.sinkBattleship:
                    shipLengths.Remove("battleship");
                    //Console.WriteLine("Loď battleship potopená");
                    break;
                case Result.sinkCarrier:
                    shipLengths.Remove("carrier");
                    //Console.WriteLine("Loď carrier potopená");
                    break;
            }
            PrintDictionary(shipLengths);
            //Console.WriteLine(map);
            //Console.ReadKey();
            
        }
        public void CalculateProbabilities(Map map)
        {
            
            
            for (int y = 0; y < map.size; y++)
            {
                for (int x = 0; x < map.size; x++)
                {
                    foreach (int shipLenght in shipLengths.Values)
                    {
                        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                        {
                            if (map.CheckForAvailability(y, x, shipLenght, dir))
                            {

                                weightMap[y, x] += 1;

                            }
                        }
                    }
                }
            }
        }
        public void GetHighestProbability(Map map)
        {
            max = 0;
            bestShot = null;
            for (int y = 0; y < map.size; y++)
            {
                for (int x = 0; x < map.size; x++)
                {
                    if (weightMap[y, x] > max)
                    {
                        max = weightMap[y, x];
                        bestShot = new Pos(y, x);


                    }

                }
            }

        }
        public void PrintDictionary(Dictionary<string, int> dictionary)
        {
            foreach (var entry in dictionary)
            {
                //Console.WriteLine($"Ship: {entry.Key}, Length: {entry.Value}");
            }
        }

    }

    //Do whatever 
    public class CustomStrategy : PlayerStrategy
    {
        public List<(int, int)> ditheredTargets = new List<(int, int)>();
        public override void Initialize()
        {
            isTargetMode = false;
        }
        public override void MakeAMove(Map map)
        {
            hasShot = false;

            if (!isTargetMode)
            {
                do
                {
                    GetRandomDitheredTarget(GenerateDitheredTargets(map.size, map.size));
                }
                while (map.Look(y, x) != Tile.unknown);

                map.Shoot(y, x);
                hasShot = true;



                TargetModeOn(map);

            }
            if (isTargetMode && lastHit.HasValue && !hasShot)
            {
                TargetMode2(map);
            }
            ////Console.WriteLine(map);
            ////Console.WriteLine(isTargetMode);
            ////Console.WriteLine(lastHit);

            //Console.ReadKey();

        }
        public List<(int, int)> GenerateDitheredTargets(int rows, int cols)
        {


            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if ((y + x) % 2 == 0)
                    {
                        ditheredTargets.Add((y, x));
                    }
                }
            }

            return ditheredTargets;
        }
        public void GetRandomDitheredTarget(List<(int, int)> ditheredTargets)
        {
            Random rnd = new Random();
            int randomIndex = rnd.Next(ditheredTargets.Count);
            y = ditheredTargets[randomIndex].Item1;
            x = ditheredTargets[randomIndex].Item2;

        }


    }

}