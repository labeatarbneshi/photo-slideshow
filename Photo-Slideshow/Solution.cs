using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSlideshow
{
    class Solution
    {
        private readonly Collection Collection;
        private List<Photo> HorizontalPhotos;
        private List<Photo> VerticalPhotos;
        private Slideshow Slideshow;
        public static int MaxTagNoInVerticalPhoto = 0;
        public static int MinTagNoInVerticalPhoto = 0;

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

            var splittedInstance = Common.SplitList(Collection.Photos, 30000).ToArray();

            var taskList = new List<Task<List<Slide>>>();
            foreach(var item in splittedInstance)
            {
                taskList.Add(Task<List<Slide>>.Factory.StartNew(() => FindSolutionForPartialInstance(item)));
            }


            Task.WaitAll(taskList.ToArray());

            foreach(var task in taskList)
            {
                slides.AddRange(task.Result);
            }


            //Find a random horizontal photo that will be in the first slide of slideshow
            //HANDLE WHEN THERE IS NO HORIZONTAL PHOTO

            int score = Common.EvaluateSolution(slides);

            Slideshow = new Slideshow(slides, score);

            Console.WriteLine($"[SOLUTION] Total generated slides: {slides.Count}");
            Console.WriteLine($"{DateTime.Now} Initial solution score: {score}");

            Console.WriteLine($"[ILS] Optimizing solution...");

            ILS ils = new ILS(CopySolution(Slideshow));
            ils.Optimize();
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
            int slidingWindow = Common.CalculatePhotosToConsider(1, unselectedPhotos.Count);

            List<CandidatePhoto> candidatePhotos = new List<CandidatePhoto>();

            for (int i = 0; i < 5; i++)
            {
                int startIndex = random.Next(0, unselectedPhotos.Count);
                List<Photo> searchSpacePhotos = Common.GetSearchSpacePhotos(unselectedPhotos, slidingWindow, startIndex);
                foreach (var photo in searchSpacePhotos)
                {
                    int score = Common.EvaluateAdjacentSlides(currentSlide.GetTags(), photo.Tags);
                    if (photo.Orientation == Orientation.HORIZONTAL && score == 0)
                    {
                        currentSlide.BadNeighbours.Add(photo.Id);
                    }

                    if (score > 0)
                    {
                        candidatePhotos.Add(new CandidatePhoto()
                        {
                            Id = photo.Id,
                            Photo = photo,
                            Score = score
                        });
                    }
                }
            }



            if (candidatePhotos.Count == 0)
            {
                return null;
            }

            List<Photo> chosenPhotos = new List<Photo>();
            CandidatePhoto chosenPhoto = CandidateSelectionProcess(candidatePhotos);
            chosenPhotos.Add(chosenPhoto.Photo);

            if (chosenPhoto.Photo.Orientation == Orientation.VERTICAL)
            {
                //Find another vertical photo to add to current slide
                Photo secondVerticalPhoto = FindSecondVerticalPhotoForSlide(currentSlide, chosenPhoto.Photo, unselectedPhotos.FindAll(photo => photo.Orientation == Orientation.VERTICAL), chosenPhoto.Score);
                if (secondVerticalPhoto != null)
                {
                    chosenPhotos.Add(secondVerticalPhoto);
                }
            }

            return chosenPhotos;
        }

        /// <summary>
        /// Finds another vertical photo to create a slide
        /// </summary>
        /// <param name="currentSlide"></param>
        /// <param name="firstVerticalPhoto"></param>
        /// <param name="unselectedPhotos"></param>
        /// <param name="initalScore"></param>
        /// <returns></returns>
        private Photo FindSecondVerticalPhotoForSlide(Slide currentSlide, Photo firstVerticalPhoto, List<Photo> unselectedPhotos, int initalScore)
        {

            int slidingWindow = Common.CalculatePhotosToConsider(1, unselectedPhotos.Count);

            List<CandidatePhoto> candidatePhotos = new List<CandidatePhoto>();
            List<Photo> searchSpacePhotos = new List<Photo>();
            for (int i = 0; i < 5; i++)
            {
                int startIndex = random.Next(0, unselectedPhotos.Count);
                searchSpacePhotos = Common.GetSearchSpacePhotos(unselectedPhotos, slidingWindow, startIndex);

                foreach (var photo in searchSpacePhotos)
                {
                    int score = Common.EvaluateAdjacentSlides(currentSlide.GetTags(), photo.Tags.Union(firstVerticalPhoto.Tags).ToList());

                    if (score > initalScore)
                    {
                        candidatePhotos.Add(new CandidatePhoto()
                        {
                            Id = photo.Id,
                            Photo = photo,
                            Score = score
                        });
                    }
                }
            }

            if (candidatePhotos.Count == 0)
            {
                return searchSpacePhotos.Where(p => p.Id != firstVerticalPhoto.Id).OrderBy(photo => photo.NumberOfTags).First();
            }

            return CandidateSelectionProcess(candidatePhotos).Photo;
        }

        /// <summary>
        /// Choose the best photos from candidates to consist next slide
        /// </summary>
        /// <param name="candidatePhotos"></param>
        /// <returns></returns>
        private CandidatePhoto CandidateSelectionProcess(List<CandidatePhoto> candidatePhotos)
        {
            if (candidatePhotos.Count == 1)
            {
                return candidatePhotos.First();
            }

            int maxScore = candidatePhotos.Max(c => c.Score);
            // If there is more than one candidate then return the one with minimum number of tags
            List<CandidatePhoto> candidates = candidatePhotos.FindAll(photo => photo.Score == maxScore);

            if(candidates.Count == 1)
            {
                return candidates.First();
            }

            candidates = candidates.OrderBy(candidate => candidate.Photo.NumberOfTags).ToList();

            return candidates.First();
        }

        /// <summary>
        /// Creates a copy of slideshow solution
        /// </summary>
        /// <param name="slideshow"></param>
        /// <returns></returns>
        private Slideshow CopySolution(Slideshow slideshow)
        {
            List<Slide> slides = new List<Slide>(slideshow.Slides);
            int score = slideshow.Score;

            return new Slideshow(slides, score);
        }


        private List<Slide> FindSolutionForPartialInstance(List<Photo> photos)
        {
            List<Photo> collectionPhotos = photos;
            List<Slide> slides = new List<Slide>();
            // WAS HORIZONTAL PHOTOS
            var currentPhoto = photos.FirstOrDefault(x=>x.Orientation == Orientation.HORIZONTAL);
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
                    if (horizontalPhotos.Count > 0)
                    {
                        randomPhotos.Add(horizontalPhotos.First());
                    }
                    else
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

            return slides;
        }
    }
}