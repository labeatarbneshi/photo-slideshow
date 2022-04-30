using PhotoSlideshow.Configuration;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PhotoSlideshow
{
    class GeneticAlgorithm
    {
        readonly int populationSize;
        readonly int tournamentSize;
        readonly int mutationRate;
        readonly int crossoverRate;
        readonly int iterations;
        public List<Solution> Population { get; set; }

        public GeneticAlgorithm()
        {
            populationSize = ConfigurationConsts.PopSize;
            tournamentSize = ConfigurationConsts.TournamentSize;
            mutationRate = ConfigurationConsts.MutationRate;
            crossoverRate = ConfigurationConsts.CrossoverRate;
            iterations = ConfigurationConsts.GAIterations;
            Population = new List<Solution>();
        }

        public Solution FindSolution()
        {
            GeneratePopulation();
            var printSw = Stopwatch.StartNew();
            var sw = Stopwatch.StartNew();
            while(sw.ElapsedMilliseconds <= 60000) { 
                var random = new Random();
                var populationFromCrossover = new List<Solution>();

                if (random.Next(0, 100) < crossoverRate && Collection.VerticalPhotos.Count > 0)
                {
                    for (int i = 0; i < Population.Count; i += 2)
                    {
                        populationFromCrossover.Add(NGeneCrossover(Population[i], Population[i + 1]));
                    }
                }


                for (int i = 0; i < iterations; i++)
                {
                    if(sw.ElapsedMilliseconds > 60000)
                    {
                        break;
                    }
                    var parent = TournamentSelection();
                    var mutant = parent.Copy();

                    var randomNr = random.Next(0, 100);
                    if (mutationRate > randomNr)
                    {
                        mutant.Mutate();
                    }

                    ILS.OptimizeWithoutPerturb(mutant, 5);
                    if (printSw.ElapsedMilliseconds >= 20000)
                    {
                        printSw.Restart();
                        Console.WriteLine($"{DateTime.Now}:  {GetBest().Slideshow.Score}");
                    }
                    Population.Add(mutant);
                }
                Population.AddRange(populationFromCrossover);
                populationFromCrossover.RemoveRange(0, populationFromCrossover.Count);
                Population = Population.OrderByDescending(s => s.Slideshow.Score).Take(populationSize).ToList();

                if (printSw.ElapsedMilliseconds >= 20000)
                {
                    printSw.Restart();
                    Console.WriteLine($"{DateTime.Now}:  {GetBest().Slideshow.Score}");
                }

            }
            var t = GetBest();
            Console.WriteLine($"{DateTime.Now} starting hill climb with score: {t.Slideshow.Score}");
            var test = ILS.OptimizeWithoutPerturb(t, 120);
            ILS iteratedLocalSearch = new ILS();


            Console.WriteLine($"{DateTime.Now} starting ils with score: {test.Slideshow.Score}");
            var best2 = iteratedLocalSearch.OptimizeV2(test);

            return best2;
        }

        public void GeneratePopulation()
        {
            for (int i = 0; i < populationSize; i++)
            {
                //ConfigurationConsts.SlidingWindowPercentage = new Random().Next(0, 5);
                Population.Add(Solution.GenerateRandom());
            }
        }

        private Solution TournamentSelection()
        {
            List<Solution> TournamentSelectionIndividuals = new List<Solution>();
            List<int> PopulationIndexes = new List<int>();
            for (int i = 0; i < populationSize; i++)
            {
                PopulationIndexes.Add(i);
            }
            Random rnd = new Random();
            for (int i = 0; i < this.tournamentSize; i++)
            {
                if (i <= populationSize)
                {
                    int rndIndex = rnd.Next(0, PopulationIndexes.Count);
                    int rndIndexAsElement = PopulationIndexes[rndIndex];
                    TournamentSelectionIndividuals.Add(Population[rndIndexAsElement]);
                    PopulationIndexes.RemoveAt(rndIndex);
                }
            }
            return TournamentSelectionIndividuals.OrderByDescending(x => x.Slideshow.Score).FirstOrDefault();
        }

        private void ReplaceWorst(Solution mutant)
        {
            var worstIndividual = GetWorst();
            if (worstIndividual.Slideshow.Score < mutant.Slideshow.Score)
            {
                Population.Remove(worstIndividual);
                Population.Add(mutant);
            }
        }

        private Solution GetBest()
        {
            return Population.OrderByDescending(p => p.Slideshow.Score).FirstOrDefault();
        }

        private Solution GetWorst()
        {
            return Population.OrderBy(p => p.Slideshow.Score).FirstOrDefault();
        }

        private Solution NGeneCrossover(Solution firstParent, Solution secondParent, int n = 0)
        {
            var random = new Random();
            int randomNumbers = n != 0 ? n : random.Next(1, 10) * firstParent.Slideshow.Slides.Count / 100;
            List<int> randomIndexes = Common.GetRandomNumbers(firstParent.Slideshow.Slides.Count, randomNumbers);

            var child = firstParent.Copy();
            List<int> firstParentSlides = new List<int>();
            var removedRandomIndexes = new List<int>();
            foreach(int index in randomIndexes)
            {
                Slide s = firstParent.Slideshow.Slides[index];
                if(s.Photos.Count == 1)
                {
                    firstParentSlides.Add(s.Id);
                } else
                {
                    removedRandomIndexes.Add(index);
                }
            }

            randomIndexes = randomIndexes.Except(removedRandomIndexes).ToList();

            var secondParentIndexes = secondParent.Slideshow.Slides.Select((item, index) => new { Item = item, Index = index })
                 .Where(v => firstParentSlides.Contains(v.Item.Id))
                 .Select(v => new  CrossoverSlide(v.Item, v.Index))
                 .OrderBy(v => v.Index)
                 .ToList();

            for(int i = 0; i < randomIndexes.Count; i++)
            {
                child.Slideshow.Slides[randomIndexes[i]] = secondParentIndexes[i].Slide;
            }

            child.Slideshow.Score = Common.EvaluateSolution(child.Slideshow.Slides);
            return child;
        }
    }
}
