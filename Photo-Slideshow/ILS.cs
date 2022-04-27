using PhotoSlideshow.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PhotoSlideshow
{
    class ILS
    {
        public ILS()
        {
        }

        public Solution FindSolution(int runMinutes, Solution solution = null)
        {
            var stopwatch = new Stopwatch();
            DateTime startTime = DateTime.Now;
            Random rnd = new Random();
            List<int> T = new List<int>(){ 100, 120, 140 };
            Solution S = solution;
            if(solution == null)
            {
                S = Solution.GenerateRandom();
            }
            Solution H = S.Copy();
            Solution Best = H.Copy();

            while (!TimeDifferenceReached(startTime, runMinutes))
            {
                DateTime startTimeInner = DateTime.Now;
                int time = T[rnd.Next(T.Count)];

                while (!TimeDifferenceReached(startTime, runMinutes) && !TimeDifferenceReachedSeconds(startTimeInner, time))
                {
                    var R = S.Copy();
                    R.Mutate();
                    var random = rnd.Next(1, 6);
                    if (S.Slideshow.Score < R.Slideshow.Score || random == 1)
                    {
                        S = R;
                    }
                }

                Console.WriteLine($"{DateTime.Now}:  {S.Slideshow.Score}");
                if (S.Slideshow.Score > Best.Slideshow.Score)
                {
                    
                    Best = S;
                }

                H = NewHomeBase(H, S);
                S = H.Copy();
                S.Perturb();
            }

            return Best;
        }

        /// <summary>
        /// Starts solution optimizing by choosing operators randomly
        /// </summary>
        public static Solution OptimizeWithoutPerturb(Solution solution, int runSeconds = 10)
        {
            var stop = Stopwatch.StartNew();
            var score = solution.Slideshow.Score;
            do
            {
                var mutationGain = solution.Mutate();

                score += mutationGain;
            }
            while (stop.ElapsedMilliseconds < runSeconds * 1000);

            return solution;
        }

        public Solution NewHomeBase(Solution H, Solution S)
        {
            return S.Slideshow.Score >= H.Slideshow.Score ? S : H;
        }

        private bool TimeDifferenceReached(DateTime startTime, int totalRunDuration = 1)
        {
            return (int)DateTime.Now.Subtract(startTime).TotalMinutes > totalRunDuration;
        }

        private bool TimeDifferenceReachedSeconds(DateTime startTime, int totalExecutionSeconds = 60)
        {
            return (int)DateTime.Now.Subtract(startTime).TotalSeconds > totalExecutionSeconds;
        }


        public Solution OptimizeV2(Solution solution = null)
        {
            Random rnd = new Random();
            List<int> T = new List<int>() { 180, 360};
            Solution S = solution;
            if (solution == null)
            {
                S = Solution.GenerateRandom();
            }
            var stop = Stopwatch.StartNew();
            var timeWithoutProgress = Stopwatch.StartNew();
            var score = S.Slideshow.Score;
            var time = T[rnd.Next(T.Count)] * 1000;
            do
            {
                var mutationGain = S.Mutate();

                if(mutationGain > 0)
                {
                    Console.WriteLine("NS: " + score);
                    timeWithoutProgress.Restart();
                }

                score += mutationGain;

                if(timeWithoutProgress.ElapsedMilliseconds > time)
                {
                    S.Perturb();
                    score = S.Slideshow.Score;
                    time = T[rnd.Next(T.Count)] * 1000;
                    timeWithoutProgress.Restart();
                }
            }
            while (stop.ElapsedMilliseconds < 999999);

            return S;
        }
    }
}
