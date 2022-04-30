namespace PhotoSlideshow.Models
{
    public class CrossoverSlide
    {
        public Slide Slide { get; set; }
        public int Index { get; set; }

        public CrossoverSlide(Slide slide, int index)
        {
            Slide = slide;
            Index = index;
        }
    }
}
