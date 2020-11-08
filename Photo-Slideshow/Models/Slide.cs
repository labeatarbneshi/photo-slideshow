using System.Collections.Generic;
using System.Linq;

namespace PhotoSlideshow.Models
{
    public class Slide
    {
        public int Id { get; set; }
        public List<Photo> Photos { get; set; }
        public List<int> BadNeighbours { get; set; } = new List<int>();
        public List<string> GetTags()
        {
            if(Photos.Count == 2)
            {
                return new List<string>(Photos[0].Tags.Union(Photos[1].Tags));
            }

            return Photos[0].Tags;
        }

        /// <summary>
        /// Copies a slide
        /// </summary>
        /// <param name="slide"></param>
        /// <returns>A copy of a given slide</returns>
        public static Slide Copy(Slide slide)
        {
            List<Photo> photos = new List<Photo>(slide.Photos);

            return new Slide() { Id = slide.Id, Photos = photos };
        }

        /// <summary>
        /// Evaluate score between two slides
        /// </summary>
        /// <param name="firstSlideTags">First slide</param>
        /// <param name="secondSlideTags">Second slide</param>
        /// <returns></returns>
        public static int EvaluateAdjacent(List<string> firstSlideTags, List<string> secondSlideTags)
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
        /// Swaps position of two slides
        /// </summary>
        /// <param name="slides">List of slides</param>
        /// <param name="firstSlideIndex">First slide index</param>
        /// <param name="secondSlideIndex">Second slide index</param>
        /// <returns>List of slides with new positions</returns>
        public static List<Slide> Swap(List<Slide> slides, int firstSlideIndex, int secondSlideIndex)
        {
            var tmp = slides[firstSlideIndex];
            slides[firstSlideIndex] = slides[secondSlideIndex];
            slides[secondSlideIndex] = tmp;

            return slides;
        }
    }
}
