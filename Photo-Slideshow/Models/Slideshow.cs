using System.Collections.Generic;

namespace PhotoSlideshow.Models
{
    public class Slideshow
    {
        public List<Slide> Slides { get; set; }
        public int Score { get; set; }

        public Slideshow(List<Slide> slides, int score)
        {
            Slides = slides;
            Score = score;
        }

        /// <summary>
        /// Calculates selected slide score with its neighbours
        /// </summary>
        /// <param name="index">Index of selected slide</param>
        /// <param name="omittedSlide">Omits slides when calculating score when selected slides are first neighbours like k and k + 1 </param>
        /// <returns>Score of slide with previous and next slide</returns>
        public static int CalculateScore(Slideshow slideshow, int index, int omittedSlide = -1)
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

            return Solution.Evaluate(slides);
        }
    }
}
