using PhotoSlideshow.Enums;
using System.Collections.Generic;
using System.Linq;

namespace PhotoSlideshow.Models
{
    public class Collection
    {
        public static int Size { get; set; }
        public static List<Photo> Photos { get; set; }
        public static List<Photo> VerticalPhotos { get; set; }
        public static List<Photo> HorizontalPhotos { get; set; }
    }
}
