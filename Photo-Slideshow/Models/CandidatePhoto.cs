using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSlideshow.Models
{
    public class CandidatePhoto
    {
        public int Id { get; set; }
        public Photo Photo { get; set; }
        public int Score { get; set; }
    }
}
