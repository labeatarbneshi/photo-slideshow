using PhotoSlideshow.Configuration;
using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using PhotoSlideshow.Operators;
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
        public void Optimize()
        {
            timeWithoutProgress = Stopwatch.StartNew();
            executionTime = Stopwatch.StartNew();
            acceptWorseSolution = Stopwatch.StartNew();

            var score = initialScore;
            var highestScore = initialScore;
            do
            {
                var randomOperator = random.Next(1, 11);

                var gainFromOperator = 0;
                if (randomOperator <= ConfigurationConsts.SlideSwapUpperFrequency)
                {
                    gainFromOperator = Swap.SwapSlides(slideshow, acceptWorseSolution, timeWithoutProgress);
                }

                else if (randomOperator > ConfigurationConsts.VerticalPhotoSwapFrequencyLowerLimit && randomOperator <= ConfigurationConsts.VerticalPhotoSwapFrequencyUpperLimit)
                {
                    gainFromOperator = Swap.SwapVerticalSlidePhotos(slideshow, verticalSlides, acceptWorseSolution, timeWithoutProgress);
                }

                else if (randomOperator > ConfigurationConsts.ShuffleOperatorFrequency)
                {
                    gainFromOperator = Shuffle.ShuffleSlides(slideshow, random.Next(4, 8));
                }

                if (gainFromOperator < 0)
                {
                    ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis += 25;
                    acceptWorseSolution.Restart();
                    timeWithoutProgress.Restart();
                }

                if(ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis > 5000)
                {
                    ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis = 2500;
                }



                score += gainFromOperator;
                if (gainFromOperator > 0)
                {
                    highestScore = score;
                    timeWithoutProgress.Restart();
                    Console.WriteLine("NEW SCORE: " + score);
                }
            }
            while (executionTime.ElapsedMilliseconds < ConfigurationConsts.RunDuration);

            Common.SaveSolution(slideshow, "final_solution");
        }
    }
}
