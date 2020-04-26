using PhotoSlideshow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSlideshow.Models
{
    class Slideshow
    {
        public List<Slide> Slides { get; set; }
        public int Score { get; set; }
    }
}
