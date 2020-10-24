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
        private Stopwatch verticalSwapStopwatch;
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
            verticalSwapStopwatch = Stopwatch.StartNew();

            var score = initialScore;
            var highestScore = initialScore;
            do
            {
                var randomOperator = random.Next(1, 11);

                var gainFromOperator = 0;
                if (randomOperator <= ConfigurationConsts.SlideSwapUpperFrequency)
                {
                    gainFromOperator = Swap.SwapSlides(slideshow);
                }

                else if (randomOperator > ConfigurationConsts.VerticalPhotoSwapFrequencyLowerLimit && randomOperator <= ConfigurationConsts.VerticalPhotoSwapFrequencyUpperLimit)
                {
                    var verticalSwap = Swap.SwapVerticalSlidePhotos(slideshow, verticalSlides, timeWithoutProgress, verticalSwapStopwatch);
                    gainFromOperator = verticalSwap.Score;

                    if (verticalSwap.Score < 0)
                    {
                        var hardSwapWithFirstIndex = Swap.HardSwap(slideshow, verticalSwap.FirstIndex, ConfigurationConsts.RetriesAfterBadVerticalSwap);
                        gainFromOperator += hardSwapWithFirstIndex;

                        var hardSwapWithSecondIndex = Swap.HardSwap(slideshow, verticalSwap.SecondIndex, ConfigurationConsts.RetriesAfterBadVerticalSwap);
                        gainFromOperator += hardSwapWithSecondIndex;
                        verticalSwapStopwatch.Restart();
                    }
                }

                else if (randomOperator > ConfigurationConsts.ShuffleOperatorFrequency)
                {
                    gainFromOperator = Shuffle.ShuffleSlides(slideshow, random.Next(4, 10));
                }

                score += gainFromOperator;
                if (gainFromOperator > 0 && score > highestScore)
                {
                    highestScore = score;
                    timeWithoutProgress.Restart();
                    Console.WriteLine("NEW SCORE: " + score);
                }
            }
            while (executionTime.ElapsedMilliseconds < ConfigurationConsts.RunDuration);

            using (StreamWriter w = File.AppendText("c.txt"))
            {
                Console.WriteLine("FINISHED WITH SCORE" + score);
                w.WriteLine(slideshow.Slides.Count);
                foreach (var s in slideshow.Slides)
                {
                    if(s.Photos.Count > 1)
                    {
                        int a = s.Photos[0].Id - 2;
                        int b = s.Photos[1].Id - 2;
                        w.WriteLine(a + " " + b);
                    } else
                    {
                        w.WriteLine(s.Photos[0].Id - 2);
                    }
                }
            }
        }
    }
}
