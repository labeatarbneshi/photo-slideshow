using PhotoSlideshow;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_Slideshow.Operators
{
    public class Swap
    {
        private static readonly Random randomNoGenerator = new Random();

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
    }
}
