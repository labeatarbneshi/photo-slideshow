using Photo_Slideshow.Operators;
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
                    var a = SwapVerticalSlidePhotos();
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
                    var shuffleScore = ShuffleSlides(random.Next(4, 12));
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

        /// <summary>
        /// Randomly selects two slides with vertical photos and generates all slides from given photos by calculating score.
        /// </summary>
        /// <returns>The highest score from combination of photos</returns>
        public VerticalSwap SwapVerticalSlidePhotos()
        {
            var firstSlideIndex = random.Next(0, verticalSlides.Count);

            int secondSlideIndex;
            do
            {
                secondSlideIndex = random.Next(0, verticalSlides.Count);
            } while (firstSlideIndex == secondSlideIndex);

            Slide firstSlide = slideshow.Slides.Find(s => s.Id == verticalSlides[firstSlideIndex].Id);
            Slide secondSlide = slideshow.Slides.Find(s => s.Id == verticalSlides[secondSlideIndex].Id);


            if (firstSlide == null || secondSlide == null)
            {
                return new VerticalSwap() { Score = 0 };
            }

            var firstSlideIndexInSlideshow = slideshow.Slides.FindIndex(s => s.Id == firstSlide.Id);
            var secondSlideIndexInSlideshow = slideshow.Slides.FindIndex(s => s.Id == secondSlide.Id);

            var omittedSlide = -1;
            if (secondSlideIndexInSlideshow - 1 == firstSlideIndexInSlideshow)
            {
                omittedSlide = 1;
            }
            else if (secondSlideIndexInSlideshow + 1 == firstSlideIndexInSlideshow)
            {
                omittedSlide = 2;
            }

            var preSwapFirstSlideScore = Common.CalculateSlideScore(slideshow, firstSlideIndexInSlideshow);
            var preSwapSecondSlideScore = Common.CalculateSlideScore(slideshow, secondSlideIndexInSlideshow, omittedSlide);
            var preSwapScore = preSwapFirstSlideScore + preSwapSecondSlideScore;

            // FIRST SWAP
            var firstPhotoSwap = SwapPhotos(CopySlide(firstSlide), CopySlide(secondSlide), 0, 0);
            slideshow.Slides[firstSlideIndexInSlideshow] = firstPhotoSwap[0];
            slideshow.Slides[secondSlideIndexInSlideshow] = firstPhotoSwap[1];

            var firstfirstPhotoSwapScore = Common.CalculateSlideScore(slideshow, firstSlideIndexInSlideshow);
            var firstSecondPhotoSwapScore = Common.CalculateSlideScore(slideshow, secondSlideIndexInSlideshow, omittedSlide);
            var postFirstSwap = firstfirstPhotoSwapScore + firstSecondPhotoSwapScore;

            // SECOND SWAP
            var secondPhotoSwap = SwapPhotos(CopySlide(firstSlide), CopySlide(secondSlide), 0, 1);
            slideshow.Slides[firstSlideIndexInSlideshow] = secondPhotoSwap[0];
            slideshow.Slides[secondSlideIndexInSlideshow] = secondPhotoSwap[1];
            var secondfirstPhotoSwapScore = Common.CalculateSlideScore(slideshow, firstSlideIndexInSlideshow);
            var secondSecondPhotoSwapScore = Common.CalculateSlideScore(slideshow, secondSlideIndexInSlideshow, omittedSlide);
            var postSecondSwap = secondfirstPhotoSwapScore + secondSecondPhotoSwapScore;

            if (postFirstSwap >= preSwapScore)
            {
                slideshow.Slides[firstSlideIndexInSlideshow] = firstPhotoSwap[0];
                slideshow.Slides[secondSlideIndexInSlideshow] = firstPhotoSwap[1];

                return new VerticalSwap() { FirstIndex = firstSlideIndexInSlideshow, SecondIndex = secondSlideIndexInSlideshow, Score = postFirstSwap - preSwapScore };
            }
            
            if(postSecondSwap >= preSwapScore)
            {
                return new VerticalSwap() { FirstIndex = firstSlideIndexInSlideshow, SecondIndex = secondSlideIndexInSlideshow, Score = postSecondSwap - preSwapScore };
            }

            if (stopwatch.ElapsedMilliseconds > 10000 && Math.Min(postFirstSwap, postSecondSwap) <= 3)
            {
                stopwatch.Restart();
                var chosenMutation = random.Next(1, 3);
                if (chosenMutation == 1)
                {
                    slideshow.Slides[firstSlideIndexInSlideshow] = firstPhotoSwap[0];
                    slideshow.Slides[secondSlideIndexInSlideshow] = firstPhotoSwap[1];
                    return new VerticalSwap() { FirstIndex = firstSlideIndexInSlideshow, SecondIndex = secondSlideIndexInSlideshow, Score = postFirstSwap - preSwapScore };

                    //return postFirstSwap - preSwapScore;
                }
                else
                {
                    return new VerticalSwap() { FirstIndex = firstSlideIndexInSlideshow, SecondIndex = secondSlideIndexInSlideshow, Score = postSecondSwap - preSwapScore };

                    //return postSecondSwap - preSwapScore;
                }
            }
            else
            {
                slideshow.Slides[firstSlideIndexInSlideshow] = firstSlide;
                slideshow.Slides[secondSlideIndexInSlideshow] = secondSlide;
                return new VerticalSwap() { Score = 0 };
            }
        }


        /// <summary>
        /// Swaps photos between two vertical slides
        /// </summary>
        /// <param name="firstSlide">First chosen slide</param>
        /// <param name="secondSlide">Second chosen slide</param>
        /// <param name="firstPhotoIndex">Index of first photo on first slide</param>
        /// <param name="secondPhotoIndex">Index of second photo on second slide</param>
        /// <returns>List with newly generated slides</returns>
        public List<Slide> SwapPhotos(Slide firstSlide, Slide secondSlide, int firstPhotoIndex, int secondPhotoIndex)
        {
            Photo temp = firstSlide.Photos[firstPhotoIndex];
            firstSlide.Photos[firstPhotoIndex] = secondSlide.Photos[secondPhotoIndex];
            secondSlide.Photos[secondPhotoIndex] = temp;

            List<Slide> slides = new List<Slide>
            {
                firstSlide,
                secondSlide
            };

            return slides;
        }

        /// <summary>
        /// Copies a slide
        /// </summary>
        /// <param name="slide"></param>
        /// <returns>A copy of a given slide</returns>
        public Slide CopySlide(Slide slide)
        {
            List<Photo> photos = new List<Photo>(slide.Photos);

            return new Slide() { Id = slide.Id, Photos = photos };
        }

        public int ShuffleSlides(int length)
        {
            var index = random.Next(0, slideshow.Slides.Count);
            if (index + length > slideshow.Slides.Count)
            {
                index = slideshow.Slides.Count - length;
            }
            List<Slide> slides = new List<Slide>();
            slides.AddRange(slideshow.Slides.GetRange(index, length));

            int preShuffleScore = Common.EvaluateSolution(slides);

            List<Slide> slidesToShuffle = new List<Slide>();
            slidesToShuffle.AddRange(slides.GetRange(1, slides.Count - 2));

            var shuffledList = slidesToShuffle.OrderBy(x => Guid.NewGuid()).ToList();
            shuffledList.Insert(0, slides[0]);
            shuffledList.Add(slides[slides.Count - 1]);

            int postShuffleScore = Common.EvaluateSolution(shuffledList);

            if (preShuffleScore > postShuffleScore)
            {
                return 0;
            }

            for (int i = index + 1, j = 1; i < index + length - 1; i++, j++)
            {
                slideshow.Slides[i] = shuffledList[j];
            }

            return postShuffleScore - preShuffleScore;
        }
    }
}
