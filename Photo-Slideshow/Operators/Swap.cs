using PhotoSlideshow;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace PhotoSlideshow.Operators
{
    public class Swap
    {
        private static readonly Random randomNoGenerator = new Random();

        #region SLIDE SWAP
        /// <summary>
        /// Swap two randomly selected slides
        /// </summary>
        /// <returns>Result achiveed by swap</returns>
        public static int SwapSlides(Slideshow slideshow)
        {
            var firstSlideIndex = randomNoGenerator.Next(0, slideshow.Slides.Count);
            var firstSlide = slideshow.Slides[firstSlideIndex];

            Slide secondSlide;
            int secondSlideIndex;
            bool repeat;
            do
            {
                repeat = false;
                secondSlideIndex = randomNoGenerator.Next(0, slideshow.Slides.Count);
                secondSlide = slideshow.Slides[secondSlideIndex];
                if (secondSlide.Photos.Count == 1)
                {
                    if (firstSlide.ComparedPhotos.Contains(secondSlide.Photos[0]))
                    {
                        repeat = true;
                    }
                }
            } while (firstSlideIndex == secondSlideIndex || repeat);

            firstSlide.ComparedPhotos.Add(secondSlide.Photos[0]);
            secondSlide.ComparedPhotos.Add(firstSlide.Photos[0]);

            var omittedSlide = -1;
            if (secondSlideIndex - 1 == firstSlideIndex)
            {
                omittedSlide = 1;
            }
            else if (secondSlideIndex + 1 == firstSlideIndex)
            {
                omittedSlide = 2;
            }

            var preSwapFirstSlideScore = Common.CalculateSlideScore(slideshow, firstSlideIndex);
            var preSwapSecondSlideScore = Common.CalculateSlideScore(slideshow, secondSlideIndex, omittedSlide);
            var preSwapScore = preSwapFirstSlideScore + preSwapSecondSlideScore;

            //Swap chosen slides
            SwapSlidesPosition(slideshow.Slides, firstSlideIndex, secondSlideIndex);

            var postSwapFirstSlideScore = Common.CalculateSlideScore(slideshow, firstSlideIndex);
            var postSwapSecondSlideScore = Common.CalculateSlideScore(slideshow, secondSlideIndex, omittedSlide);
            var postSwapScore = postSwapFirstSlideScore + postSwapSecondSlideScore;

            if (postSwapScore >= preSwapScore)
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
        /// Swaps position of two slides
        /// </summary>
        /// <param name="slides">List of slides</param>
        /// <param name="firstSlideIndex">First slide index</param>
        /// <param name="secondSlideIndex">Second slide index</param>
        /// <returns>List of slides with new positions</returns>
        public static List<Slide> SwapSlidesPosition(List<Slide> slides, int firstSlideIndex, int secondSlideIndex)
        {
            var tmp = slides[firstSlideIndex];
            slides[firstSlideIndex] = slides[secondSlideIndex];
            slides[secondSlideIndex] = tmp;

            return slides;
        }
        #endregion

        #region HARD SWAP
        /// <summary>
        /// Checks for swapping for slide that has had a vertical photo swap
        /// </summary>
        /// <param name="firstSlideIndex"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static int HardSwap(Slideshow slideshow, int firstSlideIndex, int iterations)
        {
            bool betterScore = false;
            int secondSlideIndex;
            int i = 0;
            int score = 0;
            do
            {
                do
                {
                    secondSlideIndex = randomNoGenerator.Next(0, slideshow.Slides.Count);

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

                var preSwapFirstSlideScore = Common.CalculateSlideScore(slideshow, firstSlideIndex);
                var preSwapSecondSlideScore = Common.CalculateSlideScore(slideshow, secondSlideIndex, omittedSlide);
                var preSwapScore = preSwapFirstSlideScore + preSwapSecondSlideScore;

                //Swap chosen slides
                SwapSlidesPosition(slideshow.Slides, firstSlideIndex, secondSlideIndex);

                var postSwapFirstSlideScore = Common.CalculateSlideScore(slideshow, firstSlideIndex);
                var postSwapSecondSlideScore = Common.CalculateSlideScore(slideshow, secondSlideIndex, omittedSlide);
                var postSwapScore = postSwapFirstSlideScore + postSwapSecondSlideScore;

                if (postSwapScore > preSwapScore)
                {
                    score = postSwapScore - preSwapScore;

                }
                else
                {
                    SwapSlidesPosition(slideshow.Slides, firstSlideIndex, secondSlideIndex);
                }

                i++;

            } while (betterScore == true && i < iterations);

            return score;
        }
        #endregion

        #region VERTICAL SWAP
        /// <summary>
        /// Randomly selects two slides with vertical photos and generates all slides from given photos by calculating score.
        /// </summary>
        /// <returns>The highest score from combination of photos</returns>
        public static VerticalSwap SwapVerticalSlidePhotos(Slideshow slideshow, List<Slide> verticalSlides, Stopwatch stopwatch)
        {
            var firstSlideIndex = randomNoGenerator.Next(0, verticalSlides.Count);

            int secondSlideIndex;
            do
            {
                secondSlideIndex = randomNoGenerator.Next(0, verticalSlides.Count);
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
            var firstPhotoSwap = SwapPhotos(Common.CopySlide(firstSlide), Common.CopySlide(secondSlide), 0, 0);
            slideshow.Slides[firstSlideIndexInSlideshow] = firstPhotoSwap[0];
            slideshow.Slides[secondSlideIndexInSlideshow] = firstPhotoSwap[1];

            var firstfirstPhotoSwapScore = Common.CalculateSlideScore(slideshow, firstSlideIndexInSlideshow);
            var firstSecondPhotoSwapScore = Common.CalculateSlideScore(slideshow, secondSlideIndexInSlideshow, omittedSlide);
            var postFirstSwap = firstfirstPhotoSwapScore + firstSecondPhotoSwapScore;

            // SECOND SWAP
            var secondPhotoSwap = SwapPhotos(Common.CopySlide(firstSlide), Common.CopySlide(secondSlide), 0, 1);
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

            if (postSecondSwap >= preSwapScore)
            {
                return new VerticalSwap() { FirstIndex = firstSlideIndexInSlideshow, SecondIndex = secondSlideIndexInSlideshow, Score = postSecondSwap - preSwapScore };
            }

            if (stopwatch.ElapsedMilliseconds > 10000 && Math.Min(postFirstSwap, postSecondSwap) <= 3)
            {
                stopwatch.Restart();
                var chosenMutation = randomNoGenerator.Next(1, 3);
                if (chosenMutation == 1)
                {
                    slideshow.Slides[firstSlideIndexInSlideshow] = firstPhotoSwap[0];
                    slideshow.Slides[secondSlideIndexInSlideshow] = firstPhotoSwap[1];
                    return new VerticalSwap() { FirstIndex = firstSlideIndexInSlideshow, SecondIndex = secondSlideIndexInSlideshow, Score = postFirstSwap - preSwapScore };
                }
                else
                {
                    return new VerticalSwap() { FirstIndex = firstSlideIndexInSlideshow, SecondIndex = secondSlideIndexInSlideshow, Score = postSecondSwap - preSwapScore };
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
        public static List<Slide> SwapPhotos(Slide firstSlide, Slide secondSlide, int firstPhotoIndex, int secondPhotoIndex)
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

#endregion
    }
}
