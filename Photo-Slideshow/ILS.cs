using PhotoSlideshow;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_Slideshow
{
    class ILS
    {
        private readonly Slideshow slideshow;
        private readonly int initialScore;
        private Random random = new Random();
        public ILS(Slideshow slideshow)
        {
            this.slideshow = slideshow;
            initialScore = slideshow.Score;
        }

        public void SwapSlides()
        {
            var score = initialScore;
            do
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
                } else if(secondSlideIndex + 1 == firstSlideIndex)
                {
                    omittedSlide = 2;
                }

                var preSwapFirstSlideScore = CalculateSlideScore(firstSlideIndex);
                var preSwapSecondSlideScore = CalculateSlideScore(secondSlideIndex, omittedSlide);
                var preSwapSlideScore = preSwapFirstSlideScore + preSwapSecondSlideScore;

                //Swap chosen slides
                SwapSlidesPosition(slideshow.Slides, firstSlideIndex, secondSlideIndex);

                var postSwapFirstSlideScore = CalculateSlideScore(firstSlideIndex);
                var postSwapSecondSlideScore = CalculateSlideScore(secondSlideIndex, omittedSlide);
                var postSwapScore = postSwapFirstSlideScore + postSwapSecondSlideScore;

                if(postSwapScore > preSwapSlideScore)
                {
                    score = score - preSwapSlideScore + postSwapScore;
                    Console.WriteLine("[ILS] NEW SCORE: " + score);

                } else
                {
                    SwapSlidesPosition(slideshow.Slides, firstSlideIndex, secondSlideIndex);
                }


            } while (true);

        }

        
        public int CalculateSlideScore(int index, int omittedSlide = -1)
        {
            var startingIndex = index - 1;
            var noOfSlides = 3;

            if(index == 0)
            {
                startingIndex = 0;
                noOfSlides = 2;
            } else if(index == slideshow.Slides.Count -1)
            {
                startingIndex = slideshow.Slides.Count - 2;
                noOfSlides = 2;
            }

            var slides = slideshow.Slides.GetRange(startingIndex, noOfSlides);
            if(omittedSlide != -1)
            {
                if (omittedSlide == 1)
                {
                    slides.RemoveAt(0);
                }else
                {
                    slides.RemoveAt(slides.Count - 1);
                }
            }
            
            return Common.EvaluateSolution(slides);
        }

        public List<Slide> SwapSlidesPosition(List<Slide> slides, int firstSlideIndex, int secondSlideIndex)
        {
            var tmp = slides[firstSlideIndex];
            slides[firstSlideIndex] = slides[secondSlideIndex];
            slides[secondSlideIndex] = tmp;

            return slides;
        }
    }
}
