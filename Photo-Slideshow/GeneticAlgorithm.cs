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
            this.Population = new List<Solution>();
        }

        public Solution FindSolution()
        {
            GenerateSolution();
        }

        public void GenerateSolution()
        {
            for(int i = 0; i < p; i++)
            {
                Population.Add(new Solution());
            }
        }

    }
}
