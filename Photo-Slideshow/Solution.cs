using PhotoSlideshow.Configuration;
using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSlideshow
{
    class Solution
    {
        /// <summary>
        /// Generates a new solution by splitting instance to multiple threads
        /// </summary>
        public static Slideshow Generate()
        {
            Console.WriteLine($"[{DateTime.Now}] Generating solution...");

            List<Photo> collectionPhotos = new List<Photo>(Collection.Photos);
            List<Slide> slides = new List<Slide>();

            var splittedInstance = Common.SplitList(Collection.Photos, 10000).ToArray();

            var taskList = new List<Task<List<Slide>>>();
            foreach (var item in splittedInstance)
            {
                taskList.Add(Task<List<Slide>>.Factory.StartNew(() => FindSolutionForPartialInstance(item)));
            }

            Task.WaitAll(taskList.ToArray());

            foreach (var task in taskList)
            {
                slides.AddRange(task.Result);
            }

            int score = Evaluate(slides);

            Slideshow slideshow = new Slideshow(slides, score);

            return slideshow;
            //Save(slideshow, "test_changes");
            //ILS ils = new ILS(CopySolution(slideshow));
            //ils.Optimize();
        }

        /// <summary>
        /// Created slideshow for given photo batch
        /// </summary>
        /// <param name="photos"></param>
        /// <returns></returns>
        private static List<Slide> FindSolutionForPartialInstance(List<Photo> photos)
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

        /// <summary>
        /// Creates a new slide for slideshow
        /// </summary>
        /// <param name="currentSlide"></param>
        /// <param name="unusedPhotos"></param>
        /// <param name="firstSlidePhoto"></param>
        /// <returns></returns>
        private static List<Photo> GetNextSlide(Slide currentSlide, List<Photo> unusedPhotos, Photo firstSlidePhoto = null)
        {
            var random = new Random();
            int slidingWindow = Common.CalculatePhotosToConsider(1, unusedPhotos.Count, true);

            List<CandidatePhoto> candidatePhotos = GetCandidatePhotos(currentSlide, unusedPhotos);
            List<Photo> chosenPhotos = new List<Photo>();
            CandidatePhoto candidate = null;
            if(candidatePhotos.Count == 0)
            {
                var randomPhoto = unusedPhotos[random.Next(0, unusedPhotos.Count)];
                candidate = new CandidatePhoto(randomPhoto.Id, randomPhoto, Slide.EvaluateAdjacent(currentSlide.GetTags(), randomPhoto.Tags));
            } else
            {
                candidate = CandidateSelectionProcess(candidatePhotos);
            }
            chosenPhotos.Add(candidate.Photo);

            if (candidate.Photo.Orientation == Orientation.VERTICAL)
            {
                //Find another vertical photo to add to current slide
                List<CandidatePhoto> verticalCandidatePhotos = GetCandidatePhotos(currentSlide, unusedPhotos.FindAll(photo => photo.Orientation == Orientation.VERTICAL), candidate.Photo, candidate.Score);
                chosenPhotos.Add(verticalCandidatePhotos.Count == 0 ?
                    unusedPhotos.Where(p => p.Id != candidate.Photo.Id && p.Orientation == Orientation.VERTICAL).OrderBy(photo => photo.NumberOfTags).First() :
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
        private static List<CandidatePhoto> GetCandidatePhotos(Slide currentSlide, List<Photo> unusedPhotos, Photo firstSlidePhoto = null, int initialScore = -1)
        {
            var random = new Random();
            int slidingWindow = Common.CalculatePhotosToConsider(1, unusedPhotos.Count);
            int scoreToBeat = initialScore != -1 ? initialScore : 0;
            List <CandidatePhoto> candidatePhotos = new List<CandidatePhoto>();
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
                        Slide.EvaluateAdjacent(currentSlide.GetTags(), photo.Tags.Union(firstSlidePhoto.Tags).ToList()) : 
                        Slide.EvaluateAdjacent(currentSlide.GetTags(), photo.Tags);

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
        private static CandidatePhoto CandidateSelectionProcess(List<CandidatePhoto> candidatePhotos)
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
        private Slideshow Copy(Slideshow slideshow)
        {
            List<Slide> slides = new List<Slide>(slideshow.Slides);
            int score = slideshow.Score;

            return new Slideshow(slides, score);
        }

        /// <summary>
        /// Evaluate solution score
        /// </summary>
        /// <param name="slides">List of slides that should be evaluated</param>
        /// <returns></returns>
        public static int Evaluate(List<Slide> slides)
        {
            int score = 0;

            if (slides.Count == 1)
            {
                return 1;
            }

            for (int i = 0; i < slides.Count - 1; i++)
            {
                score += Slide.EvaluateAdjacent(slides[i].GetTags(), slides[i + 1].GetTags());
            }


            return score;
        }

        /// <summary>
        /// Saves solution to text file
        /// </summary>
        /// <param name="slideshow"></param>
        /// <param name="name"></param>
        public static void Save(Slideshow slideshow, string name)
        {
            using (StreamWriter w = File.AppendText($"{name}.txt"))
            {
                Console.WriteLine("FINISHED WITH SCORE: " + Evaluate(slideshow.Slides));
                w.WriteLine(slideshow.Slides.Count);
                foreach (var s in slideshow.Slides)
                {
                    if (s.Photos.Count > 1)
                    {
                        int a = s.Photos[0].Id - 2;
                        int b = s.Photos[1].Id - 2;
                        w.WriteLine(a + " " + b);
                    }
                    else
                    {
                        w.WriteLine(s.Photos[0].Id - 2);
                    }
                }
            }
        }

        /// <summary>
        /// Swap two randomly selected slides
        /// </summary>
        /// <returns>Result achiveed by swap</returns>
        public static int SwapSlidesMutation(Slideshow slideshow, Stopwatch acceptWorseSolution, Stopwatch timeWithoutProgress, int firstIndex = -1)
        {
            var random = new Random();
            var firstSlideIndex = firstIndex != -1 ? firstIndex : random.Next(0, slideshow.Slides.Count);
            int secondSlideIndex;
            Slide firstSlide = slideshow.Slides[firstSlideIndex];

            bool bothHorizontal;
            do
            {
                bothHorizontal = false;
                secondSlideIndex = random.Next(0, slideshow.Slides.Count);
            } while (firstSlideIndex == secondSlideIndex);

            Slide secondSlide = slideshow.Slides[secondSlideIndex];

            if (firstSlide.Photos.Count == 1 && secondSlide.Photos.Count == 1)
            {
                bothHorizontal = true;
                if (firstSlide.BadNeighbours.Contains(secondSlide.Photos[0].Id) || secondSlide.BadNeighbours.Contains(firstSlide.Photos[0].Id))
                {
                    return 0;
                }
            }

            var omittedSlide = -1;
            if (secondSlideIndex - 1 == firstSlideIndex)
            {
                omittedSlide = 1;
            }
            else if (secondSlideIndex + 1 == firstSlideIndex)
            {
                omittedSlide = 2;
            }

            var preSwapScore = Slideshow.CalculateScore(slideshow, firstSlideIndex) + Slideshow.CalculateScore(slideshow, secondSlideIndex, omittedSlide);

            //Swap chosen slides
            Slide.Swap(slideshow.Slides, firstSlideIndex, secondSlideIndex);
            var postSwapScore = Slideshow.CalculateScore(slideshow, firstSlideIndex) + Slideshow.CalculateScore(slideshow, secondSlideIndex, omittedSlide);

            if (bothHorizontal && postSwapScore == 0)
            {
                firstSlide.BadNeighbours.Add(secondSlide.Photos[0].Id);
                secondSlide.BadNeighbours.Add(firstSlide.Photos[0].Id);
            }

            if (postSwapScore >= preSwapScore ||
                (acceptWorseSolution.ElapsedMilliseconds > ConfigurationConsts.AcceptWorseSolutionAfterMillis &&
                timeWithoutProgress.ElapsedMilliseconds > ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis && firstIndex != -1))
            {
                return postSwapScore - preSwapScore;

            }
            else
            {
                Slide.Swap(slideshow.Slides, firstSlideIndex, secondSlideIndex);

                return 0;
            }
        }

        /// <summary>
        /// Checks for swapping for slide that has had a vertical photo swap
        /// </summary>
        /// <param name="firstSlideIndex"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static int HardSwapMutation(Slideshow slideshow, int firstSlideIndex, int iterations)
        {
            bool betterScore = false;
            int i = 0;
            int score;
            do
            {
                score = SwapSlidesMutation(slideshow, null, null, firstSlideIndex);
                if (score > 0)
                {
                    betterScore = true;
                }

                i++;

            } while (betterScore == true && i < iterations);

            return score;
        }

         /// <summary>
        /// Randomly selects two slides with vertical photos and generates all slides from given photos by calculating score.
        /// </summary>
        /// <returns>The highest score from combination of photos</returns>
        public static int VerticalSlidePhotoSwapMutation(Slideshow slideshow, List<Slide> verticalSlides, Stopwatch acceptWorseSolution, Stopwatch timeWithoutProgress)
        {
            var random = new Random();
            var firstSlideIndex = random.Next(0, verticalSlides.Count);

            int secondSlideIndex;
            do
            {
                secondSlideIndex = random.Next(0, verticalSlides.Count);
            } while (firstSlideIndex == secondSlideIndex);

            Slide firstSlide = slideshow.Slides.Find(s => s.Id == verticalSlides[firstSlideIndex].Id);
            Slide secondSlide = slideshow.Slides.Find(s => s.Id == verticalSlides[secondSlideIndex].Id);


            if (firstSlide == null || secondSlide == null)
            {
                return 0;
            }

            var firstSlideIndexInSlideshow = slideshow.Slides.FindIndex(s => s.Id == firstSlide.Id);
            var secondSlideIndexInSlideshow = slideshow.Slides.FindIndex(s => s.Id == secondSlide.Id);

            var omittedSlide = -1;
            if (secondSlideIndexInSlideshow - 1 == firstSlideIndexInSlideshow)
            {
                omittedSlide = 1;
            }
            else if (secondSlideIndexInSlideshow + 1 == firstSlideIndexInSlideshow)
            {
                omittedSlide = 2;
            }

            var preSwapFirstSlideScore = Slideshow.CalculateScore(slideshow, firstSlideIndexInSlideshow);
            var preSwapSecondSlideScore = Slideshow.CalculateScore(slideshow, secondSlideIndexInSlideshow, omittedSlide);
            var preSwapScore = preSwapFirstSlideScore + preSwapSecondSlideScore;

            VerticalPhotoSwap verticalPhotoSwap = GetVerticalSlidePhotoSwapGain(
                slideshow, 
                SwapPhotos(Slide.Copy(firstSlide), Slide.Copy(secondSlide), random.Next(0, 2), random.Next(0, 2)), 
                firstSlideIndexInSlideshow, 
                secondSlideIndexInSlideshow, 
                omittedSlide);

            if (verticalPhotoSwap.Gain >= preSwapScore)
            {
                slideshow.Slides[firstSlideIndexInSlideshow] = verticalPhotoSwap.Slides[0];
                slideshow.Slides[secondSlideIndexInSlideshow] = verticalPhotoSwap.Slides[1];

                return verticalPhotoSwap.Gain - preSwapScore;
            }

            if (acceptWorseSolution.ElapsedMilliseconds > ConfigurationConsts.AcceptWorseSolutionAfterMillis &&
                timeWithoutProgress.ElapsedMilliseconds > ConfigurationConsts.AcceptWorseSolutionAfterNoProgressMillis)
            {
                slideshow.Slides[firstSlideIndexInSlideshow] = verticalPhotoSwap.Slides[0];
                slideshow.Slides[secondSlideIndexInSlideshow] = verticalPhotoSwap.Slides[1];

                var hardSwapWithFirstIndex = HardSwapMutation(slideshow, firstSlideIndexInSlideshow, ConfigurationConsts.RetriesAfterBadVerticalSwap);

                var hardSwapWithSecondIndex = HardSwapMutation(slideshow, secondSlideIndexInSlideshow, ConfigurationConsts.RetriesAfterBadVerticalSwap);

                return verticalPhotoSwap.Gain + hardSwapWithFirstIndex + hardSwapWithSecondIndex - preSwapScore;
            }
            else
            {
                slideshow.Slides[firstSlideIndexInSlideshow] = firstSlide;
                slideshow.Slides[secondSlideIndexInSlideshow] = secondSlide;
                return 0;
            }
        }

        /// <summary>
        /// Swaps photos between two vertical slides
        /// </summary>
        /// <param name="firstSlide">First chosen slide</param>
        /// <param name="secondSlide">Second chosen slide</param>
        /// <param name="firstPhotoIndex">Index of first photo on first slide</param>
        /// <param name="secondPhotoIndex">Index of second photo on second slide</param>
        /// <returns>List with newly generated slides</returns>
        public static List<Slide> SwapPhotos(Slide firstSlide, Slide secondSlide, int firstPhotoIndex, int secondPhotoIndex)
        {
            Photo temp = firstSlide.Photos[firstPhotoIndex];
            firstSlide.Photos[firstPhotoIndex] = secondSlide.Photos[secondPhotoIndex];
            secondSlide.Photos[secondPhotoIndex] = temp;

            List<Slide> slides = new List<Slide>
            {
                firstSlide,
                secondSlide
            };

            return slides;
        }

        public static VerticalPhotoSwap GetVerticalSlidePhotoSwapGain(Slideshow slideshow, List<Slide> swappedSlidePhotos, int firstIndex, int secondIndex, int omittedSlide)
        {
            slideshow.Slides[firstIndex] = swappedSlidePhotos[0];
            slideshow.Slides[secondIndex] = swappedSlidePhotos[1];

            var firstSwapScore = Slideshow.CalculateScore(slideshow, firstIndex);
            var secondSwapScore = Slideshow.CalculateScore(slideshow, secondIndex, omittedSlide);

            return new VerticalPhotoSwap(swappedSlidePhotos, firstSwapScore + secondSwapScore);
        }

        public static int ShuffleSlidesMutation(Slideshow slideshow, int length)
        {
            var random = new Random();
            var index = random.Next(0, slideshow.Slides.Count);
            if (index + length > slideshow.Slides.Count)
            {
                index = slideshow.Slides.Count - length;
            }
            List<Slide> slides = new List<Slide>();
            slides.AddRange(slideshow.Slides.GetRange(index, length));

            int preShuffleScore = Evaluate(slides);

            List<Slide> slidesToShuffle = new List<Slide>();
            slidesToShuffle.AddRange(slides.GetRange(1, slides.Count - 2));

            var shuffledList = slidesToShuffle.OrderBy(x => Guid.NewGuid()).ToList();
            shuffledList.Insert(0, slides[0]);
            shuffledList.Add(slides[slides.Count - 1]);

            int postShuffleScore = Evaluate(shuffledList);

            if (preShuffleScore > postShuffleScore)
            {
                return 0;
            }

            for (int i = index + 1, j = 1; i < index + length - 1; i++, j++)
            {
                slideshow.Slides[i] = shuffledList[j];
            }

            return postShuffleScore - preShuffleScore;
        }

    }
}