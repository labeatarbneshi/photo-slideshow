using PhotoSlideshow.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSlideshow.Models
{
    class Photo
    {
        public int Id { get; set; }
        public int NumberOfTags { get; set; }
        public List<string> Tags { get; set; }
        public Orientation Orientation { get; set; }
    }
}
