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
		DistType wType, double mean1, double sigma1, 
		DistType mType, double mean2, double sigma2)
	{

		// TODO : MaxTime is in seconds
		double deadline = GlobalVariables.tlaWaitTimeSec.TotalMilliseconds;
		double k = GlobalVariables.NumWorkerPerMla;
		double time = 0.0001;
		double utility = 0.0;
		double maxUtility = 0;
		double maxTime = 0;
		double increment = 0.001;
		while (time < deadline) {
			double gain = (Randomness.GetCdf (time + increment,
			                                  wType, mean1, sigma1)
			               - Randomness.GetCdf (time, wType, mean1, sigma1))
				* Randomness.GetCdf (deadline - time - increment, mType, mean2, sigma2);
			double loss = (Randomness.GetCdf (time, wType, mean1, sigma1)
			               - Math.Pow (Randomness.GetCdf (time, wType, mean1, sigma1), k))
				* (Randomness.GetCdf (deadline - time, mType, mean2, sigma2)
				   - Randomness.GetCdf (deadline - time - increment, mType, mean2, sigma2));
			utility += gain - loss;
			if (utility > maxUtility) {
				maxUtility = utility;
				maxTime = time;
			}
			time += increment;
		}
		return new TimeSpan(0, 0, 0, 0, (int) maxTime);
	}

	public static TimeSpan GetOptimalWaitTime3 (
		DistType wType, double mean1, double sigma1, 
		DistType mType, double mean2, double sigma2,
		DistType sType, double mean3, double sigma3)
	{

		// TODO: max time is in seconds
		double deadline = GlobalVariables.tlaWaitTimeSec.TotalMilliseconds;
		double k = GlobalVariables.NumWorkerPerMla;
		double time = 0.0001;
		double utility = 0.0;
		double maxUtility = 0;
		double maxTime = 0;
		double increment = 0.001;
		while (time < deadline) {
			double gain = (Randomness.GetCdf (time + increment,
			                                  wType, mean1, sigma1)
			               - Randomness.GetCdf (time, wType, mean1, sigma1))
				* Randomness.GetCdf (deadline - time - increment, mType, mean2, sigma2);
			double loss = (Randomness.GetCdf (time, wType, mean1, sigma1)
			               - Math.Pow (Randomness.GetCdf (time, wType, mean1, sigma1), k))
				* (Randomness.GetCdf (deadline - time, sType, mean2, sigma2) - Randomness.GetCdf (deadline - time - increment, mType, mean2, sigma2));
			utility += gain - loss;
			if (utility > maxUtility) {
				maxUtility = utility;
				maxTime = time;
			}
			time += increment;
		}
		return new TimeSpan(0, 0, 0, 0, (int) maxTime);
	}



	}
