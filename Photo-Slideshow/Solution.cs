using Photo_Slideshow;
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
        private Slideshow Slideshow;

        private readonly Random random = new Random();
        public Solution(Collection collection)
        {
            Collection = collection;
            PrepareCollection();
        }

        public void Generate()
        {
            Console.WriteLine($"[{DateTime.Now}] Generating solution...");

            List<Photo> collectionPhotos = new List<Photo>(Collection.Photos);
            List<Slide> slides = new List<Slide>();


            //Find a random horizontal photo that will be in the first slide of slideshow
            //HANDLE WHEN THERE IS NO HORIZONTAL PHOTO
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
            collectionPhotos.Remove(currentPhoto);

            while (collectionPhotos.Count > 0)
            {
                List<Photo> nextSlidePhotos = FindNextSlide(currentSlide, collectionPhotos);
                if (nextSlidePhotos != null)
                {
                    currentSlide = new Slide() { Id = nextSlidePhotos[0].Id, Photos = nextSlidePhotos };
                    slides.Add(currentSlide);

                    foreach (var photo in nextSlidePhotos)
                    {
                        collectionPhotos.Remove(photo);
                    }
                }
                else
                {
                    List<Photo> randomPhotos = new List<Photo>();
                    //Attempt to find a photo which orentiation is HORIZONTAL
                    var horizontalPhotos = collectionPhotos.FindAll(photo => photo.Orientation == Orientation.HORIZONTAL);
                    if(horizontalPhotos.Count > 0)
                    {
                        randomPhotos.Add(horizontalPhotos.First());
                    } else
                    {
                        //If there is no HORIZONTAL photo left in our collection we must find two vertical photo to consist a slide
                        randomPhotos = collectionPhotos.FindAll(p => p.Orientation == Orientation.VERTICAL).Take(2).ToList();
                    }

                    currentSlide = new Slide()
                    {
                        Id = randomPhotos[0].Id,
                        Photos = randomPhotos
                    };
                    slides.Add(currentSlide);
                    foreach (var photo in randomPhotos)
                    {
                        collectionPhotos.Remove(photo);
                    }
                }
            }

            int score = Common.EvaluateSolution(slides);

            Slideshow = new Slideshow(slides, score);

            Console.WriteLine($"[SOLUTION] Total generated slides: {slides.Count}");
            Console.WriteLine($"{DateTime.Now} Initial solution score: {score}");

            Console.WriteLine($"[ILS] Optimizing solution...");
            
            ILS ils = new ILS(CopySolution(Slideshow));
            ils.SwapSlides();
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
            const int searchSpacePercentage = 5;
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
            CandidatePhoto chosenPhoto = CandidateSelectionProcess(candidatePhotos);
            chosenPhotos.Add(chosenPhoto.Photo);

            //UNTIL HERE
            //CandidatePhoto chosenPhoto = candidatePhotos.OrderByDescending(photo => photo.Score).First();
            //chosenPhotos.Add(chosenPhoto.Photo);

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
            const int searchSpacePercentage = 5;
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

            //return candidatePhotos.OrderByDescending(photo => photo.Score).First().Photo;
            return CandidateSelectionProcess(candidatePhotos).Photo;
        }

        private CandidatePhoto CandidateSelectionProcess(List<CandidatePhoto> candidatePhotos)
        {
            var chosenCandidate = candidatePhotos.OrderByDescending(photo => photo.Score).First();
            
            if (candidatePhotos.Count == 1)
            {
                return chosenCandidate;
            }

            int maxScore = chosenCandidate.Score;
            List<CandidatePhoto> candidates = candidatePhotos.FindAll(photo => photo.Score == maxScore);

            if(candidates.Count == 1)
            {
                return chosenCandidate;
            }

            candidates = candidates.OrderBy(candidate => candidate.Photo.NumberOfTags).ToList();

            return candidates.First();
        }

        private Slideshow CopySolution(Slideshow slideshow)
        {
            List<Slide> slides = new List<Slide>(slideshow.Slides);
            int score = slideshow.Score;

            return new Slideshow(slides, score);
        }
    }
}