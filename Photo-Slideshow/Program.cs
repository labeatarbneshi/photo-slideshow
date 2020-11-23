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
            var fileStream = new FileStream(@"C:\Users\Arbneshi\Labi\dev\photo-slideshow\Photo-Slideshow\Instances\c_memorable_moments.txt", FileMode.Open, FileAccess.Read);
            var collectionPhotos = new List<Photo>();
            Console.WriteLine("Reading instance content...");
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                int lineNumber = 1;

                while ((line = streamReader.ReadLine()) != null)
                {
                    if (lineNumber == 1)
                    {
                        Collection.Size = int.Parse(line);
                    }
                    else
                    {
                        collectionPhotos.Add(ProcessLine(line, lineNumber));
                    }
                    lineNumber++;
                }
            }

            Console.WriteLine("Photo collection setup finished. Starting inital solution...");
            Collection.Photos = collectionPhotos;
            Collection.HorizontalPhotos = new List<Photo>(collectionPhotos.Where(photo => photo.Orientation == Orientation.HORIZONTAL).ToList());
            Collection.VerticalPhotos  = new List<Photo>(collectionPhotos.Where(photo => photo.Orientation == Orientation.HORIZONTAL).ToList());

            Solution solution = Solution.Generate();

            Console.WriteLine($"[SOLUTION] Total generated slides: {solution.Slideshow.Slides.Count}");
            Console.WriteLine($"{DateTime.Now} Initial solution score: {solution.Score}");

            ILS ils = new ILS();
            ils.FindSolution();
        }

        static Photo ProcessLine(string line, int lineNo)
        {
            string[] lineElements = line.Split(' ');
            Photo photo = new Photo
            {
                Id = lineNo,
                Orientation = lineElements[0] == "H" ? Enums.Orientation.HORIZONTAL : Enums.Orientation.VERTICAL,
                NumberOfTags = int.Parse(lineElements[1]),
                Tags = lineElements.Skip(2).Take(lineElements.Length - 1).ToList()
            };
            return photo;
        }
    }
}
