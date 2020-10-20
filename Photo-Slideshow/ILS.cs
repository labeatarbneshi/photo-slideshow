using PhotoSlideshow.Operators;
using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
                var rnd = random.Next(1, 15);
                if (rnd <= 5)
                {
                    var result = Swap.SwapSlides(slideshow);
                    score += result;
                    if (result > 0)
                    {
                        stopwatch.Restart();
                        Console.WriteLine("NEW SCORE FROM HORIZONTAL SWAP: " + score);
                    }

                }

                else if (rnd > 5 && rnd <= 10)
                {
                    var a = Swap.SwapVerticalSlidePhotos(slideshow, verticalSlides, stopwatch);
                    score += a.Score;

                    if (a.Score < 0)
                    {
                        var result = Swap.HardSwap(slideshow, a.FirstIndex, 10);
                        score += result;
                        if (result > 0)
                        {
                            //timeWithoutProgress.Restart();
                            stopwatch.Restart();
                            Console.WriteLine("NEW SCORE FROM VERTICAL SWAP: " + score);
                        }

                        var result1 = Swap.HardSwap(slideshow, a.SecondIndex, 10);
                        score += result1;
                        if (result1 > 0)
                        {
                            //timeWithoutProgress.Restart();
                            stopwatch.Restart();
                            Console.WriteLine("NEW SCORE FROM VERTICAL SWAP: " + score);
                        }
                    }

                    else if (a.Score > 0)
                    {
                        //timeWithoutProgress.Restart();
                        stopwatch.Restart();
                        Console.WriteLine("NEW SCORE FROM VERTICAL SWAP: " + score);
                    }
                }

                else if (rnd > 10)
                {
                    var shuffleScore = Shuffle.ShuffleSlides(slideshow, random.Next(4, 12));
                    if (shuffleScore > 0)
                    {
                        stopwatch.Restart();
                        score += shuffleScore;
                        Console.WriteLine("NEW SCORE FROM SHUFFLE: " + score);

                    }

                }
            }
            while (timeWithoutProgress.ElapsedMilliseconds < 1800000);

            using (StreamWriter w = File.AppendText("c.txt"))
            {
                Console.WriteLine("FINISHED WITH SCORE" + initialScore);
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
