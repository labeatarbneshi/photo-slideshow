using PhotoSlideshow.Configuration;
using System;
using System.Diagnostics;

namespace PhotoSlideshow
{
    class ILS
    {
        private readonly Solution solution;
        private readonly int initialScore;
        public Stopwatch executionTime;
        public static Stopwatch TimeWithoutProgress;
        public static Stopwatch AcceptWorseSolution;
        public ILS(Solution solution)
        {
            this.solution = solution;
            initialScore = solution.Slideshow.Score;
        }

        /// <summary>
        /// Starts solution optimizing by choosing operators randomly
        /// </summary>
        public void Optimize()
        {
            TimeWithoutProgress = Stopwatch.StartNew();
            executionTime = Stopwatch.StartNew();
            AcceptWorseSolution = Stopwatch.StartNew();

            var score = initialScore;
            var highestScore = initialScore;
            do
            {
                var mutationGain = solution.Mutate();

                if (mutationGain < 0)
                {
                    ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis += 100;
                    AcceptWorseSolution.Restart();
                    TimeWithoutProgress.Restart();
                }

                if (ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis > 10000)
                {
                    ConfigurationConsts.SlideSwapUpperFrequency = 0;
                    ConfigurationConsts.VerticalPhotoSwapFrequencyLowerLimit = 1;
                    ConfigurationConsts.VerticalPhotoSwapFrequencyUpperLimit = 10;
                    ConfigurationConsts.ShuffleOperatorFrequency = 12;
                    ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis = 2500;
                }

                score += mutationGain;
                if (mutationGain > 0)
                {
                    highestScore = score;
                    TimeWithoutProgress.Restart();
                    Console.WriteLine("NEW SCORE: " + score);
                }
            }
            while (executionTime.ElapsedMilliseconds < ConfigurationConsts.RunDuration);

            Common.SaveSolution(solution.Slideshow, "final_solution");
        }
    }
}
