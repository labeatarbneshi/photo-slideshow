using PhotoSlideshow;
using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_Slideshow
{
    class ILS
    {
        private readonly Slideshow slideshow;
        private readonly List<Slide> verticalSlides;
        private readonly int initialScore;
        private Random random = new Random();
        private Stopwatch stopwatch;
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
            var score = initialScore;
            do
            {
                var rnd = random.Next(1, 11);
                if (rnd < 9)
                {
                    var result = SwapSlides();
                    score += result;
                    if (result > 0)
                    {
                        stopwatch.Restart();
                        Console.WriteLine("NEW SCORE:" + score);
                    }

                }
                else
                {
                    var a = SwapVerticalSlidePhotos();
                    score += a;


                    if (a > 0)
                    {
                        stopwatch.Restart();
                        Console.WriteLine("NEW SCORE:" + score);
                    }
                }
            }
            while (true);
        }

        /// <summary>
        /// Swap two randomly selected slides
        /// </summary>
        /// <returns>Result achiveed by swap</returns>
        public int SwapSlides()
        {
            var firstSlideIndex = random.Next(0, slideshow.Slides.Count);

            int secondSlideIndex;
            do
            {
                secondSlideIndex = random.Next(0, slideshow.Slides.Count);
            } while (firstSlideIndex == secondSlideIndex);

            var omittedSlide = -1;
            if (secondSlideIndex - 1 == firstSlideIndex)
            {
                omittedSlide = 1;
            }
            else if (secondSlideIndex + 1 == firstSlideIndex)
            {
                omittedSlide = 2;
            }

            var preSwapFirstSlideScore = CalculateSlideScore(firstSlideIndex);
            var preSwapSecondSlideScore = CalculateSlideScore(secondSlideIndex, omittedSlide);
            var preSwapScore = preSwapFirstSlideScore + preSwapSecondSlideScore;

            //Swap chosen slides
            SwapSlidesPosition(slideshow.Slides, firstSlideIndex, secondSlideIndex);

            var postSwapFirstSlideScore = CalculateSlideScore(firstSlideIndex);
            var postSwapSecondSlideScore = CalculateSlideScore(secondSlideIndex, omittedSlide);
            var postSwapScore = postSwapFirstSlideScore + postSwapSecondSlideScore;

            if (postSwapScore > preSwapScore)
            {
                return postSwapScore - preSwapScore;

            }
            else
            {
                SwapSlidesPosition(slideshow.Slides, firstSlideIndex, secondSlideIndex);

                return 0;
            }


        }

        /// <summary>
        /// Calculates selected slide score with its neighbours
        /// </summary>
        /// <param name="index">Index of selected slide</param>
        /// <param name="omittedSlide">Omits slides when calculating score when selected slides are first neighbours like k and k + 1 </param>
        /// <returns>Score of slide with previous and next slide</returns>
        public int CalculateSlideScore(int index, int omittedSlide = -1)
        {
            var startingIndex = index - 1;
            var noOfSlides = 3;

            if (index == 0)
            {
                startingIndex = 0;
                noOfSlides = 2;
            }
            else if (index == slideshow.Slides.Count - 1)
            {
                startingIndex = slideshow.Slides.Count - 2;
                noOfSlides = 2;
            }

            var slides = slideshow.Slides.GetRange(startingIndex, noOfSlides);
            if (omittedSlide != -1)
            {
                if (omittedSlide == 1)
                {
                    slides.RemoveAt(0);
                }
                else
                {
                    slides.RemoveAt(slides.Count - 1);
                }
            }

            return Common.EvaluateSolution(slides);
        }

        /// <summary>
        /// Swaps position of two slides
        /// </summary>
        /// <param name="slides">List of slides</param>
        /// <param name="firstSlideIndex">First slide index</param>
        /// <param name="secondSlideIndex">Second slide index</param>
        /// <returns>List of slides with new positions</returns>
        public List<Slide> SwapSlidesPosition(List<Slide> slides, int firstSlideIndex, int secondSlideIndex)
        {
            var tmp = slides[firstSlideIndex];
            slides[firstSlideIndex] = slides[secondSlideIndex];
            slides[secondSlideIndex] = tmp;

            return slides;
        }

        /// <summary>
        /// Randomly selects two slides with vertical photos and generates all slides from given photos by calculating score.
        /// </summary>
        /// <returns>The highest score from combination of photos</returns>
        public int SwapVerticalSlidePhotos()
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
                return 0;
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

            var preSwapFirstSlideScore = CalculateSlideScore(firstSlideIndexInSlideshow);
            var preSwapSecondSlideScore = CalculateSlideScore(secondSlideIndexInSlideshow, omittedSlide);
            var preSwapScore = preSwapFirstSlideScore + preSwapSecondSlideScore;

            // FIRST SWAP
            var firstPhotoSwap = SwapPhotos(CopySlide(firstSlide), CopySlide(secondSlide), 0, 0);
            slideshow.Slides[firstSlideIndexInSlideshow] = firstPhotoSwap[0];
            slideshow.Slides[secondSlideIndexInSlideshow] = firstPhotoSwap[1];

            var firstfirstPhotoSwapScore = CalculateSlideScore(firstSlideIndexInSlideshow);
            var firstSecondPhotoSwapScore = CalculateSlideScore(secondSlideIndexInSlideshow, omittedSlide);
            var postFirstSwap = firstfirstPhotoSwapScore + firstSecondPhotoSwapScore;

            //// SECOND SWAP
            var secondPhotoSwap = SwapPhotos(CopySlide(firstSlide), CopySlide(secondSlide), 0, 1);
            slideshow.Slides[firstSlideIndexInSlideshow] = secondPhotoSwap[0];
            slideshow.Slides[secondSlideIndexInSlideshow] = secondPhotoSwap[1];
            var secondfirstPhotoSwapScore = CalculateSlideScore(firstSlideIndexInSlideshow);
            var secondSecondPhotoSwapScore = CalculateSlideScore(secondSlideIndexInSlideshow, omittedSlide);
            var postSecondSwap = secondfirstPhotoSwapScore + secondSecondPhotoSwapScore;

            var acceptMutationRate = random.Next(1, 101);

            if (postFirstSwap >= preSwapScore)
            {
                slideshow.Slides[firstSlideIndexInSlideshow] = firstPhotoSwap[0];
                slideshow.Slides[secondSlideIndexInSlideshow] = firstPhotoSwap[1];

                return postFirstSwap - preSwapScore;
            }
            
            if(postSecondSwap >= preSwapScore)
            {
                return postSecondSwap - preSwapScore;
            }

            if (stopwatch.ElapsedMilliseconds > 5000)
            {
                stopwatch.Restart();
                var chosenMutation = random.Next(1, 3);
                if (chosenMutation == 1)
                {
                    slideshow.Slides[firstSlideIndexInSlideshow] = firstPhotoSwap[0];
                    slideshow.Slides[secondSlideIndexInSlideshow] = firstPhotoSwap[1];

                    return postFirstSwap - preSwapScore;
                }
                else
                {
                    return postSecondSwap - preSwapScore;
                }
            }
            else
            {
                slideshow.Slides[firstSlideIndexInSlideshow] = firstSlide;
                slideshow.Slides[secondSlideIndexInSlideshow] = secondSlide;
                return 0;
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
    }
}
