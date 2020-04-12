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
        private List<Slide> VerticalSlides = new List<Slide>();
        private List<Slide> HorizontalSlides = new List<Slide>();
        private int SlidingWindow = -1;
        private int NoOfIterations = 5;
        private readonly List<int> SlidingWindowsScores = new List<int>();

        private readonly Random random = new Random();
        public Solution(Collection collection)
        {
            Collection = collection;
            PrepareCollection();
            GenerateInitialSlides();
        }

        public void Generate()
        {
            List<Slide> initialSlides = HorizontalSlides.Concat(VerticalSlides).ToList();
            //CalculateSlidingWindow(completeSlides, 2);

            List<Slide> slides = new List<Slide>();
            while (initialSlides.Count > 0)
            {
                //Find a random slide that will be the first slide in slideshow
                var startingSlide = initialSlides[random.Next(0, initialSlides.Count)];
                initialSlides.Remove(startingSlide);
                slides.Add(startingSlide);

                //Continue by adding next slides
                var nextSlide = FindNextSlide(slides, initialSlides, startingSlide, 1);

                if (nextSlide != null)
                {
                    slides.Add(startingSlide);
                    slides.Add(nextSlide);
                }
            }

            int score = Evaluate(slides);
            Console.WriteLine($"Initial solution score: {score}");
        }

        /// <summary>
        /// Prepares collection by separating photos by orientation
        /// </summary>
        private void PrepareCollection()
        {
            //FILTERING PHOTOS BY ORIENTATION
            FilterVerticalPhotos();
            FilterHorizontalPhotos();

            //IF NUMBER OF VERTICAL PHOTOS IS ODD WE SHOULD REMOVE ONE FROM LIST
            if (VerticalPhotos.Count % 2 != 0)
            {
                RemovePhotoFromVerticalPhotosList(VerticalPhotoRemovalPolicy.RandomIndex);
            }
        }

        /// <summary>
        /// Filter vertical photos from collection
        /// </summary>
        private void FilterVerticalPhotos()
        {
            VerticalPhotos = new List<Photo>(Collection.Photos.Where(photo => photo.Orientation == Orientation.VERTICAL).ToList());
        }

        /// <summary>
        /// Filter horizontal photos from collection
        /// </summary>
        private void FilterHorizontalPhotos()
        {
            HorizontalPhotos = new List<Photo>(Collection.Photos.Where(photo => photo.Orientation == Orientation.HORIZONTAL).ToList());
        }

        /// <summary>
        /// Remove one photo from Vertical photos list
        /// </summary>
        /// <param name="removalPolicy">A VerticalPhotoRemovalPolicy value from specified enum</param>
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

        /// <summary>
        /// Calculate the number of slides to search for as a candidate for next slide
        /// </summary>
        /// <param name="initalSlideList">List of initially created slides</param>
        /// <param name="percentageFromTotal">User specified percentage which determines number of slides to search in</param>
        private void CalculateSlidingWindow(List<Slide> initalSlideList, double percentageFromTotal) 
        {
            SlidingWindow = 
                (int)Math.Floor(percentageFromTotal * initalSlideList.Count / 100) == 0 ? 1 : (int)Math.Floor(percentageFromTotal * initalSlideList.Count / 100);
        }

        /// <summary>
        /// Evaluate solution score
        /// </summary>
        /// <param name="slides">List of slides that should be evaluated</param>
        /// <returns></returns>
        private int Evaluate(List<Slide> slides)
        {
            int score = 0;
            
            if(slides.Count == 1)
            {
                return 1;
            }

            for(int i = 0; i < slides.Count - 1; i++)
            {
                score += EvaluateAdjacentSlides(slides[i].GetTags(), slides[i + 1].GetTags());
                
                if(i == SlidingWindow - 1)
                {
                    SlidingWindowsScores.Add(score);
                    SlidingWindow += SlidingWindow;
                }
            }


            return score;
        }

        /// <summary>
        /// Evaluate score between two slides
        /// </summary>
        /// <param name="firstSlideTags">First slide</param>
        /// <param name="secondSlideTags">Second slide</param>
        /// <returns></returns>
        private int EvaluateAdjacentSlides(List<string> firstSlideTags, List<string> secondSlideTags)
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

        private Slide FindNextSlide(List<Slide> finalSlides, List<Slide> initialSlides, Slide startingSlide, double percentageOfCandidatePhotos)
        {
            int slidingWindow = CalculatePhotosToConsider(initialSlides, percentageOfCandidatePhotos);
            int maxTagNo = startingSlide.GetTags().Count / 2;

            List<CandidateSlide> candidateSlides = new List<CandidateSlide>();
            List<Slide> searchSpaceSlides = new List<Slide>();
            List<Slide> searchSpace = initialSlides.Where(s => s.GetTags().Count <= maxTagNo).ToList();

            for (int i = 0; i < NoOfIterations; i++)
            {
                int startingIndex = 0;

                if (searchSpace.Count > 0)
                {
                    startingIndex = random.Next(0, searchSpace.Count - slidingWindow < 0 ? searchSpace.Count : searchSpace.Count - slidingWindow);
                    if (searchSpace.Count - startingIndex < slidingWindow)
                    {
                        slidingWindow = initialSlides.Count - startingIndex;
                    }
                }
                else
                {
                    startingIndex = random.Next(0, initialSlides.Count - slidingWindow < 0 ? initialSlides.Count : initialSlides.Count - slidingWindow);
                    if (initialSlides.Count - startingIndex < slidingWindow)
                    {
                        slidingWindow = initialSlides.Count - startingIndex;
                    }
                }

                searchSpaceSlides.AddRange(initialSlides.GetRange(startingIndex, slidingWindow));

                foreach (var slide in searchSpaceSlides)
                {
                    int score = EvaluateAdjacentSlides(startingSlide.GetTags(), slide.GetTags());
                    if (score > 0)
                    {
                        candidateSlides.Add(new CandidateSlide() 
                        {
                            Id = slide.Id,
                            Slide = slide,
                            IsUsed = false,
                            Score = score
                        });
                    }
                }

                if (candidateSlides.Count != 0)
                {
                    break;
                }
            }

            if (candidateSlides.Count == 0)
            {
                return null;
            }

            Slide chosenSlide = candidateSlides.OrderByDescending(slide => slide.Score).First().Slide;
            initialSlides.Remove(chosenSlide);

            return chosenSlide;
        }

        /// <summary>
        /// Calculates search space for a specific slide
        /// </summary>
        /// <param name="initialSlides"></param>
        /// <param name="percentageOfCandidatePhotos"></param>
        /// <returns></returns>
        private int CalculatePhotosToConsider(List<Slide> initialSlides, double percentageOfCandidatePhotos)
        {
            if(initialSlides.Count <= 100)
            {
                return initialSlides.Count;
            }

            int photosToConsider = (int)Math.Floor(percentageOfCandidatePhotos * initialSlides.Count / 100);
            
            if(photosToConsider == 0)
            {
                return 1;
            }

            return (int)Math.Floor(percentageOfCandidatePhotos * initialSlides.Count / 100);
        }
    
        /// <summary>
        /// Generate inital slides from photos
        /// </summary>
        private void GenerateInitialSlides()
        {
            GenerateHorizontalSlides();
            GenerateVerticalSlides();
        }

        /// <summary>
        /// Generates initial slides only from horizontal photos
        /// </summary>
        private void GenerateHorizontalSlides()
        {
            foreach (var photo in HorizontalPhotos)
            {
                HorizontalSlides.Add(new Slide()
                {
                    Id = photo.Id,
                    Photos = new List<Photo>() { photo }
                });
            }
        }

        /// <summary>
        /// Generate initial slides from vertical photos
        /// </summary>
        public void GenerateVerticalSlides()
        {
            List<Photo> verticalPhotos = new List<Photo>(VerticalPhotos);
            List<Photo> orderedByNoOfTags = verticalPhotos.OrderBy(photos => photos.NumberOfTags).ToList();

            while (orderedByNoOfTags.Count > 0)
            {
                VerticalSlides.Add(new Slide()
                {
                    Id = orderedByNoOfTags[0].Id,
                    Photos = new List<Photo>()
                    {
                        orderedByNoOfTags[0],
                        orderedByNoOfTags[orderedByNoOfTags.Count - 1]
                    }
                });

                orderedByNoOfTags.RemoveAt(0);
                orderedByNoOfTags.RemoveAt(orderedByNoOfTags.Count - 1);
            }
        }

        /// <summary>
        /// Calculate reduced search space by returning a list that has elements == slidingWindow
        /// </summary>
        /// <param name="searchSpace">A filtered list of slides</param>
        /// <param name="initialSlides">List of solution slides</param>
        /// <param name="slidingWindow">Indicates the number of slides that search space should include</param>
        /// <returns></returns>
        public List<Slide> CalculateReducedSearchSpace(List<Slide> searchSpace, List<Slide> initialSlides, int slidingWindow)
        {
            int startingIndex;

            if (searchSpace.Count > 0)
            {
                startingIndex = random.Next(0, searchSpace.Count - slidingWindow < 0 ? searchSpace.Count : searchSpace.Count - slidingWindow);
                if (searchSpace.Count - startingIndex < slidingWindow)
                {
                    slidingWindow = initialSlides.Count - startingIndex;
                }
            }
            else
            {
                startingIndex = random.Next(0, initialSlides.Count - slidingWindow < 0 ? initialSlides.Count : initialSlides.Count - slidingWindow);
                if (initialSlides.Count - startingIndex < slidingWindow)
                {
                    slidingWindow = initialSlides.Count - startingIndex;
                }
            }

            List<Slide> candidateSlides = new List<Slide>();
            candidateSlides.AddRange(initialSlides.GetRange(startingIndex, slidingWindow));
            
            return candidateSlides;
        }
    }
}
