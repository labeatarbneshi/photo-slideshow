using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_Slideshow.TwoOpt
{
    public class TwoOptMove
    {
        public int FirstTourIndex { get; set; }
        public int SecondTourIndex { get; set; }
        public int ScoreGain { get; set; }

        public TwoOptMove() { }
        public TwoOptMove(int firstTourIndex, int secondTourIndex, int scoreGain)
        {
            FirstTourIndex = firstTourIndex;
            SecondTourIndex = secondTourIndex;
            ScoreGain = scoreGain;
        }
    }
}
