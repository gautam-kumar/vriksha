#!/usr/bin/python

from scipy.stats import norm
import math
import figure
import plotter


# For each of the individual probabilities, I get the probability that P[X1 + X2 <= t1 + t2]
def GetNormalDistributionResults():
  X = [x * 0.01 for x in range(2, 100, 1)]
  Y = []
  for x in X:
  	t1 = norm.ppf(x)
  	t = math.sqrt(2.0) * t1
  	Y.append(norm.cdf(t))
  return [X, Y]


def GetExponentialDistributionResults(mean):
  X = [x * 0.01 for x in range(2, 100, 1)]
  Y = []
  for x in X:
    t = -1.0 * mean * math.log(1 - x)
    d = 2.0 * t / mean
    Y.append(1 - ((1 + d) * math.exp(-d)))
  return [X, Y]


[X, Y] = GetExponentialDistributionResults(10.0)
XExp = [(1 - (x * x)) for x in X]
YExp = [(1 - (x * x)) / (1 - y) for (x, y) in zip(X, Y)]

[X, Y] = GetNormalDistributionResults()
XNormal = [(1 - (x * x)) for x in X]
YNormal = [(1 - (x * x)) / (1 - y) for (x, y) in zip(X, Y)]
plotter.PlotN([XNormal, XExp], [YNormal, YExp], X='Failure Probability for one stage',  \
    Y='Ratio of Failure Probabilities \nwith Vriksha and Baseline', \
    labels=['Normal', 'Exp'], outputFile='dist', ext='pdf') 
