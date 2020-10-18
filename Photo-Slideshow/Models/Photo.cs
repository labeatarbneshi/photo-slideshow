using PhotoSlideshow.Enums;
using System.Collections.Generic;

namespace PhotoSlideshow.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public int NumberOfTags { get; set; }
        public List<string> Tags { get; set; }
        public Orientation Orientation { get; set; }
    }
}
