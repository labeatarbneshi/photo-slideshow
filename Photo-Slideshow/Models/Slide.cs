using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_Slideshow.Models
{
    class Slide
    {
        public List<Photo> Photos { get; set; }

        public List<string> GetSlideTags()
        {
            if(Photos.Count == 2)
            {
                return new List<string>(Photos[0].Tags.Union(Photos[1].Tags));
            }

            return Photos[0].Tags;
        }
    }
}
