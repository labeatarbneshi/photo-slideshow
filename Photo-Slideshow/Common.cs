using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhotoSlideshow
{
    public class Common
    {
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

        public static IEnumerable<List<Photo>> SplitList(List<Photo> photos, int nSize = 1000)
        {
            bool checkVerticals = false;
            if(photos.Any(x => x.Orientation == Orientation.VERTICAL))
            {
                checkVerticals = true;
            }

            for (int i = 0; i < photos.Count; i += nSize)
            {
                var chunk = photos.GetRange(i, Math.Min(nSize, photos.Count - i));
                var additonalPhotos = 0;
                while (checkVerticals && chunk.Count(x => x.Orientation == Orientation.VERTICAL) % 2 != 0)
                {
                    additonalPhotos += 1000;
                    chunk = photos.GetRange(i, Math.Min(nSize + additonalPhotos, photos.Count - i + additonalPhotos));
                }

                yield return chunk;
            }
        }
    }
}
