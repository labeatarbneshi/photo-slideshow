using System.Collections.Generic;

namespace PhotoSlideshow.Models
{
    class Collection
    {
        public static int Size { get; set; }
        public static List<Photo> Photos { get; set; }
        public static List<Photo> VerticalPhotos { get; set; }
        public static List<Photo> HorizontalPhotos { get; set; }
    }
}
