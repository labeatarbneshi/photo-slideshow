using PhotoSlideshow;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_Slideshow.TwoOpt
{
    public class TwoOpt
    {
        public void TwoOptAlgorithm(Slideshow slideshow)
        {
            bool locallyOptimal = false;
            int i, j; // TOUR INDEXES
            int secondCounterLimit;
            int gainExpected;
            Slide S1, S2, S3, S4;
            TwoOptMove bestMove;

            while (!locallyOptimal)
            {
                locallyOptimal = true;
                bestMove = new TwoOptMove();
                bestMove.ScoreGain = 0;

                for (int firstCounter = 1; firstCounter <= slideshow.Slides.Count - 3; firstCounter++)
                {
                    i = firstCounter;
                    S1 = slideshow.Slides[i];
                    S2 = slideshow.Slides[(i + 1) % slideshow.Slides.Count];

                    if (i == 0)
                    {
                        secondCounterLimit = slideshow.Slides.Count - 2;
                    }
                    else
                    {
                        secondCounterLimit = slideshow.Slides.Count - 1;
                    }

                    for (int secondCounter = i + 2; i <= secondCounterLimit; secondCounter++)
                    {
                        j = secondCounter;
                        S3 = slideshow.Slides[j];
                        S4 = slideshow.Slides[(j + 1) % slideshow.Slides.Count];

                        gainExpected = CalculateGain(S1, S2, S3, S4, slideshow);
                        if (gainExpected > bestMove.ScoreGain)
                        {
                            bestMove = new TwoOptMove(i, j, gainExpected);
                            locallyOptimal = false;
                        }
                    }

                }

                if (!locallyOptimal)
                {
                    MakeTwoOptMove(slideshow, bestMove.FirstTourIndex, bestMove.SecondTourIndex);
                }
            }
        }

        public int CalculateGain(Slide S1, Slide S2, Slide S3, Slide S4, Slideshow slideshow)
        {
            int s1Index = slideshow.Slides.IndexOf(S1);
            int s2Index = slideshow.Slides.IndexOf(S2);
            int s3Index = slideshow.Slides.IndexOf(S3);
            int s4Index = slideshow.Slides.IndexOf(S4);

            List<Slide> firstSegmentSlides = new List<Slide>();
            firstSegmentSlides.AddRange(slideshow.Slides.GetRange(s1Index, 4));
            int firstSegmentInitialScore = Common.EvaluateSolution(firstSegmentSlides);

            List<Slide> secondSegmentSlides = new List<Slide>();
            if(s3Index >= 747)
            {
                return 0;
            }
            int secondSegmentInitialScore = Common.EvaluateSolution(secondSegmentSlides);

            int firstSegmentScoreAfter2Opt = Common.EvaluateSolution(new List<Slide>() { slideshow.Slides[s1Index - 1], S1, S3, slideshow.Slides[s2Index + 1] });
            int secondSegmentScoreAfter2Opt = Common.EvaluateSolution(new List<Slide>() { slideshow.Slides[s3Index - 1], S2, S4, slideshow.Slides[s4Index + 1] });

            return firstSegmentInitialScore - secondSegmentInitialScore + firstSegmentScoreAfter2Opt + secondSegmentScoreAfter2Opt;
        }

        public void MakeTwoOptMove(Slideshow slideshow, int i, int j)
        {
            ReverseSegment(slideshow, (i + 1) % slideshow.Slides.Count, j);
        }

        public void ReverseSegment(Slideshow slides, int startIndex, int endIndex)
        {
            int left, right;

            var inversionSize = ((slides.Slides.Count + endIndex - startIndex + 1) % slides.Slides.Count) / 2;

            left = startIndex;
            right = endIndex;

            for (int i = 1; i <= inversionSize; i++)
            {
                Slide temp = slides.Slides[left];
                slides.Slides[left] = slides.Slides[right];
                slides.Slides[right] = temp;

                left = (left + 1) % slides.Slides.Count;
                right = (slides.Slides.Count + right - 1) % slides.Slides.Count;

            }
        }
    }
}
