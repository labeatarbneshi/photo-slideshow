using PhotoSlideshow.Configuration;
using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PhotoSlideshow
{
    class ILS
    {
        public ILS()
        {
        }

        /// <summary>
        /// Starts solution optimizing by choosing operators randomly
        /// </summary>
        public Solution FindSolution() {
            
            DateTime startTime = DateTime.Now;
            Random rnd = new Random();
            List<int> T = new List<int>() { 100, 120, 115, 70, 85, 90 };
            Solution S = Solution.Generate();
            Console.WriteLine($"[SOLUTION] Total generated slides: {S.Slideshow.Slides.Count}");
            Console.WriteLine($"{DateTime.Now} Initial solution score: {S.Score}");
            Solution H = S.Copy();
            Solution Best = H.Copy();

            while (!RunDurationReached(startTime))
            {
                DateTime startTimeInner = DateTime.Now;
                int time = T[rnd.Next(T.Count)];

                while (!RunDurationReached(startTime) && !SearchSpaceExplorationTimeLimitReached(startTimeInner))
                {
                    var R = S.Copy();
                    R.Mutate();
                    var random = rnd.Next(1, 6);
                    if (S.Score < R.Score)
                    {
                        Console.WriteLine("New score: " + R.Score);
                        S = R;
                    }
                }

                if (S.Score > Best.Score)
                {
                    Best = S;
                }

                H = NewHomeBase(H, S);
                S = H.Copy();
                S.Perturb();
            }

            return Best;
        }
        //{
        //    timeWithoutProgress = Stopwatch.StartNew();
        //    executionTime = Stopwatch.StartNew();
        //    acceptWorseSolution = Stopwatch.StartNew();

        //    var score = initialScore;
        //    var highestScore = initialScore;
        //    do
        //    {
        //        var gainFromOperator = Mutate(random.Next(1, 11));
        //        if (randomOperator <= ConfigurationConsts.SlideSwapUpperFrequency)
        //        {
        //            gainFromOperator = Solution.SwapSlidesMutation(slideshow);
        //        }

        //        else if (randomOperator > ConfigurationConsts.VerticalPhotoSwapFrequencyLowerLimit && randomOperator <= ConfigurationConsts.VerticalPhotoSwapFrequencyUpperLimit)
        //        {
        //            gainFromOperator = Solution.VerticalSlidePhotoSwapMutation(slideshow, verticalSlides);
        //        }

        //        else if (randomOperator > ConfigurationConsts.ShuffleOperatorFrequency)
        //        {
        //            gainFromOperator = Solution.ShuffleSlidesMutation(slideshow, random.Next(4, 8));
        //        }

        //        if (gainFromOperator < 0)
        //        {
        //            ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis += 25;
        //            acceptWorseSolution.Restart();
        //            timeWithoutProgress.Restart();
        //        }

        //        if (ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis > 5000)
        //        {
        //            ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis = 1200;
        //        }


        //        score += gainFromOperator;
        //        if (gainFromOperator > 0)
        //        {
        //            highestScore = score;
        //            timeWithoutProgress.Restart();
        //            Console.WriteLine("NEW SCORE: " + score);
        //        }
        //    }
        //    while (executionTime.ElapsedMilliseconds < ConfigurationConsts.RunDuration);

        public Solution NewHomeBase(Solution H, Solution S)
        {
            return S.Score >= H.Score ? S : H;
        }

        private bool RunDurationReached(DateTime startTime, int totalExecutionMinutes = 5)
        {
            return (int)DateTime.Now.Subtract(startTime).TotalMinutes > totalExecutionMinutes;
        }

        private bool SearchSpaceExplorationTimeLimitReached(DateTime startTime, int totalExecutionSeconds = 20)
        {
            return (int)DateTime.Now.Subtract(startTime).TotalSeconds > totalExecutionSeconds;
        }
    }
}
