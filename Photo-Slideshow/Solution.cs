using Photo_Slideshow.Enums;
using Photo_Slideshow.Models;
using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_Slideshow
{
    class Solution
    {
        private readonly Collection Collection;
        private List<Photo> VerticalPhotos;
        private List<Photo> HorizontalPhotos;

        private readonly Random random = new Random();
        public Solution(Collection collection)
        {
            Collection = collection;
        }

        public void Generate()
        {
            //FILTERING PHOTOS BY ORIENTATION
            FilterVerticalPhotos();
            FilterHorizontalPhotos();

            if (VerticalPhotos.Count % 2 != 0)
            {
                //REMOVE ONE PHOTO FROM VERTICAL PHOTOS LIST
                RemovePhotoFromVerticalPhotosList(VerticalPhotoRemovalPolicy.RandomIndex);
            }

            List<Slide> horizontalPhotoSlides = GenerateSlidesFromPhotos(HorizontalPhotos);
            List<Slide> verticalPhotoSlides = GenerateSlidesWithVerticalPhotos();

            List<Slide> completeSlides = horizontalPhotoSlides.Concat(verticalPhotoSlides).ToList();

            List<Slide> slides = new List<Slide>();
            while (completeSlides.Count > 0)
            {
                var currentSlide = completeSlides[random.Next(0, completeSlides.Count)];
                var nextSlide = FindCandidateAdjacentSlide(completeSlides, currentSlide, 20, 3);

                completeSlides.Remove(currentSlide);

                if (nextSlide != null)
                {
                    slides.Add(currentSlide);
                    slides.Add(nextSlide);
                }
            }


            var a = CalculateScore(slides);
        }

        public List<Slide> GenerateSlidesWithVerticalPhotos()
        {
            List<Slide> slidesWithVerticalPhotos = new List<Slide>();
            List<Photo> verticalPhotos = new List<Photo>(VerticalPhotos);

            List<Photo> orderedByNoOfTags = verticalPhotos.OrderBy(photos => photos.NumberOfTags).ToList();

            while (orderedByNoOfTags.Count > 0)
            {
                slidesWithVerticalPhotos.Add(new Slide()
                {
                    Photos = new List<Photo>()
                    {
                        orderedByNoOfTags[0],
                        orderedByNoOfTags[orderedByNoOfTags.Count - 1]
                    }
                });

                orderedByNoOfTags.RemoveAt(0);
                orderedByNoOfTags.RemoveAt(orderedByNoOfTags.Count -1);
            }

            return slidesWithVerticalPhotos;
        }

        private void FilterVerticalPhotos()
        {
            VerticalPhotos = new List<Photo>(Collection.Photos.Where(photo => photo.Orientation == Orientation.VERTICAL).ToList());
        }

        private void FilterHorizontalPhotos()
        {
            HorizontalPhotos = new List<Photo>(Collection.Photos.Where(photo => photo.Orientation == Orientation.HORIZONTAL).ToList());
        }

        private void RemovePhotoFromVerticalPhotosList(VerticalPhotoRemovalPolicy removalPolicy)
        {
            switch (removalPolicy)
            {
                case VerticalPhotoRemovalPolicy.FewestTags:
                    {
                        VerticalPhotos.Remove(VerticalPhotos.OrderBy(photo => photo.NumberOfTags).First());
                        break;
                    }
                case VerticalPhotoRemovalPolicy.MostTags:
                    {
                        VerticalPhotos.Remove(VerticalPhotos.OrderBy(photo => photo.NumberOfTags).Last());
                        break;
                    }
                case VerticalPhotoRemovalPolicy.RandomIndex:
                    {
                        VerticalPhotos.RemoveAt(random.Next(0, VerticalPhotos.Count));
                        break;
                    }
            }
        }
 
        private int CalculateScore(List<Slide> slides)
        {
            int score = 0;
            
            if(slides.Count == 1)
            {
                return 1;
            }

            for(int i = 0; i < slides.Count - 1; i++)
            {
                score += CalculateAdjacentSlidesScore(slides[i].GetTags(), slides[i + 1].GetTags());
            }


            return score;
        }

        private int CalculateAdjacentSlidesScore(List<string> firstSlideTags, List<string> secondSlideTags)
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

        private Slide FindCandidateAdjacentSlide(List<Slide> slides, Slide currentSlide, double percentageOfCandidatePhotos, int noOfIterations)
        {
            int photosToConsider = (int)Math.Floor(percentageOfCandidatePhotos * VerticalPhotos.Count / 100);

            List<Tuple<Slide, int>> scores = new List<Tuple<Slide, int>>();

            for (int i = 0; i < noOfIterations; i++)
            {
                int startingIndex = random.Next(0, slides.Count - photosToConsider < 0 ? slides.Count : slides.Count - photosToConsider);
                List<Slide> candidateSlides = new List<Slide>();

                if(slides.Count - startingIndex < photosToConsider)
                {
                    photosToConsider = slides.Count - startingIndex;
                }

                candidateSlides.AddRange(slides.GetRange(startingIndex, photosToConsider));

                foreach (var candidateSlide in candidateSlides)
                {
                    int score = CalculateAdjacentSlidesScore(currentSlide.GetTags(), candidateSlide.GetTags());
                    if (score > 0)
                    {
                        scores.Add(new Tuple<Slide, int>(candidateSlide, score));
                    }
                }

                if(scores.Count != 0) 
                {
                    break;
                }
            }

            if(scores.Count == 0)
            {
                return null;
            }

            Slide chosenSlide = scores.OrderByDescending(x => x.Item2).First().Item1;
            slides.Remove(chosenSlide);
            
            return chosenSlide;
        }
    
        private List<Slide> GenerateSlidesFromPhotos(List<Photo> photos)
        {
            List<Slide> slides = new List<Slide>();
            foreach(var photo in photos)
            {
                slides.Add(new Slide()
                {
                    Photos = new List<Photo>() { photo }
                });
            }

            return slides;
        }
    }
}
