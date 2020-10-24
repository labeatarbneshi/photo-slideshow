using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhotoSlideshow
{
    public class Common
    {
        /// <summary>
        /// Evaluate score between two slides
        /// </summary>
        /// <param name="firstSlideTags">First slide</param>
        /// <param name="secondSlideTags">Second slide</param>
        /// <returns></returns>
        public static int EvaluateAdjacentSlides(List<string> firstSlideTags, List<string> secondSlideTags)
        {
            List<int> scores = new List<int>
            {
                //COMMON TAGS
                firstSlideTags.Intersect(secondSlideTags).ToList().Count,
                //TAGS IN SLIDE 1 BUT NOT IN SLIDE 2
                firstSlideTags.Except(secondSlideTags).ToList().Count,
                //TAGS IN SLIDE 2 BUT NOT IN SLIDE 1
                secondSlideTags.Except(firstSlideTags).ToList().Count
            };

            return scores.Min();
        }

        /// <summary>
        /// Evaluate solution score
        /// </summary>
        /// <param name="slides">List of slides that should be evaluated</param>
        /// <returns></returns>
        public static int EvaluateSolution(List<Slide> slides)
        {
            int score = 0;

            if (slides.Count == 1)
            {
                return 1;
            }

            for (int i = 0; i < slides.Count - 1; i++)
            {
                score += EvaluateAdjacentSlides(slides[i].GetTags(), slides[i + 1].GetTags());
            }


            return score;
        }

        /// <summary>
        /// Calculates search space for a specific slide
        /// </summary>
        /// <param name="percentageOfCandidatePhotos"></param>
        /// <param name="totalPhotos"></param>
        /// <returns></returns>
        public static int CalculatePhotosToConsider(double percentageOfCandidatePhotos, int totalPhotos, bool fastSearch = false)
        {
            if (totalPhotos <= 100)
            {
                return totalPhotos;
            }

            if (fastSearch)
            {
                return 100;
            }

            int photosToConsider = (int)Math.Floor(percentageOfCandidatePhotos * totalPhotos / 100);

            return photosToConsider == 0 ? 1 : photosToConsider;
        }

        public static List<Photo> GetSearchSpacePhotos(List<Photo> possiblePhotos, int slidingWindow, int startIndex)
        {
            if (slidingWindow > possiblePhotos.Count)
            {
                return possiblePhotos;
            }

            if (startIndex + slidingWindow > possiblePhotos.Count)
            {
                startIndex = possiblePhotos.Count - slidingWindow;
            }

            List<Photo> searchSpacePhotos = new List<Photo>();
            searchSpacePhotos.AddRange(possiblePhotos.GetRange(startIndex, slidingWindow));

            return searchSpacePhotos;
        }

        /// <summary>
        /// Calculates selected slide score with its neighbours
        /// </summary>
        /// <param name="index">Index of selected slide</param>
        /// <param name="omittedSlide">Omits slides when calculating score when selected slides are first neighbours like k and k + 1 </param>
        /// <returns>Score of slide with previous and next slide</returns>
        public static int CalculateSlideScore(Slideshow slideshow, int index, int omittedSlide = -1)
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

            return EvaluateSolution(slides);
        }

        /// <summary>
        /// Copies a slide
        /// </summary>
        /// <param name="slide"></param>
        /// <returns>A copy of a given slide</returns>
        public static Slide CopySlide(Slide slide)
        {
            List<Photo> photos = new List<Photo>(slide.Photos);

            return new Slide() { Id = slide.Id, Photos = photos };
        }
    }
}
