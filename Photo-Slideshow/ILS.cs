using PhotoSlideshow.Operators;
using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Photo_Slideshow.Configuration;

namespace PhotoSlideshow
{
    class ILS
    {
        private readonly Slideshow slideshow;
        private readonly List<Slide> verticalSlides;
        private readonly int initialScore;
        private readonly Random random = new Random();
        private Stopwatch stopwatch;
        private Stopwatch timeWithoutProgress;
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
            stopwatch = Stopwatch.StartNew();
            timeWithoutProgress = Stopwatch.StartNew();
            var score = initialScore;
            do
            {
                var randomOperator = random.Next(1, 11);

                var gainFromOperator = 0;
                if (randomOperator <= ConfigurationConsts.SlideSwapUpperFrequency)
                {
                    gainFromOperator = Swap.SwapSlides(slideshow);
                }

                else if (randomOperator > ConfigurationConsts.SlideSwapUpperFrequency && randomOperator <= 7)
                {
                    var verticalSwap = Swap.SwapVerticalSlidePhotos(slideshow, verticalSlides, stopwatch);
                    gainFromOperator = verticalSwap.Score;

                    if (verticalSwap.Score < 0)
                    {
                        var hardSwapWithFirstIndex = Swap.HardSwap(slideshow, verticalSwap.FirstIndex, 30);
                        gainFromOperator += hardSwapWithFirstIndex;

                        var hardSwapWithSecondIndex = Swap.HardSwap(slideshow, verticalSwap.SecondIndex, 30);
                        gainFromOperator += hardSwapWithSecondIndex;
                    }
                }

                else if (randomOperator > 7)
                {
                    gainFromOperator = Shuffle.ShuffleSlides(slideshow, random.Next(4, 12));
                }

                score += gainFromOperator;
                if (gainFromOperator > 0)
                {
                    stopwatch.Restart();
                    Console.WriteLine("NEW SCORE: " + score);
                }
            }
            while (timeWithoutProgress.ElapsedMilliseconds < ConfigurationConsts.RunDuration);

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
