namespace PhotoSlideshow.Configuration
{
    public class ConfigurationConsts
    {
        // Total algorithm run duration
        public static long RunDuration = 99999999999999999L;

        // From 1 - 4 -> Slide swap operator
        public static int SlideSwapUpperFrequency = 8;

        // From this to VerticalPhotoSwapFrequencyUpperLimit -> Photo swap operator
        public static int VerticalPhotoSwapFrequencyLowerLimit = 41;
        public static int VerticalPhotoSwapFrequencyUpperLimit = 81;

        // Shuffle swap operator frequency
        public static int ShuffleOperatorFrequency = 8;

        public static int AcceptWorseSolutionAfterMillis = 999999999;
        public static int RetriesAfterBadVerticalSwap = 30;
        public static int AcceptWorseSolutionAfterNoProgressMillis = 1000;
    }
}
