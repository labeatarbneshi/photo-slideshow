using PhotoSlideshow;
using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Collection collection = new Collection();
            var fileStream = new FileStream(@"C:\Users\Arbneshi\Labi\dev\photo-slideshow\Photo-Slideshow\Instances\c_memorable_moments.txt", FileMode.Open, FileAccess.Read);

            Console.WriteLine("Reading instance content...");
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                int lineNumber = 1;
                collection.Photos = new List<Photo>();
                while ((line = streamReader.ReadLine()) != null)
                {
                    if(lineNumber == 1)
                    {
                        collection.Size = int.Parse(line);
                    }
                    else
                    {
                        //if(lineNumber > 25)
                        //{
                        //    break;
                        //}
                        //var photoLine = ProcessLine(line, lineNumber);
                        //if (photoLine.Orientation == Enums.Orientation.HORIZONTAL)
                        //{
                            collection.Photos.Add(ProcessLine(line, lineNumber));
                        //}
                    }
                    lineNumber++;
                }
            }

            Console.WriteLine("Photo collection setup finished. Starting inital solution...");
            Solution solution = new Solution(collection);
            solution.Generate();
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
