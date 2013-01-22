#!/usr/bin/env python
import plotter
import os
import sys
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")

k = 40
l1 = 1.0 / 100.0
l2 = 1.0 / 100.0
T = [0.1 * x for x in range(2001)]
D = [200, 220, 240, 260, 280, 300]

mean1 = 100.0; 
mean2 = 100.0;
sigma1 = 20.0;
sigma2 = 20.0;

X = []
YExp = []
YNormal = []

for d in D:
	YNormal.append([k * stats.norm.cdf((t - mean1) / sigma1) * stats.norm.cdf((d - t - mean2) / sigma2) for t in T])
	YExp.append([k * (1 - math.exp(-l1 * t)) * (1 - math.exp(-l2 * (d - t))) for t in T])
	X.append(T)
  

plotter.PlotN(X, YExp, Y="Expected Utility", X="Wait Time", labels=['200ms', '220ms', '240ms', '260ms', '280ms', '300ms'], outputFile="UtilityAnalyticalExp", xAxis=[0, 200], yAxis=[0, 40], ext="pdf")


print YNormal[-1]
plotter.PlotN(X, YNormal, Y="Expected Utility", X="Wait Time", labels=['200ms', '220ms', '240ms', '260ms', '280ms', '300ms'], outputFile="UtilityAnalyticalNormal", xAxis=[0, 200], yAxis=[0, 40], ext="pdf")





