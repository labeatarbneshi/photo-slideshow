using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSlideshow
{
    class GeneticAlgorithm
    {
        readonly int p;
        readonly int t;
        readonly int mutationRate;
        readonly int iterations;
        public List<Solution> Population { get; set; }

        public GeneticAlgorithm(int p, int t, int mutationRate, int iterations)
        {
            this.p = p;
            this.t = t;
            this.mutationRate = mutationRate;
            this.iterations = iterations;
            Population = new List<Solution>();
        }

        public Solution FindSolution()
        {
            var random = new Random();
            GeneratePopulation();
            var parent = TournamentSelection();
            for (int i = 0; i < iterations; i++)
            {
                var mutant = parent.Copy();

                var randomNr = random.Next(0, 100);
                if (mutationRate > randomNr)
                {
                    mutant.Mutate();
                }

                ReplaceWorst(mutant);
            }

            return GetBest();
        }

        public void GeneratePopulation()
        {
            for (int i = 0; i < p; i++)
            {
                Population.Add(Solution.GenerateRandom());
            }
        }

        private Solution TournamentSelection()
        {
            List<Solution> TournamentSelectionIndividuals = new List<Solution>();
            List<int> PopulationIndexes = new List<int>();
            for (int i = 0; i < p; i++)
            {
                PopulationIndexes.Add(i);
            }
            Random rnd = new Random();
            for (int i = 0; i < this.t; i++)
            {
                if (i <= p)
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
    }
}
