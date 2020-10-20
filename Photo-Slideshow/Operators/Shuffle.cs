using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhotoSlideshow.Operators
{
    public class Shuffle
    {
        static readonly Random randomNoGenerator = new Random();
        public static int ShuffleSlides(Slideshow slideshow, int length)
        {
            var index = randomNoGenerator.Next(0, slideshow.Slides.Count);
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
