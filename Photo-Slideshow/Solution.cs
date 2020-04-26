using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhotoSlideshow
{
    class Solution
    {
        private readonly Collection Collection;
        private List<Photo> HorizontalPhotos;
        private List<Photo> VerticalPhotos;

        private Random random = new Random();
        public Solution(Collection collection)
        {
            Collection = collection;
            PrepareCollection();
        }

        public void Generate()
        {
            Console.WriteLine("Generating solution...");

            List<Photo> collectionPhotos = new List<Photo>(Collection.Photos);
            List<Slide> slides = new List<Slide>();


            //Find a random horizontal photo that will be in the first slide of slideshow
            var currentPhoto = HorizontalPhotos[random.Next(0, HorizontalPhotos.Count)];
            var currentSlide = new Slide()
            {
                Id = currentPhoto.Id,
                Photos = new List<Photo>()
                {
                    currentPhoto
                }
            };
            slides.Add(currentSlide);
            Console.WriteLine($"Adding initial slide with id: {currentSlide.Id} with photos {currentPhoto.Id} and orientation {currentPhoto.Orientation}");
            collectionPhotos.Remove(currentPhoto);

            while (collectionPhotos.Count > 0)
            {
                List<Photo> nextSlidePhotos = FindNextSlide(currentSlide, collectionPhotos);
                if (nextSlidePhotos != null)
                {
                    currentSlide = new Slide() { Id = nextSlidePhotos[0].Id, Photos = nextSlidePhotos };
                    if (currentSlide.Id == 1001)
                    {
                        Console.WriteLine("TEST");
                    }

                    slides.Add(currentSlide);
                    if (currentSlide.Photos.Count > 1)
                    {
                        Console.WriteLine($"Adding slide with id: {currentSlide.Id} with photos [{nextSlidePhotos[0].Id}, {nextSlidePhotos[1].Id}] and orientation {nextSlidePhotos[0].Orientation}");
                    }
                    else
                    {

                        Console.WriteLine($"Adding slide with id: {currentSlide.Id} with photos [{nextSlidePhotos[0].Id}] and orientation {nextSlidePhotos[0].Orientation}");
                    }


                    foreach (var photo in nextSlidePhotos)
                    {
                        collectionPhotos.Remove(photo);
                    }
                }
                else
                {
                    //slides.Add(currentSlide);
                    //Console.WriteLine($"Adding slide with id: {currentSlide.Id} with photos {currentSlide.Photos.Count} and orientation {currentSlide.Photos[0].Orientation}");
                    var randomPhoto = collectionPhotos.First(photo => photo.Orientation == Orientation.HORIZONTAL);
                    currentSlide = new Slide()
                    {
                        Id = randomPhoto.Id,
                        Photos = new List<Photo>()
                        {
                            randomPhoto
                        }
                    };
                    slides.Add(currentSlide);
                    Console.WriteLine($"Adding current slide with id: {currentSlide.Id} with photos {randomPhoto.Id} and orientation {randomPhoto.Orientation}");

                    collectionPhotos.Remove(randomPhoto);
                }
            }

            var result = slides.GroupBy(p => p.Id)
                   .Select(grp => grp.First())
                   .ToList();

            int score = Common.EvaluateSolution(slides);
            Console.WriteLine($"[SOLUTION] Total generated slides: {slides.Count}");
            Console.WriteLine($"Initial solution score: {score}");
        }

        /// <summary>
        /// Prepares collection by separating photos by orientation
        /// </summary>
        private void PrepareCollection()
        {
            //FILTERING PHOTOS BY ORIENTATION
            HorizontalPhotos = new List<Photo>(Collection.Photos.Where(photo => photo.Orientation == Orientation.HORIZONTAL).ToList());
            Console.WriteLine($"[COLLECTION INFO] Total number of horizontal photos: {HorizontalPhotos.Count}");
            VerticalPhotos = new List<Photo>(Collection.Photos.Where(photo => photo.Orientation == Orientation.VERTICAL).ToList());
            Console.WriteLine($"[COLLECTION INFO] Total number of vertical photos: {VerticalPhotos.Count}");

            //IF NUMBER OF VERTICAL PHOTOS IS ODD WE SHOULD REMOVE ONE FROM LIST
            if (VerticalPhotos.Count % 2 != 0)
            {
                VerticalPhotos.Remove(VerticalPhotos.OrderBy(photo => photo.NumberOfTags).First());
            }
        }

        private List<Photo> FindNextSlide(Slide currentSlide, List<Photo> unselectedPhotos)
        {
            const int searchSpacePercentage = 100;
            const int noOfIterations = 3;

            int slidingWindow = Common.CalculatePhotosToConsider(searchSpacePercentage, unselectedPhotos.Count);

            List<CandidatePhoto> candidatePhotos = new List<CandidatePhoto>();

            for (int i = 0; i < noOfIterations; i++)
            {
                int startIndex = random.Next(0, unselectedPhotos.Count);
                List<Photo> searchSpacePhotos = Common.GetSearchSpacePhotos(unselectedPhotos, slidingWindow, startIndex);

                foreach (var photo in searchSpacePhotos)
                {
                    int score = Common.EvaluateAdjacentSlides(currentSlide.GetTags(), photo.Tags);

                    if (score > 0)
                    {
                        candidatePhotos.Add(new CandidatePhoto()
                        {
                            Id = photo.Id,
                            Photo = photo,
                            IsUsed = false,
                            Score = score
                        });
                    }
                }

                if (candidatePhotos.Count != 0)
                {
                    break;
                }
            }

            if (candidatePhotos.Count == 0)
            {
                return null;
            }

            List<Photo> chosenPhotos = new List<Photo>();
            CandidatePhoto chosenPhoto = candidatePhotos.OrderByDescending(photo => photo.Score).Last();
            chosenPhotos.Add(chosenPhoto.Photo);

            if (chosenPhoto.Photo.Orientation == Orientation.VERTICAL)
            {
                //Find another vertical photo to add to current slide
                Photo secondVerticalPhoto = FindSecondVerticalPhotoForSlide(currentSlide, chosenPhoto.Photo, unselectedPhotos.FindAll(photo=>photo.Orientation == Orientation.VERTICAL), chosenPhoto.Score);
                if(secondVerticalPhoto != null)
                {
                    chosenPhotos.Add(secondVerticalPhoto);
                }
            }

            return chosenPhotos;
        }

        private Photo FindSecondVerticalPhotoForSlide(Slide currentSlide, Photo firstVerticalPhoto, List<Photo> unselectedPhotos, int initalScore)
        {
            const int searchSpacePercentage = 100;
            const int noOfIterations = 3;

            int slidingWindow = Common.CalculatePhotosToConsider(searchSpacePercentage, unselectedPhotos.Count);

            List<CandidatePhoto> candidatePhotos = new List<CandidatePhoto>();

            for (int i = 0; i < noOfIterations; i++)
            {
                int startIndex = random.Next(0, unselectedPhotos.Count);
                List<Photo> searchSpacePhotos = Common.GetSearchSpacePhotos(unselectedPhotos, slidingWindow, startIndex);

                foreach (var photo in searchSpacePhotos)
                {
                    int score = Common.EvaluateAdjacentSlides(currentSlide.GetTags(), photo.Tags.Union(firstVerticalPhoto.Tags).ToList());

                    if (score > initalScore)
                    {
                        candidatePhotos.Add(new CandidatePhoto()
                        {
                            Id = photo.Id,
                            Photo = photo,
                            IsUsed = false,
                            Score = score
                        });
                    }
                }

                if (candidatePhotos.Count != 0)
                {
                    break;
                }
            }

            if (candidatePhotos.Count == 0)
            {
                return unselectedPhotos.Where(p => p.Id != firstVerticalPhoto.Id).OrderBy(photo => photo.NumberOfTags).First();
            }

            return candidatePhotos.OrderByDescending(photo => photo.Score).First().Photo;
        }
    }
}