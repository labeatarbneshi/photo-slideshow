﻿using System.Collections.Generic;
using System.Linq;

namespace PhotoSlideshow.Models
{
    public class Slide
    {
        public int Id { get; set; }
        public List<Photo> Photos { get; set; }
        public List<Photo> ComparedPhotos { get; set; } = new List<Photo>();
        public List<string> GetTags()
        {
            if(Photos.Count == 2)
            {
                return new List<string>(Photos[0].Tags.Union(Photos[1].Tags));
            }

            return Photos[0].Tags;
        }
    }
}
