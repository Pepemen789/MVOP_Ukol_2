using System;
using System.Collections.Generic;


namespace MVOP_Ukol_2 {

    class Program {

        static void Main(string[] args) {

            Player player = new Player();

            //remove the comments below to test your strategies.
            PlayerStrategy[] strategies = {
             //new ExampleStrategy(),                   // 0 bodů
             new RandomStrategy(),                    // 2 body
             //new NoDuplicateRandomStrategy(),         // 2 body
             //new LinearStrategy(),                    // 2 body
            
             //new LinearHuntingStrategy(),             // 2 body
             //new NoDuplicateRandomHuntingStrategy(),  // 2 body
             //new DitheredHuntingStrategy(),           // 2 body
             
             //new WeightedHuntingStrategy(),            // 3 body
             //new CustomStrategy()                     // 1 až 3 body podle náročnosti na implementaci a úspěšnosti na počet tahů
            };
            float[] strategyResults = new float[strategies.Length];
            int gameCount = 300;
            int mapSize   = 10;
            int howManyBeforeAfterGamesToOutput = 2;    //the program will output the BEFORE/AFTER state of the map for each strategy
                                                        //change this variable to see more or less of the games

            for (int i = 0; i < strategies.Length; i++) {

                player.ChangeStrategy(strategies[i]);
                Console.WriteLine(new string('-', ($"Testing {strategies[i].GetType().Name}:  \\/\\/").Length));
                Console.WriteLine($"Testing {strategies[i].GetType().Name}:  \\/\\/");
                Console.WriteLine();
                strategyResults[i] = player.TestStrategy(gameCount, howManyBeforeAfterGamesToOutput,mapSize);

            }

            Console.WriteLine(new string('-', 52));
            Console.WriteLine($"Strategy performance results after {gameCount} games:  \\/\\/\n");

            for (int i = 0; i < strategies.Length; i++) {
                Console.WriteLine(i == 0 ? ' ' : '-');
                Console.WriteLine($"Average number of {strategies[i].GetType().Name} moves per game: {strategyResults[i]}");
            }

            Console.ReadKey();
        }
    }
}

   