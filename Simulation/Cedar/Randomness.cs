using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;

public class Randomness
{
	Random rand;

	public Randomness (int seed)
	{
		rand = new Random (seed); 

	}

	public double pickRandomDouble ()
	{
		return rand.NextDouble ();
	}

	public double pickRandomDouble (double minV, double maxV)
	{
		return pickRandomDouble () * (maxV - minV) + minV;
	}

	public int pickRandomInt (int maxRange) // 0 -- maxRange-1
	{
		return pickRandomInt (0, maxRange - 1);
	}

	public int pickRandomInt (int minRange, int maxRange)// both inclusive
	{
		// range has to be the same size as # of ints needed
		int val = (int)Math.Floor (pickRandomDouble () * (maxRange - minRange + 1)) + minRange;

		/*
            if (val == maxRange && maxRange != minRange)
                val -= 1;
             */

		Debug.Assert (val >= minRange && val <= maxRange);
		return val;
	}

	// mean = 0, stdev = 1
	public double GetNormal ()
	{
		// Use Box-Muller algorithm
		double u1 = pickRandomDouble ();
		double u2 = pickRandomDouble ();
		double r = Math.Sqrt (-2.0 * Math.Log (u1));
		double theta = 2.0 * Math.PI * u2;
		return r * Math.Sin (theta);
	}

	public double GetNormalSample (double mean, double stdev)
	{
		return GetNormal () * stdev + mean;
	}

	public double GetLogNormalSample (double mean, double stdev)
	{
		return Math.Exp (GetNormal () * stdev + mean);
	}

	public double GetExponentialSample (double mean)
	{
		//
		// Let X be U [0, 1]
		// note mean = 1/\lambda for exponential
		// Pr (Y < y ) = Pr (-1/\lambda logX < y ) = Pr ( X > exp(-\lambda y)) = 1 - exp (-\lambda y)
		// 
		// Hence Y = -1 * mean * log (X)
		//
		Debug.Assert (mean > 0);
		return -1 * mean * Math.Log (pickRandomDouble ());
	}

	public double GetParetoSample (double shape_alpha, double scale)
	{
		// 
		// Let X be U [0, 1]
		// note scale is x_m. cdf Pr(Y < y ) = 1 - (x_m/y)^\alpha = ... = Pr ( x_m/ X^{1/\alpha}  < y)
		// 
		// Hence Y = x_m/ Pow(X, 1/ \alpha)
		//
		Debug.Assert (shape_alpha > 0 && scale > 0);
		return scale / Math.Pow (pickRandomDouble (), 1.0 / shape_alpha);
	}

	public int[] GetRandomPermutation (int n)
	{
		return GetRandomPermutation (n, n);
	}

	/// <summary>
	/// Random permutation of 0 ... n-1
	/// Second parameter m is optional
	///   m \in [0, n]
	///   when specified it yields only m out of the n values
	/// </summary>
	/// <param name="n"></param>
	/// <param name="m"></param>
	/// <returns></returns>
	public int[] GetRandomPermutation (int n, int m)
	{
		Debug.Assert (m >= 0 && m <= n);

		// return an array with a random permutation of integers 0, 1, ... n-1
		int[] retval = new int[m];

		List<int> allIndices = new List<int> ();

		for (int i = 0; i < n; i++)
			allIndices.Add (i);

		for (int i = 0; i < m; i++) {
			int pick = pickRandomInt (allIndices.Count);
			retval [i] = allIndices [pick];

			allIndices.RemoveAt (pick);
		}

		// Debug, remove once you are sure of this code
		if (m == n) {
			int last = -1;
			foreach (int x in retval.OrderBy(i => i))
				Debug.Assert (x == ++last);
		}

		return retval;
	}

	public static double GetExponentialCdf (double t, double mean)
	{
		return 1 - Math.Exp (-1.0 * t / mean);
	}

	public static double FacebookCdf (double x)
	{
		return GetStandardNormalCdf ((Math.Log (x) - 4.4) / 1.15);
	}
	
	public static double GoogleCdf (double x)
	{
		return GetStandardNormalCdf ((Math.Log (x) - 2.94) / 0.55);
	}
	
	public static double GetLogNormalCdf (double x, double mean, double sigma)
	{
		return GetStandardNormalCdf ((Math.Log (x) - mean) / sigma);
	}
	
	public static double GetNormalCdf (double x, double mean, double sigma)
	{
		return GetStandardNormalCdf ((x - mean) / sigma);
	}

	public static double GetCdf ( double time, DistType type, double mu, double sigma) 
	{
		switch (type) {
		case DistType.Normal:
			return GetNormalCdf (time, mu, sigma);
		case DistType.Exponential:
			return GetExponentialCdf (time, mu);
		default: // Treated as Log Normal
			return GetLogNormalCdf (time, mu, sigma);
		}
	}
	
	public static double GetStandardNormalCdf (double x)
	{
		double a1 = 0.254829592;
		double a2 = -0.284496736;
		double a3 = 1.421413741;
		double a4 = -1.453152027;
		double a5 = 1.061405429;
		double p = 0.3275911;
		
		// Save the sign of x
		double sign = 1;
		if (x < 0)
			sign = -1;
		x = Math.Abs (x) / Math.Sqrt (2.0);
		
		// A&S formula 7.1.26
		double t = 1.0 / (1.0 + p * x);
		double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp (-x * x);
		
		return 0.5 * (1.0 + sign * y);
	}
}
