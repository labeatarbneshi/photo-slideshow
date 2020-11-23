﻿using PhotoSlideshow.Configuration;
using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoSlideshow
{
    class Solution
    {
        public Guid Id { get; }
        public Slideshow Slideshow { get; set; }
        public List<Slide> VerticalSlides { get; set; }
        public int Score { get; set; }

        public Solution()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Generates a new solution by splitting instance to multiple threads
        /// </summary>
        public static Solution Generate()
        {
            var solution = new Solution();
            Console.WriteLine($"[{DateTime.Now}] Generating solution...");

            List<Photo> collectionPhotos = new List<Photo>(Collection.Photos);
            List<Slide> slides = new List<Slide>();

            var splittedInstance = Common.SplitList(Collection.Photos, 10000).ToArray();

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

            int score = Evaluate(slides);

            solution.Slideshow = new Slideshow(slides, score);
            solution.VerticalSlides = new List<Slide>(solution.Slideshow.Slides.Where(s => s.Photos.Any(p => p.Orientation == Orientation.VERTICAL)).ToList());
            solution.Score = score;

            return solution;
            //Save(slideshow, "test_changes");
            //ILS ils = new ILS(CopySolution(slideshow));
            //ils.Optimize();
        }

        /// <summary>
        /// Created slideshow for given photo batch
        /// </summary>
        /// <param name="photos"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a new slide for slideshow
        /// </summary>
        /// <param name="currentSlide"></param>
        /// <param name="unusedPhotos"></param>
        /// <param name="firstSlidePhoto"></param>
        /// <returns></returns>
        private List<Photo> GetNextSlide(Slide currentSlide, List<Photo> unusedPhotos, Photo firstSlidePhoto = null)
        {
            var random = new Random();
            int slidingWindow = Common.CalculatePhotosToConsider(1, unusedPhotos.Count, true);

            List<CandidatePhoto> candidatePhotos = GetCandidatePhotos(currentSlide, unusedPhotos);
            List<Photo> chosenPhotos = new List<Photo>();
            CandidatePhoto candidate = null;
            if (candidatePhotos.Count == 0)
            {
                var randomPhoto = unusedPhotos[random.Next(0, unusedPhotos.Count)];
                candidate = new CandidatePhoto(randomPhoto.Id, randomPhoto, Slide.EvaluateAdjacent(currentSlide.GetTags(), randomPhoto.Tags));
            }
            else
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
            var solution = new Solution
            {
                Slideshow = new Slideshow(new List<Slide>(Slideshow.Slides), Score),
                VerticalSlides = new List<Slide>(VerticalSlides),
                Score = Score
            };

            return solution;
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
        public void Save(string name)
        {
            using (StreamWriter w = File.AppendText($"{name}.txt"))
            {
                Console.WriteLine("FINISHED WITH SCORE: " + Evaluate(Slideshow.Slides));
                w.WriteLine(Slideshow.Slides.Count);
                foreach (var s in Slideshow.Slides)
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
        public int SwapSlidesMutation(int firstIndex = -1, bool acceptWorse = true)
        {
            var random = new Random();
            var firstSlideIndex = firstIndex != -1 ? firstIndex : random.Next(0, Slideshow.Slides.Count);
            int secondSlideIndex;
            Slide firstSlide = Slideshow.Slides[firstSlideIndex];

            bool bothHorizontal;
            do
            {
                bothHorizontal = false;
                secondSlideIndex = random.Next(0, Slideshow.Slides.Count);
            } while (firstSlideIndex == secondSlideIndex);

            Slide secondSlide = Slideshow.Slides[secondSlideIndex];

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

            var preSwapScore = Slideshow.CalculateScore(Slideshow, firstSlideIndex) + Slideshow.CalculateScore(Slideshow, secondSlideIndex, omittedSlide);

            //Swap chosen slides
            Slide.Swap(Slideshow.Slides, firstSlideIndex, secondSlideIndex);
            var postSwapScore = Slideshow.CalculateScore(Slideshow, firstSlideIndex) + Slideshow.CalculateScore(Slideshow, secondSlideIndex, omittedSlide);
            var score = 0;
            if (bothHorizontal && postSwapScore == 0)
            {
                firstSlide.BadNeighbours.Add(secondSlide.Photos[0].Id);
                secondSlide.BadNeighbours.Add(firstSlide.Photos[0].Id);
            }

            if (acceptWorse)
            {
                score = postSwapScore - preSwapScore;
            }

            else
            {
                Slide.Swap(Slideshow.Slides, firstSlideIndex, secondSlideIndex);
            }

            return score;
        }

        /// <summary>
        /// Checks for swapping for slide that has had a vertical photo swap
        /// </summary>
        /// <param name="firstSlideIndex"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public int HardSwapMutation(int firstSlideIndex, int iterations)
        {
            bool betterScore = false;
            int i = 0;
            int score;
            do
            {
                score = SwapSlidesMutation(firstSlideIndex, i == iterations - 1);
                if (score > 0)
                {
                    betterScore = true;
                }

                i++;

            } while (betterScore == false && i < iterations);

            return score;
        }

        /// <summary>
        /// Randomly selects two slides with vertical photos and generates all slides from given photos by calculating score.
        /// </summary>
        /// <returns>The highest score from combination of photos</returns>
        public int VerticalSlidePhotoSwapMutation()
        {
            var random = new Random();
            var firstSlideIndex = random.Next(0, VerticalSlides.Count);

            int secondSlideIndex;
            do
            {
                secondSlideIndex = random.Next(0, VerticalSlides.Count);
            } while (firstSlideIndex == secondSlideIndex);

            Slide firstSlide = Slideshow.Slides.Find(s => s.Id == VerticalSlides[firstSlideIndex].Id);
            Slide secondSlide = Slideshow.Slides.Find(s => s.Id == VerticalSlides[secondSlideIndex].Id);

            if (firstSlide == null || secondSlide == null)
            {
                return 0;
            }

            var firstSlideIndexInSlideshow = Slideshow.Slides.FindIndex(s => s.Id == firstSlide.Id);
            var secondSlideIndexInSlideshow = Slideshow.Slides.FindIndex(s => s.Id == secondSlide.Id);

            var omittedSlide = -1;
            if (secondSlideIndexInSlideshow - 1 == firstSlideIndexInSlideshow)
            {
                omittedSlide = 1;
            }
            else if (secondSlideIndexInSlideshow + 1 == firstSlideIndexInSlideshow)
            {
                omittedSlide = 2;
            }

            var preSwapFirstSlideScore = Slideshow.CalculateScore(Slideshow, firstSlideIndexInSlideshow);
            var preSwapSecondSlideScore = Slideshow.CalculateScore(Slideshow, secondSlideIndexInSlideshow, omittedSlide);
            var preSwapScore = preSwapFirstSlideScore + preSwapSecondSlideScore;

            VerticalPhotoSwap verticalPhotoSwap = GetVerticalSlidePhotoSwapGain(
                SwapPhotos(Slide.Copy(firstSlide), Slide.Copy(secondSlide), random.Next(0, 2), random.Next(0, 2)),
                firstSlideIndexInSlideshow,
                secondSlideIndexInSlideshow,
                omittedSlide);

            if (verticalPhotoSwap.Gain >= preSwapScore)
            {
                Slideshow.Slides[firstSlideIndexInSlideshow] = verticalPhotoSwap.Slides[0];
                Slideshow.Slides[secondSlideIndexInSlideshow] = verticalPhotoSwap.Slides[1];
                return verticalPhotoSwap.Gain - preSwapScore;
            }

            else
            {
                Slideshow.Slides[firstSlideIndexInSlideshow] = verticalPhotoSwap.Slides[0];
                Slideshow.Slides[secondSlideIndexInSlideshow] = verticalPhotoSwap.Slides[1];


                var hardSwapWithFirstIndex = HardSwapMutation(firstSlideIndexInSlideshow, ConfigurationConsts.RetriesAfterBadVerticalSwap);
                var hardSwapWithSecondIndex = HardSwapMutation(secondSlideIndexInSlideshow, ConfigurationConsts.RetriesAfterBadVerticalSwap);
                return verticalPhotoSwap.Gain - preSwapScore + hardSwapWithFirstIndex + hardSwapWithSecondIndex;
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
        public List<Slide> SwapPhotos(Slide firstSlide, Slide secondSlide, int firstPhotoIndex, int secondPhotoIndex)
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

        public VerticalPhotoSwap GetVerticalSlidePhotoSwapGain(List<Slide> swappedSlidePhotos, int firstIndex, int secondIndex, int omittedSlide)
        {
            Slideshow.Slides[firstIndex] = swappedSlidePhotos[0];
            Slideshow.Slides[secondIndex] = swappedSlidePhotos[1];

            var firstSwapScore = Slideshow.CalculateScore(Slideshow, firstIndex);
            var secondSwapScore = Slideshow.CalculateScore(Slideshow, secondIndex, omittedSlide);

            return new VerticalPhotoSwap(swappedSlidePhotos, firstSwapScore + secondSwapScore);
        }

        public int ShuffleSlidesMutation()
        {
            var random = new Random();
            var length = random.Next(4, 12);
            var index = random.Next(0, Slideshow.Slides.Count);
            if (index + length > Slideshow.Slides.Count)
            {
                index = Slideshow.Slides.Count - length;
            }
            List<Slide> slides = new List<Slide>();
            slides.AddRange(Slideshow.Slides.GetRange(index, length));

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
                Slideshow.Slides[i] = shuffledList[j];
            }

            return postShuffleScore - preShuffleScore;
        }

        public void Mutate()
        {
            int mutationGain;
            var randomOperator = new Random().Next(1, 11);
            if (randomOperator <= ConfigurationConsts.SlideSwapUpperFrequency)
            {
                mutationGain = SwapSlidesMutation();
            }

            else if (randomOperator > ConfigurationConsts.VerticalPhotoSwapFrequencyLowerLimit &&
                randomOperator <= ConfigurationConsts.VerticalPhotoSwapFrequencyUpperLimit)
            {
                mutationGain = VerticalSlidePhotoSwapMutation();
            }

            else
            {
                mutationGain = ShuffleSlidesMutation();
            }

            Score += mutationGain;
        }

        public void Perturb(int percentage = 10)
        {
            var random = new Random();
            var removedSlides = Slideshow.Slides.Count * percentage / 100;
            List<Photo> removedPhotos = new List<Photo>();
            for (int i = 0; i < removedSlides; i++)
            {
                var rnd = random.Next(Slideshow.Slides.Count - 1);
                var removedSlide = Slideshow.Slides[rnd];
                foreach (var photo in removedSlide.Photos)
                {
                    removedPhotos.Add(photo);
                }

                Slideshow.Slides.RemoveAt(rnd);
            }


            while (removedPhotos.Count != 0)
            {
                var rndSlide = random.Next(Slideshow.Slides.Count - 1);
                var nextSlidePhotos = GetNextSlide(Slideshow.Slides[rndSlide], removedPhotos);
                Slideshow.Slides.Insert(rndSlide + 1, new Slide() { Id = nextSlidePhotos[0].Id, Photos = nextSlidePhotos });

                foreach(var photo in nextSlidePhotos)
                {
                    removedPhotos.Remove(photo);
                }
            }

            //var solutionForRemovedPhotos = FindSolutionForPartialInstance(removedPhotos);
            //Slideshow.Slides.AddRange(solutionForRemovedPhotos);
            //Score += Evaluate(solutionForRemovedPhotos);
            Score = Evaluate(Slideshow.Slides);
        }
    }
}