using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Common
{
    public class WilsonScore
    {
        public static double pnormaldist(double qn)
        {
            double[] b = { 1.570796288, 0.03706987906, -0.8364353589e-3, -0.2250947176e-3,
                0.6841218299e-5, 0.5824238515e-5, -0.104527497e-5,
                0.8360937017e-7, -0.3231081277e-8, 0.3657763036e-10,
                0.6936233982e-12 };

            if (qn < 0.0 || 1.0 < qn)
                return 0.0;

            if (qn == 0.5)
                return 0.0;

            double w1 = qn;
            if (qn > 0.5)
                w1 = 1.0 - w1;
            double w3 = -Math.Log(4.0 * w1 * (1.0 - w1));
            w1 = b[0];
            int i = 1;
            for (; i < 11; i++)
                w1 += b[i] * Math.Pow(w3, i);

            if (qn > 0.5)
                return Math.Sqrt(w1 * w3);
            return -Math.Sqrt(w1 * w3);
        }

        public static double Score(double up, double total, double confidence)
        {
            /** Based on http://www.evanmiller.org/how-not-to-sort-by-average-rating.html **/
            if (total <= 0 || total < up)
                return 0;

            double phat = up / total;
            double z2 = confidence * confidence;

            return (phat + z2 / (2 * total) - confidence * Math.Sqrt((phat * (1 - phat) + z2 / (4 * total)) / total)) / (1 + z2 / total);
        }

        public static double Score(int up, int total, double confidence)
        {
            return Score((double)up, (double)total, confidence);
        }
    }
}
