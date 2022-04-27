using PhotoSlideshow.Enums;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PhotoSlideshow
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadFile();
            Console.ReadKey();
        }

        static void ReadFile()
        {
            var fileStream = new FileStream(@"C:\Users\Labeat\dev\photo-slideshow\Photo-Slideshow\Instances\c_memorable_moments.txt", FileMode.Open, FileAccess.Read);

            Console.WriteLine("Reading instance content...");
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                int lineNumber = 1;
                Collection.Photos = new List<Photo>();
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (lineNumber == 1)
                    {
                        Collection.Size = int.Parse(line);
                    }
                    else
                    {
                        Collection.Photos.Add(ProcessLine(line, lineNumber));
                    }
                    lineNumber++;
                }
            }

            Collection.HorizontalPhotos = new List<Photo>(Collection.Photos.Where(photo => photo.Orientation == Orientation.HORIZONTAL).ToList());
            Collection.VerticalPhotos = new List<Photo>(Collection.Photos.Where(photo => photo.Orientation == Orientation.VERTICAL).ToList());
            Console.WriteLine("Photo collection setup finished. Starting inital solution...");

            //GeneticAlgorithm ga = new GeneticAlgorithm(20, 10, 80, 50, 2);
            //var a = ga.FindSolution();
            //Console.WriteLine($"{DateTime.Now} Finished with score:  {a.Slideshow.Score}");

            var solution = Solution.GenerateRandom();
            ILS iteratedLocalSearch = new ILS();
            iteratedLocalSearch.FindSolution(1, solution);
            Common.SaveSolution(solution.Slideshow, "e_solution");
        }

        static Photo ProcessLine(string line, int lineNo)
        {
            string[] lineElements = line.Split(' ');
            Photo photo = new Photo
            {
                Id = lineNo,
                Orientation = lineElements[0] == "H" ? Orientation.HORIZONTAL : Orientation.VERTICAL,
                NumberOfTags = int.Parse(lineElements[1]),
                Tags = lineElements.Skip(2).Take(lineElements.Length - 1).ToList()
            };
            return photo;
        }
    }
}
