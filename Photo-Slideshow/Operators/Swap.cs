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
        public static int SwapSlides(Slideshow slideshow, Stopwatch timeWithoutProgress)
        {
            var firstSlideIndex = randomNoGenerator.Next(0, slideshow.Slides.Count);
            var firstSlide = slideshow.Slides[firstSlideIndex];

            Slide secondSlide;
            int secondSlideIndex;
            do
            {
                secondSlideIndex = randomNoGenerator.Next(0, slideshow.Slides.Count);
                secondSlide = slideshow.Slides[secondSlideIndex];
                //if (secondSlide.Photos.Count == 1)
                //{
                //    if (firstSlide.ComparedPhotos.Contains(secondSlide.Photos[0]))
                //    {
                //        repeat = true;
                //    }
                //}
            } while (firstSlideIndex == secondSlideIndex);

            //firstSlide.ComparedPhotos.Add(secondSlide.Photos[0]);
            //secondSlide.ComparedPhotos.Add(firstSlide.Photos[0]);

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

            if (postSwapScore >= preSwapScore || timeWithoutProgress.ElapsedMilliseconds > 5000)
            {
                timeWithoutProgress.Restart();
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
        public static VerticalSwap SwapVerticalSlidePhotos(Slideshow slideshow, List<Slide> verticalSlides, Stopwatch stopwatch, Stopwatch verticalSwapStopwatch)
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

            List<VerticalPhotoSwap> verticalPhotoSwaps = new List<VerticalPhotoSwap>();

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    var photoSwap = SwapPhotos(Common.CopySlide(firstSlide), Common.CopySlide(secondSlide), i, j);
                    verticalPhotoSwaps.Add(CalculatePhotoSwapGain(slideshow, photoSwap, firstSlideIndexInSlideshow, secondSlideIndexInSlideshow, omittedSlide));
                }
            }

            var filteredMoves = verticalPhotoSwaps.Where(x => x.Gain >= preSwapScore);
            VerticalPhotoSwap bestVerticalSwapMove = null;

            if (filteredMoves.Any())
            {
                bestVerticalSwapMove = filteredMoves.OrderBy(x => x.Gain).First();
            }

            if (bestVerticalSwapMove != null)
            {
                slideshow.Slides[firstSlideIndexInSlideshow] = bestVerticalSwapMove.Slides[0];
                slideshow.Slides[secondSlideIndexInSlideshow] = bestVerticalSwapMove.Slides[1];

                return new VerticalSwap() { FirstIndex = firstSlideIndexInSlideshow, SecondIndex = secondSlideIndexInSlideshow, Score = bestVerticalSwapMove.Gain - preSwapScore };
            }

            if (verticalSwapStopwatch.ElapsedMilliseconds > ConfigurationConsts.AcceptBadSolutionAfterMillis)
            {
                var lowestNegativeScore = verticalPhotoSwaps.OrderBy(x => x.Gain).First();
                slideshow.Slides[firstSlideIndexInSlideshow] = lowestNegativeScore.Slides[0];
                slideshow.Slides[secondSlideIndexInSlideshow] = lowestNegativeScore.Slides[1];
                return new VerticalSwap() { FirstIndex = firstSlideIndexInSlideshow, SecondIndex = secondSlideIndexInSlideshow, Score = lowestNegativeScore.Gain - preSwapScore };
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

        public static VerticalPhotoSwap CalculatePhotoSwapGain(Slideshow slideshow, List<Slide> swappedSlidePhotos, int firstIndex, int secondIndex, int omittedSlide)
        {
            slideshow.Slides[firstIndex] = swappedSlidePhotos[0];
            slideshow.Slides[secondIndex] = swappedSlidePhotos[1];

            var firstSwapScore = Common.CalculateSlideScore(slideshow, firstIndex);
            var secondSwapScore = Common.CalculateSlideScore(slideshow, secondIndex, omittedSlide);

            return new VerticalPhotoSwap(swappedSlidePhotos, firstSwapScore + secondSwapScore);
        }

        public static void HardSwapVerticalSlides(List<VerticalPhotoSwap> verticalPhotoSwaps, int firstIndex, int secondIndex, Slideshow slideshow)
        {
            List<VerticalPhotoSwap> photoSwaps = new List<VerticalPhotoSwap>();
            foreach(VerticalPhotoSwap swap in verticalPhotoSwaps)
            {
                slideshow.Slides[firstIndex] = swap.Slides[0];
                slideshow.Slides[secondIndex] = swap.Slides[1];

                var result = 0;
                result += HardSwap(slideshow, firstIndex, 10);
                result += HardSwap(slideshow, secondIndex, 10);
                photoSwaps.Add(new VerticalPhotoSwap(swap.Slides, result));
            }
        }

#endregion
    }
}
