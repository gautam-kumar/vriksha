using System;

	public class Algorithms
	{

	public static Tuple<TimeSpan, double> GetOptimal1 (
		TimeSpan deadline, 
		DistType t1, double mean1, double sigma1) {
		return new Tuple<TimeSpan, double> (deadline, 
		                 Randomness.GetCdf ((deadline).TotalMilliseconds, t1, mean1, sigma1));
	}

	public static Tuple<TimeSpan, double> GetOptimal2 (
		TimeSpan deadline, int k, 
		DistType t1, double mean1, double sigma1, 
		DistType t2, double mean2, double sigma2)
	{
		// TODO : MaxTime is in seconds
		TimeSpan time = new TimeSpan(0, 0, 0, 0, 1);
		double quality = 0.0;
		double maxQuality = 0;
		TimeSpan maxTime;
		TimeSpan increment = new TimeSpan (0, 0, 0, 0, 2);
		while (time < deadline) {
			double aboveQualityWithoutWait = GetOptimal1(deadline - time,
			                                             t2, mean2, sigma2).Item2;
			double aboveQualityWithWait = GetOptimal1(deadline - time - increment,
			                                          t2, mean2, sigma2).Item2;
			double gain = (Randomness.GetCdf ((time + increment).TotalMilliseconds,
			                                  t1, mean1, sigma1)
			               - Randomness.GetCdf (time.TotalMilliseconds, t1, mean1, sigma1)
			               ) * aboveQualityWithWait;
			double loss = (Randomness.GetCdf (time.TotalMilliseconds, t1, mean1, sigma1)
			               - Math.Pow (Randomness.GetCdf (time.TotalMilliseconds, t1, mean1, sigma1), k))
						  * (aboveQualityWithoutWait - aboveQualityWithWait);
			quality += gain - loss;
			if (quality > maxQuality) {
				maxQuality = quality;
				maxTime = time;
			}
			time += increment;
		}
		return new Tuple<TimeSpan, double> (maxTime, maxQuality);
	}


	public static Tuple<TimeSpan, double> GetOptimal3 (
		TimeSpan deadline, int k1, int k2, 
		DistType type1, double mean1, double sigma1, 
		DistType type2, double mean2, double sigma2,
		DistType type3, double mean3, double sigma3)
	{

		TimeSpan time = new TimeSpan(0, 0, 0, 0, 1);
		double quality = 0.0;
		double maxQuality = 0;
		TimeSpan maxTime;
		TimeSpan increment = new TimeSpan (0, 0, 0, 0, 2);
		while (time < deadline) {
			// Step 1: Get Optimal quality of the second stage
			double aboveQualityWithoutWait = GetOptimal2(deadline - time, k2,
			                                       type2, mean2, sigma2,
			                                       type3, mean3, sigma3).Item2;
			double aboveQualityWithWait = GetOptimal2(deadline - time - increment, k2,
			                                          type2, mean2, sigma2,
			                                          type3, mean3, sigma3).Item2;

			//Console.WriteLine("At time: " + time.TotalMilliseconds + " Half: " + midTime.TotalMilliseconds +
			//                  " Opt: " + midTime2.TotalMilliseconds + " " + mean2 + " " + mean3);
 			double gain = (Randomness.GetCdf ((time + increment).TotalMilliseconds,
			                                  type1, mean1, sigma1)
			               - Randomness.GetCdf (time.TotalMilliseconds, type1, mean1, sigma1)
			               ) * aboveQualityWithWait;
			double loss = (Randomness.GetCdf (time.TotalMilliseconds, type1, mean1, sigma1)
			               - Math.Pow (Randomness.GetCdf (time.TotalMilliseconds, type1, mean1, sigma1), k1)
			               ) * (aboveQualityWithoutWait - aboveQualityWithWait);
			quality += gain - loss;
			if (quality > maxQuality) {
				maxQuality = quality;
				maxTime = time;
			}
			time += increment;
		}
		return new Tuple<TimeSpan, double> (maxTime, maxQuality);
	}


	public static Tuple<TimeSpan, double> GetOptimal4 (
		TimeSpan deadline, int k1, int k2, int k3, 
		DistType type1, double mean1, double sigma1, 
		DistType type2, double mean2, double sigma2,
		DistType type3, double mean3, double sigma3,
		DistType type4, double mean4, double sigma4)
	{
		
		TimeSpan time = new TimeSpan(0, 0, 0, 0, 1);
		double quality = 0.0;
		double maxQuality = 0;
		TimeSpan maxTime;
		TimeSpan increment = new TimeSpan (0, 0, 0, 0, 2);
		while (time < deadline) {
			// Step 1: Get Optimal quality of the second stage
			double aboveQualityWithoutWait = GetOptimal3(deadline - time, k2, k3, 
			                                             type2, mean2, sigma2,
			                                             type3, mean3, sigma3,
			                                             type4, mean4, sigma4).Item2;
			double aboveQualityWithWait = GetOptimal3(deadline - time - increment, k2, k3,
			                                          type2, mean2, sigma2,
			                                          type3, mean3, sigma3,
			                                          type4, mean4, sigma4).Item2;
			
			//Console.WriteLine("At time: " + time.TotalMilliseconds + " Half: " + midTime.TotalMilliseconds +
			//                  " Opt: " + midTime2.TotalMilliseconds + " " + mean2 + " " + mean3);
			double gain = (Randomness.GetCdf ((time + increment).TotalMilliseconds,
			                                  type1, mean1, sigma1)
			               - Randomness.GetCdf (time.TotalMilliseconds, type1, mean1, sigma1)
			               ) * aboveQualityWithWait;
			double loss = (Randomness.GetCdf (time.TotalMilliseconds, type1, mean1, sigma1)
			               - Math.Pow (Randomness.GetCdf (time.TotalMilliseconds, type1, mean1, sigma1), k1)
			               ) * (aboveQualityWithoutWait - aboveQualityWithWait);
			quality += gain - loss;
			if (quality > maxQuality) {
				maxQuality = quality;
				maxTime = time;
			}
			time += increment;
		}
		return new Tuple<TimeSpan, double> (maxTime, maxQuality);
	}

	public static TimeSpan[] GetProportionalSplit(TimeSpan d, double[] means) {
		double sum = 0;
		foreach (double m in means) {
			sum += m;
		}
		TimeSpan[] a = new TimeSpan[means.Length];
		double remaining = sum;
		for (int i = 0; i < means.Length; i++) {
			remaining = remaining - means [i];
			a [i] = new TimeSpan ((Int64) (d.Ticks * (remaining / sum)));
		}
		return a;
	}


	/*
	public static TimeSpan GetOptimalWaitTime4 (
		TimeSpan deadline,
		int k1, int k2, int k3,
		DistType wType, double mean1, double sigma1, 
		DistType mType, double mean2, double sigma2,
		DistType sType, double mean3, double sigma3,
		DistType tType, double mean4, double sigma4)
	{
		
		TimeSpan time = new TimeSpan(0, 0, 0, 0, 1);
		double utility = 0.0;
		double maxUtility = 0;
		TimeSpan maxTime;
		TimeSpan increment = new TimeSpan (0, 0, 0, 0, 1);
		while (time < deadline) {
			// Step 1: Get Optimal time of the second stage
			TimeSpan midTime = new TimeSpan(0, 0, 0, 0, (int) (2 * (deadline - time).TotalMilliseconds / 3));
			//Console.WriteLine("At time: " + time.TotalMilliseconds + " Half: " + midTime.TotalMilliseconds +
			//                  " Opt: " + midTime2.TotalMilliseconds + " " + mean2 + " " + mean3);
			
			double gain = (Randomness.GetCdf ((time + increment).TotalMilliseconds,
			                                  wType, mean1, sigma1)
			               - Randomness.GetCdf (time.TotalMilliseconds, wType, mean1, sigma1))
				* Randomness.GetCdf ((deadline - time - increment - midTime).TotalMilliseconds, tType, mean4, sigma4);
			double loss = (Randomness.GetCdf (time.TotalMilliseconds, wType, mean1, sigma1)
			               - Math.Pow (Randomness.GetCdf (time.TotalMilliseconds, wType, mean1, sigma1), k1))
				* (Randomness.GetCdf ((deadline - time - midTime).TotalMilliseconds, tType, mean4, sigma4)
				   - Randomness.GetCdf ((deadline - time - increment - midTime).TotalMilliseconds, tType, mean4, sigma4));
			utility += gain - loss;
			if (utility > maxUtility) {
				maxUtility = utility;
				maxTime = time;
			}
			time += increment;
		}
		return maxTime;
	}
	*/


	}
