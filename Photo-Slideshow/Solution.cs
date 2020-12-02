using PhotoSlideshow.Configuration;
using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using PhotoSlideshow.Operators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSlideshow
{
    class Solution
    {
        public Guid Id { get; }
        public Slideshow Slideshow{ get; set; }
        public Solution()
        {
            Id = Guid.NewGuid();
            Slideshow = new Slideshow();
        }

        public static Solution GenerateRandom()
        {
            var solution = new Solution();
            Console.WriteLine($"[{DateTime.Now}] Generating solution...");

            List<Photo> collectionPhotos = new List<Photo>(Collection.Photos);
            List<Slide> slides = new List<Slide>();

            var splittedInstance = Common.SplitList(Collection.Photos, 20000).ToArray();

            var taskList = new List<Task<List<Slide>>>();
            foreach (var item in splittedInstance)
            {
                taskList.Add(Task<List<Slide>>.Factory.StartNew(() => solution.FindSolutionForPartialInstance(item)));
            }

            Task.WaitAll(taskList.ToArray());

            foreach (var task in taskList)
            {
                slides.AddRange(task.Result);
            }

            int score = Common.EvaluateSolution(slides);

            solution.Slideshow = new Slideshow(slides, score);

            Console.WriteLine($"[SOLUTION] Total generated slides: {slides.Count}");
            Console.WriteLine($"{DateTime.Now} Initial solution score: {score}");

            return solution;
        }

        private List<Slide> FindSolutionForPartialInstance(List<Photo> photos)
        {
            var random = new Random();
            List<Photo> collectionPhotos = new List<Photo>(photos);
            List<Slide> slides = new List<Slide>();
            var currentPhoto = photos[random.Next(0, photos.Count - 1)];
            Slide currentSlide = null;
            if (currentPhoto.Orientation == Orientation.HORIZONTAL)
            {
                currentSlide = new Slide()
                {
                    Id = currentPhoto.Id,
                    Photos = new List<Photo>()
                    {
                        currentPhoto
                    }
                };
                collectionPhotos.Remove(currentPhoto);
            }
            else
            {
                currentSlide = new Slide()
                {
                    Id = currentPhoto.Id,
                    Photos = new List<Photo>()
                    {
                        currentPhoto,
                        photos.FirstOrDefault(x => x.Orientation == Orientation.VERTICAL && x.Id != currentPhoto.Id)
                    }
                };
            }

            slides.Add(currentSlide);
            foreach (var photo in currentSlide.Photos)
            {
                collectionPhotos.Remove(photo);
            }

            while (collectionPhotos.Count > 0)
            {
                List<Photo> nextSlidePhotos = GetNextSlide(currentSlide, collectionPhotos);

                currentSlide = new Slide() { Id = nextSlidePhotos[0].Id, Photos = nextSlidePhotos };
                slides.Add(currentSlide);

                foreach (var photo in nextSlidePhotos)
                {
                    collectionPhotos.Remove(photo);
                }
            }

            return slides;
        }

        private List<Photo> GetNextSlide(Slide currentSlide, List<Photo> unselectedPhotos)
        {
            var random = new Random();
            List<CandidatePhoto> candidatePhotos = GetCandidatePhotos(currentSlide, unselectedPhotos);
            List<Photo> chosenPhotos = new List<Photo>();
            CandidatePhoto candidate = null;
            if (candidatePhotos.Count == 0)
            {
                var randomPhoto = unselectedPhotos[random.Next(0, unselectedPhotos.Count)];
                candidate = new CandidatePhoto(randomPhoto.Id, randomPhoto, Common.EvaluateAdjacentSlides(currentSlide.GetTags(), randomPhoto.Tags));
            }
            else
            {
                candidate = CandidateSelectionProcess(candidatePhotos);
            }
            chosenPhotos.Add(candidate.Photo);

            if (candidate.Photo.Orientation == Orientation.VERTICAL)
            {
                //Find another vertical photo to add to current slide
                List<CandidatePhoto> verticalCandidatePhotos = GetCandidatePhotos(currentSlide, unselectedPhotos.FindAll(photo => photo.Orientation == Orientation.VERTICAL), candidate.Photo, candidate.Score);
                chosenPhotos.Add(verticalCandidatePhotos.Count == 0 ?
                    unselectedPhotos.Where(p => p.Id != candidate.Photo.Id && p.Orientation == Orientation.VERTICAL)
                                    .OrderBy(photo => photo.NumberOfTags).First() :
                    CandidateSelectionProcess(verticalCandidatePhotos).Photo);
            }

            return chosenPhotos;
        }

        /// <summary>
        /// Returns candidate photos to consist slide being created
        /// </summary>
        /// <param name="currentSlide"></param>
        /// <param name="unusedPhotos"></param>
        /// <param name="firstSlidePhoto"></param>
        /// <param name="initialScore"></param>
        /// <returns></returns>
        private List<CandidatePhoto> GetCandidatePhotos(Slide currentSlide, List<Photo> unusedPhotos, Photo firstSlidePhoto = null, int initialScore = -1)
        {
            var random = new Random();
            int slidingWindow = Common.CalculatePhotosToConsider(1, unusedPhotos.Count);
            int scoreToBeat = initialScore != -1 ? initialScore : 0;
            List<CandidatePhoto> candidatePhotos = new List<CandidatePhoto>();
            List<Photo> searchSpacePhotos = new List<Photo>();
            for (int i = 0; i < 5; i++)
            {
                int startIndex = random.Next(0, unusedPhotos.Count);
                searchSpacePhotos = firstSlidePhoto != null ?
                    Common.GetSearchSpacePhotos(unusedPhotos, slidingWindow, startIndex).FindAll(p => p.Id != firstSlidePhoto.Id) :
                    Common.GetSearchSpacePhotos(unusedPhotos, slidingWindow, startIndex); ;
                foreach (var photo in searchSpacePhotos)
                {
                    int score = firstSlidePhoto != null ?
                        Common.EvaluateAdjacentSlides(currentSlide.GetTags(), photo.Tags.Union(firstSlidePhoto.Tags).ToList()) :
                        Common.EvaluateAdjacentSlides(currentSlide.GetTags(), photo.Tags);

                    if (photo.Orientation == Orientation.HORIZONTAL && score == 0)
                    {
                        currentSlide.BadNeighbours.Add(photo.Id);
                    }

                    if (score > scoreToBeat)
                    {
                        candidatePhotos.Add(new CandidatePhoto(photo.Id, photo, score));
                    }
                }
            }

            return candidatePhotos;
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

            if (candidates.Count == 1)
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
        public Solution Copy()
        {
            List<Slide> slides = new List<Slide>(Slideshow.Slides);
            int score = Slideshow.Score;

            var solution = new Solution
            {
                Slideshow = new Slideshow(slides, score)
            };

            return solution;
        }

        public int Mutate()
        {
            var random = new Random();
            var randomOperator = random.Next(1, 11);
            int gain;
            if (randomOperator <= ConfigurationConsts.SlideSwapUpperFrequency)
            {
                gain = Swap.SwapSlides(Slideshow);
            }

            else if (randomOperator > ConfigurationConsts.VerticalPhotoSwapFrequencyLowerLimit && randomOperator <= ConfigurationConsts.VerticalPhotoSwapFrequencyUpperLimit)
            {
                gain = Swap.SwapVerticalSlidePhotos(Slideshow);
            }

            else
            {
                gain = Shuffle.ShuffleSlides(Slideshow, random.Next(4, 8));
            }

            Slideshow.Score += gain;
            return gain;
        }
    }
}