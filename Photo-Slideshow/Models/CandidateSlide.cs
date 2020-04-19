using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_Slideshow.Models
{
    public class CandidateSlide
    {
        public int Id { get; set; }
        public Slide Slide { get; set; }
        public bool IsUsed { get; set; }
        public int Score { get; set; }
    }
}
