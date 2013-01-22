#!/usr/bin/env python
import plotter
import os
import sys
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")

k = 40
mean1 = 80.0
mean2 = 100.0
sigma1 = 20.0
sigma2 = 25.0
T = [0.1 * x for x in range(2001)]



def MinNormCdf(t, mean, sigma):
  return 1 - math.pow(1 - stats.norm.cdf((t - mean) / sigma), k);

def MaxNormCdf(t, mean, sigma):
  return math.pow(stats.norm.cdf((t - mean) / sigma), k);

YMinNorm1 = [MinNormCdf(t, mean1, sigma1) for t in T]
YMinNorm2 = [MinNormCdf(t, mean2, sigma2) for t in T]
YMaxNorm1 = [MaxNormCdf(t, mean1, sigma1) for t in T]
YMaxNorm2 = [MaxNormCdf(t, mean2, sigma2) for t in T]
plotter.PlotN([T, T, T, T], [YMinNorm1, YMinNorm2, YMaxNorm1, YMaxNorm2], Y="CDF", X="Time", \
  labels=['Min 80ms', 'Min 100ms', 'Max 80ms', 'Max 100ms'], outputFile="MinMaxNormals", \
  xAxis=[0, 200], yAxis=[0, 1.0], ext="pdf")

def InvCdf(X, Y, val):
  for i in range(len(X)):
    if Y[i] >= val:
    	return X[i]


print "Min1 99:", InvCdf(T, YMinNorm1, 0.99)
print "Min2 99:", InvCdf(T, YMinNorm2, 0.99)
print "Max1 99:", InvCdf(T, YMaxNorm1, 0.99)
print "Max2 99:", InvCdf(T, YMaxNorm2, 0.99)



