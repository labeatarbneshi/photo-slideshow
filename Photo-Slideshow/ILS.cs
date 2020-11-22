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
        private readonly Slideshow slideshow;
        private readonly List<Slide> verticalSlides;
        private readonly int initialScore;
        private readonly Random random = new Random();
        private Stopwatch timeWithoutProgress;
        private Stopwatch executionTime;
        private Stopwatch acceptWorseSolution;
        public ILS(Slideshow slideshow)
        {
            this.slideshow = slideshow;
            verticalSlides = slideshow.Slides.Where(s => s.Photos.Any(p => p.Orientation == Orientation.VERTICAL)).ToList();
            initialScore = slideshow.Score;
        }

        /// <summary>
        /// Starts solution optimizing by choosing operators randomly
        /// </summary>
        public void Optimize() { }
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

        //    Solution.Save(slideshow, "final_solution");

        //public int Mutate(int chosenOperator)
        //{
        //    if (chosenOperator <= ConfigurationConsts.SlideSwapUpperFrequency)
        //    {
        //        return Solution.SwapSlidesMutation(slideshow);
        //    }

        //    else if (chosenOperator > ConfigurationConsts.VerticalPhotoSwapFrequencyLowerLimit && chosenOperator <= ConfigurationConsts.VerticalPhotoSwapFrequencyUpperLimit)
        //    {
        //        return Solution.VerticalSlidePhotoSwapMutation(slideshow, verticalSlides);
        //    }

        //    return Solution.ShuffleSlidesMutation(slideshow, random.Next(4, 8));
        //}
    }
}
