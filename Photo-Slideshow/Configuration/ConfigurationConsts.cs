using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSlideshow.Configuration
{
    public class ConfigurationConsts
    {
        // Total algorithm run duration
        public static long RunDuration = 1800000;

        // From 1 - 4 -> Slide swap operator
        public static int SlideSwapUpperFrequency = 3;

        // From this to VerticalPhotoSwapFrequencyUpperLimit -> Photo swap operator
        public static int VerticalPhotoSwapFrequencyLowerLimit = 3;
        public static int VerticalPhotoSwapFrequencyUpperLimit = 9;

        // Shuffle swap operator frequency
        public static int ShuffleOperatorFrequency = 9;

        public static int AcceptWorseSolutionAfterMillis = 5000;
        public static int RetriesAfterBadVerticalSwap = 110;
        public static int AcceptWorseSolutionAfterNoProgressMillis = 0;
    }
}
