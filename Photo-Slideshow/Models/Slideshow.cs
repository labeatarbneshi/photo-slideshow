﻿using System.Collections.Generic;

namespace PhotoSlideshow.Models
{
    public class Slideshow
    {
        public List<Slide> Slides { get; set; }
        public int Score { get; set; }

        public Slideshow()
        {

        }
        public Slideshow(List<Slide> slides, int score)
        {
            Slides = slides;
            Score = score;
        }
    }
}
