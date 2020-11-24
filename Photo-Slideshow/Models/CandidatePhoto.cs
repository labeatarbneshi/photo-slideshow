namespace PhotoSlideshow.Models
{
    public class CandidatePhoto
    {
        public int Id { get; set; }
        public Photo Photo { get; set; }
        public int Score { get; set; }

        public CandidatePhoto(int id, Photo photo, int score)
        {
            Id = id;
            Photo = photo;
            Score = score;
        }
    }
}
