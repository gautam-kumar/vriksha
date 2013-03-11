using System;

	public class Algorithms
	{

		public static double GetOptimalWaitTimeFacebookGoogle (
			double mean, double sigma)
	{// TODO: Fix the 1000 multiplication error, for  now return 0
		/*
			double deadline = GlobalVariables.tlaWaitTimeSec * 1000.0;
			double k = GlobalVariables.NumWorkerPerMla;
			double time = 1.0;
			double utility = 0.0;
			double maxUtility = 0;
			double maxTime = 0;
			double increment = 1.0;
			while (time < deadline) {
			double gain = (Randomness.GetLogNormalCdf (time + increment, 
				                                                mean, sigma)
			               - Randomness.GetLogNormalCdf (time, mean, sigma))
				* Randomness.GoogleCdf (deadline - time - increment);
			double loss = (Randomness.GetLogNormalCdf (time, mean, sigma)
			               - Math.Pow (Randomness.GetLogNormalCdf (time, mean, sigma), k))
				* (Randomness.GoogleCdf (deadline - time) - Randomness.GoogleCdf (deadline - time - increment));
				utility += gain - loss;
				if (utility > maxUtility) {
					maxUtility = utility;
					maxTime = time;
				}
				time += increment;
			}
			//Console.Error.WriteLine("MaxUtility: " + maxUtility);
			//Console.Error.WriteLine("MaxTime: " + maxTime);
			return maxTime;
			*/
		return 0;
		}
		
		public static double GetOptimalWaitTimeLogNormal (
			double mean1, double sigma1,
			double mean2, double sigma2)
		{
		// TODO: Fix the 1000 multiplication error, for  now return 0
		/*
			double deadline = GlobalVariables.tlaWaitTimeSec * 1000.0;
			double k = GlobalVariables.NumWorkerPerMla;
			double time = 0.001;
			double utility = 0.0;
			double maxUtility = 0;
			double maxTime = 0;
			double increment = 1;
			while (time < deadline) {
				double gain = (Randomness.GetLogNormalCdf (time + increment,
				                                                mean1, sigma1)
				               - Randomness.GetLogNormalCdf (time, mean1, sigma1))
					* Randomness.GetLogNormalCdf (deadline - time - increment, mean2, sigma2);
				double loss = (Randomness.GetLogNormalCdf (time, mean1, sigma1)
				               - Math.Pow (Randomness.GetLogNormalCdf (time, mean1, sigma1), k))
					* (Randomness.GetLogNormalCdf (deadline - time, mean2, sigma2) - Randomness.GetLogNormalCdf (deadline - time - increment, mean2, sigma2));
				utility += gain - loss;
				if (utility > maxUtility) {
					maxUtility = utility;
					maxTime = time;
				}
				time += increment;
			}
			//Console.Error.WriteLine("MaxUtility: " + maxUtility);
			//Console.Error.WriteLine("MaxTime: " + maxTime);
			return maxTime;
		*/
		return 0;
		}

	public static TimeSpan GetOptimalWaitTime2 (
		TimeSpan deadline, int k, 
		DistType wType, double mean1, double sigma1, 
		DistType mType, double mean2, double sigma2)
	{

		// TODO : MaxTime is in seconds
		TimeSpan time = new TimeSpan(0, 0, 0, 0, 1);
		double utility = 0.0;
		double maxUtility = 0;
		TimeSpan maxTime;
		TimeSpan increment = new TimeSpan (0, 0, 0, 0, 1);
		while (time < deadline) {
			double gain = (Randomness.GetCdf ((time + increment).TotalMilliseconds,
			                                  wType, mean1, sigma1)
			               - Randomness.GetCdf (time.TotalMilliseconds, wType, mean1, sigma1))
				* Randomness.GetCdf ((deadline - time - increment).TotalMilliseconds, mType, mean2, sigma2);
			double loss = (Randomness.GetCdf (time.TotalMilliseconds, wType, mean1, sigma1)
			               - Math.Pow (Randomness.GetCdf (time.TotalMilliseconds, wType, mean1, sigma1), k))
				* (Randomness.GetCdf ((deadline - time).TotalMilliseconds, mType, mean2, sigma2)
				   - Randomness.GetCdf ((deadline - time - increment).TotalMilliseconds, mType, mean2, sigma2));
			utility += gain - loss;
			
			//Console.Error.WriteLine("At " + time + ": G = " + gain + " L = " + loss + " U = " + utility); 
			if (utility > maxUtility) {
				maxUtility = utility;
				maxTime = time;
			}
			time += increment;
		}
		return maxTime;
	}

	public static TimeSpan GetOptimalWaitTime3 (
		TimeSpan deadline, int k1, int k2, 
		DistType wType, double mean1, double sigma1, 
		DistType mType, double mean2, double sigma2,
		DistType sType, double mean3, double sigma3)
	{

		TimeSpan time = new TimeSpan(0, 0, 0, 0, 1);
		double utility = 0.0;
		double maxUtility = 0;
		TimeSpan maxTime;
		TimeSpan increment = new TimeSpan (0, 0, 0, 0, 1);
		while (time < deadline) {
			// Step 1: Get Optimal time of the second stage
			TimeSpan midTime = new TimeSpan(0, 0, 0, 0, (int) ((deadline - time).TotalMilliseconds / 2));
			TimeSpan midTime2 = Algorithms.GetOptimalWaitTime2(deadline - time, k2,
			                                       mType, mean2, sigma2,
			                                       sType, mean3, sigma3);
			//Console.WriteLine("At time: " + time.TotalMilliseconds + " Half: " + midTime.TotalMilliseconds +
			//                  " Opt: " + midTime2.TotalMilliseconds + " " + mean2 + " " + mean3);
 
			double gain = (Randomness.GetCdf ((time + increment).TotalMilliseconds,
			                                  wType, mean1, sigma1)
			               - Randomness.GetCdf (time.TotalMilliseconds, wType, mean1, sigma1))
				* Randomness.GetCdf ((deadline - time - increment - midTime).TotalMilliseconds, sType, mean3, sigma3);
			double loss = (Randomness.GetCdf (time.TotalMilliseconds, wType, mean1, sigma1)
			               - Math.Pow (Randomness.GetCdf (time.TotalMilliseconds, wType, mean1, sigma1), k1))
				* (Randomness.GetCdf ((deadline - time - midTime).TotalMilliseconds, sType, mean3, sigma3) - Randomness.GetCdf ((deadline - time - increment - midTime).TotalMilliseconds, sType, mean3, sigma3));
			utility += gain - loss;
			if (utility > maxUtility) {
				maxUtility = utility;
				maxTime = time;
			}
			time += increment;
		}
		return maxTime;
	}


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



	}
