namespace PhotoSlideshow.Configuration
{
    public class ConfigurationConsts
    {
        // Total algorithm run duration
        public static long RunDuration = 10000;

        // From 1 - 4 -> Slide swap operator
        public static int SlideSwapUpperFrequency = 4;

        // From this to VerticalPhotoSwapFrequencyUpperLimit -> Photo swap operator
        public static int VerticalPhotoSwapFrequencyLowerLimit = 4;
        public static int VerticalPhotoSwapFrequencyUpperLimit = 9;

        // Shuffle swap operator frequency
        public static int ShuffleOperatorFrequency = 9;

        public static int AcceptWorseSolutionAfterMillis = 60000;
        public static int RetriesAfterBadVerticalSwap = 100;
        public static int AcceptWorseSolutionAfterNoProgressMillis = 120000;

        public static int PerturbationPercentage = 5;
        public static int SlidingWindowPercentage = 10;
        public static int NumberOfGenerations = 5;
        public static int PopSize = 20;
        public static int TournamentSize = 10;
        public static int MutationRate = 80;
        public static int CrossoverRate = 20;
        public static int GAIterations = 10;
        public static int AcceptWorse = 10000011;
    }
}
