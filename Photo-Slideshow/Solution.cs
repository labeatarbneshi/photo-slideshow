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
            
            if(VerticalPhotos.Count % 2 != 0)
            {
                //REMOVE ONE PHOTO FROM VERTICAL PHOTOS LIST
                RemovePhotoFromVerticalPhotosList(VerticalPhotoRemovalPolicy.RandomIndex);
            }

            List<Slide> verticalPhotoSlides = GenerateSlidesWithVerticalPhotos();
            List<Slide> horizontalPhotoSlides = new List<Slide>();
            foreach(var photo in HorizontalPhotos)
            {
                horizontalPhotoSlides.Add(new Slide()
                {
                    Photos = new List<Photo>()
                    {
                        photo
                    }
                });
            }

            var a = CalculateScore(verticalPhotoSlides.Concat(horizontalPhotoSlides).ToList());
        }

        public List<Slide> GenerateSlidesWithVerticalPhotos()
        {
            List<Slide> slidesWithVerticalPhotos = new List<Slide>();
            List<Photo> verticalPhotos = new List<Photo>(VerticalPhotos);
            
            int minimumNumberOfTags = VerticalPhotos.Min(photo => photo.NumberOfTags);
            int maximumNumberOfTags = VerticalPhotos.Max(photo => photo.NumberOfTags);

            int minimumNumberOfTagsPerSlideThreshold = minimumNumberOfTags;
            int maximumNUmberOfTagsPerSlideThreshold = maximumNumberOfTags;

            if (minimumNumberOfTags != maximumNumberOfTags) {
                minimumNumberOfTagsPerSlideThreshold = maximumNumberOfTags - minimumNumberOfTags;
                maximumNUmberOfTagsPerSlideThreshold = maximumNumberOfTags + minimumNumberOfTags;
            }


            for(int i = 0; i<verticalPhotos.Count;i++)
            {

            }

            while(verticalPhotos.Count > 0)
            {
                //SELECT INITAL PHOTO FOR SLIDE
                int index = random.Next(0, verticalPhotos.Count);
                var selectedPhoto = verticalPhotos[index];
                verticalPhotos.RemoveAt(index);

                //SELECT PHOTO TO ADD TO SLIDE BEING CREATED
                bool shouldContinueSearching = true;

                while(shouldContinueSearching)
                {
                    int randomIndex = random.Next(0, verticalPhotos.Count);
                    var secondSlidePhoto = verticalPhotos[randomIndex];
                    if (selectedPhoto.NumberOfTags + secondSlidePhoto.NumberOfTags < maximumNUmberOfTagsPerSlideThreshold
                        || selectedPhoto.NumberOfTags + secondSlidePhoto.NumberOfTags > minimumNumberOfTagsPerSlideThreshold)
                    {
                        shouldContinueSearching = false;
                        verticalPhotos.RemoveAt(randomIndex);
                        slidesWithVerticalPhotos.Add(new Slide()
                        {
                            Photos = new List<Photo>()
                            {
                                selectedPhoto,
                                secondSlidePhoto
                            }
                        });
                    }
                }
                

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
                score += CalculateAdjacentSlidesScore(slides[i].GetSlideTags(), slides[i + 1].GetSlideTags());
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
    }
}
