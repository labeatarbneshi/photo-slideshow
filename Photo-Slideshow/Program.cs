using Photo_Slideshow;
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
            var fileStream = new FileStream(@"C:\dev\photo-slideshow\Photo-Slideshow\Instances\c_memorable_moments.txt", FileMode.Open, FileAccess.Read);
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
                        collection.Photos.Add(ProcessLine(line, lineNumber));
                    }
                    lineNumber++;
                }
            }

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
