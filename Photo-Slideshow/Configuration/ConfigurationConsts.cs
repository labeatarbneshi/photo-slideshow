namespace PhotoSlideshow.Configuration
{
    public class ConfigurationConsts
    {
        // Total algorithm run duration
        public static long RunDuration = 120000;

        // From 1 - 4 -> Slide swap operator
        public static int SlideSwapUpperFrequency = 4;

        // From this to VerticalPhotoSwapFrequencyUpperLimit -> Photo swap operator
        public static int VerticalPhotoSwapFrequencyLowerLimit = 4;
        public static int VerticalPhotoSwapFrequencyUpperLimit = 10;

        // Shuffle swap operator frequency
        public static int ShuffleOperatorFrequency = 11;

        public static int AcceptWorseSolutionAfterMillis = 2000;
        public static int RetriesAfterBadVerticalSwap = 30;
        public static int AcceptWorseSolutionAfterNoProgressMillis = 1000;
    }
}
