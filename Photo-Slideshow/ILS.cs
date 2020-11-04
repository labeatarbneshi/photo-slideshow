using PhotoSlideshow.Operators;
using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PhotoSlideshow.Configuration;

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
            bool paramsChanged = false;

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
                    var verticalSwap = Swap.SwapVerticalSlidePhotos(slideshow, verticalSlides, acceptWorseSolution, timeWithoutProgress);
                    gainFromOperator = verticalSwap.Score;


                    //if (verticalSwap.Score < 0)
                    //{
                    //    var hardSwapWithFirstIndex = Swap.HardSwap(slideshow, verticalSwap.FirstIndex, ConfigurationConsts.RetriesAfterBadVerticalSwap);
                    //    gainFromOperator += hardSwapWithFirstIndex;

                    //    var hardSwapWithSecondIndex = Swap.HardSwap(slideshow, verticalSwap.SecondIndex, ConfigurationConsts.RetriesAfterBadVerticalSwap);
                    //    gainFromOperator += hardSwapWithSecondIndex;

                    //    if (hardSwapWithFirstIndex + hardSwapWithSecondIndex > 0)
                    //    {
                    //        timeWithoutProgress.Restart();
                    //    }
                    //    acceptWorseSolution.Restart();
                    //}
                }

                else if (randomOperator > ConfigurationConsts.ShuffleOperatorFrequency)
                {
                    gainFromOperator = Shuffle.ShuffleSlides(slideshow, random.Next(4, 8));
                }
                    
                if(gainFromOperator < 0)
                {
                    acceptWorseSolution.Restart();
                }

                score += gainFromOperator;
                if (gainFromOperator > 0)
                {
                    highestScore = score;
                    timeWithoutProgress.Restart();
                    Console.WriteLine("NEW SCORE: " + score);
                }

                //if (executionTime.ElapsedMilliseconds > 150000 && !paramsChanged)
                //{
                //    paramsChanged = true;
                //    ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis = 3000;
                //    ConfigurationConsts.RetriesAfterBadVerticalSwap = 20;
                //    ConfigurationConsts.AcceptWorseSolutionAfterMillis = 5000;
                //}

                //if (executionTime.ElapsedMilliseconds > 300000)
                //{
                //    ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis = 7000;
                //    ConfigurationConsts.RetriesAfterBadVerticalSwap = 25;
                //    ConfigurationConsts.SlideSwapUpperFrequency = 5;
                //    ConfigurationConsts.VerticalPhotoSwapFrequencyLowerLimit = 5;
                //    ConfigurationConsts.VerticalPhotoSwapFrequencyUpperLimit = 10;
                //}
            }
            while (executionTime.ElapsedMilliseconds < ConfigurationConsts.RunDuration);

            Common.SaveSolution(slideshow, "final_solution");
        }
    }
}
