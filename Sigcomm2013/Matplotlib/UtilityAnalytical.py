#!/usr/bin/env python
import plotter
import os
import sys
import math
k = 40
l1 = 1.0 / 45.0
l2 = 1.0 / 100.0
T = [0.1 * x for x in range(1501)]
D = [150, 170, 190, 210, 230, 250]

X = []
Y = []

for d in D:
	Y.append([k * (1 - math.exp(-l1 * t)) * (1 - math.exp(-l2 * (d - t))) for t in T])
	X.append(T)

print Y[0]
print X[0]

plotter.PlotN(X, Y, Y="Expected Utility", X="Wait Time", labels=['150ms', '170ms', '190ms', '210ms', '230ms', '250ms'], outputFile="UtilityAnalytical", xAxis=[0, 150], yAxis=[0, 30], ext="pdf")


