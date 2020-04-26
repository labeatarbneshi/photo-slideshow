﻿using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                //if (i == SlidingWindow - 1)
                //{
                //    SlidingWindowsScores.Add(score);
                //    SlidingWindow += SlidingWindow;
                //}
            }


            return score;
        }

        /// <summary>
        /// Calculates search space for a specific slide
        /// </summary>
        /// <param name="percentageOfCandidatePhotos"></param>
        /// <param name="totalPhotos"></param>
        /// <returns></returns>
        public static int CalculatePhotosToConsider(double percentageOfCandidatePhotos, int totalPhotos)
        {
            if (totalPhotos <= 100)
            {
                return totalPhotos;
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
    }
}