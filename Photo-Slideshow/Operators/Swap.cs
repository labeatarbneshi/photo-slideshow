using PhotoSlideshow;
using PhotoSlideshow.Configuration;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        public static int SwapSlides(Slideshow slideshow, Stopwatch acceptWorseSolution, Stopwatch timeWithoutProgress)
        {
            var firstSlideIndex = randomNoGenerator.Next(0, slideshow.Slides.Count);
            int secondSlideIndex;
            Slide firstSlide = slideshow.Slides[firstSlideIndex];
            
            bool bothHorizontal;
            do
            {
                bothHorizontal = false;
                secondSlideIndex = randomNoGenerator.Next(0, slideshow.Slides.Count);
            } while (firstSlideIndex == secondSlideIndex);

            Slide secondSlide = slideshow.Slides[secondSlideIndex];

            if (firstSlide.Photos.Count == 1 && secondSlide.Photos.Count == 1)
            {
                bothHorizontal = true;
                if (firstSlide.BadNeighbours.Contains(secondSlide.Photos[0].Id))
                {
                    return 0;
                }
            }

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

            var gain = postSwapScore - preSwapScore;

            if (bothHorizontal && postSwapScore == 0)
            {
                firstSlide.BadNeighbours.Add(secondSlide.Photos[0].Id);
                secondSlide.BadNeighbours.Add(firstSlide.Photos[0].Id);
            }

            if (postSwapScore >= preSwapScore ||
                (acceptWorseSolution.ElapsedMilliseconds > ConfigurationConsts.AcceptWorseSolutionAfterMillis &&
                timeWithoutProgress.ElapsedMilliseconds > ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis))
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
                    betterScore = true;
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
        public static int SwapVerticalSlidePhotos(Slideshow slideshow, List<Slide> verticalSlides, Stopwatch acceptWorseSolution, Stopwatch timeWithoutProgress)
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

            var preSwapFirstSlideScore = Common.CalculateSlideScore(slideshow, firstSlideIndexInSlideshow);
            var preSwapSecondSlideScore = Common.CalculateSlideScore(slideshow, secondSlideIndexInSlideshow, omittedSlide);
            var preSwapScore = preSwapFirstSlideScore + preSwapSecondSlideScore;

            VerticalPhotoSwap verticalPhotoSwap = CalculatePhotoSwapGain(
                slideshow, 
                SwapPhotos(Common.CopySlide(firstSlide), Common.CopySlide(secondSlide), randomNoGenerator.Next(0, 2), randomNoGenerator.Next(0, 2)), 
                firstSlideIndexInSlideshow, 
                secondSlideIndexInSlideshow, 
                omittedSlide);

            if (verticalPhotoSwap.Gain >= preSwapScore)
            {
                slideshow.Slides[firstSlideIndexInSlideshow] = verticalPhotoSwap.Slides[0];
                slideshow.Slides[secondSlideIndexInSlideshow] = verticalPhotoSwap.Slides[1];

                return verticalPhotoSwap.Gain - preSwapScore;
            }

            if (acceptWorseSolution.ElapsedMilliseconds > ConfigurationConsts.AcceptWorseSolutionAfterMillis &&
                timeWithoutProgress.ElapsedMilliseconds > ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis)
            {
                slideshow.Slides[firstSlideIndexInSlideshow] = verticalPhotoSwap.Slides[0];
                slideshow.Slides[secondSlideIndexInSlideshow] = verticalPhotoSwap.Slides[1];

                var hardSwapWithFirstIndex = HardSwap(slideshow, firstSlideIndexInSlideshow, ConfigurationConsts.RetriesAfterBadVerticalSwap);

                var hardSwapWithSecondIndex = HardSwap(slideshow, secondSlideIndexInSlideshow, ConfigurationConsts.RetriesAfterBadVerticalSwap);

                return verticalPhotoSwap.Gain + hardSwapWithFirstIndex + hardSwapWithSecondIndex - preSwapScore;
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

        public static VerticalPhotoSwap CalculatePhotoSwapGain(Slideshow slideshow, List<Slide> swappedSlidePhotos, int firstIndex, int secondIndex, int omittedSlide)
        {
            slideshow.Slides[firstIndex] = swappedSlidePhotos[0];
            slideshow.Slides[secondIndex] = swappedSlidePhotos[1];

            var firstSwapScore = Common.CalculateSlideScore(slideshow, firstIndex);
            var secondSwapScore = Common.CalculateSlideScore(slideshow, secondIndex, omittedSlide);

            return new VerticalPhotoSwap(swappedSlidePhotos, firstSwapScore + secondSwapScore);
        }

#endregion
    }
}
