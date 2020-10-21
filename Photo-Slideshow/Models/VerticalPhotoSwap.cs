using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSlideshow.Models
{
    public class VerticalPhotoSwap
    {
        public List<Slide> Slides { get; set; }
        public int Gain { get; set; }

        public VerticalPhotoSwap(List<Slide> slides, int gain)
        {
            this.Slides = slides;
            this.Gain = gain;
        }
    }
}
