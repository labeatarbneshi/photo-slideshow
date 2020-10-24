using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSlideshow.Configuration
{
    public class ConfigurationConsts
    {
        public static long RunDuration = 1800000;
        public static int SlideSwapUpperFrequency = 4;
        public static int VerticalPhotoSwapFrequencyLowerLimit = SlideSwapUpperFrequency;
        public static int VerticalPhotoSwapFrequencyUpperLimit = 7;
        public static int ShuffleOperatorFrequency = VerticalPhotoSwapFrequencyUpperLimit;
        public static int AcceptBadSolutionAfterMillis = 5000;
        public static int RetriesAfterBadVerticalSwap = 15;
        public static bool SkipVerticalSwap = false;
        public static int AcceptWorseSolutionAfterNoProgressMillis = 0;
    }
}
