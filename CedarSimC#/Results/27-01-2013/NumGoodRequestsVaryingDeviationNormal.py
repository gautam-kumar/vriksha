#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Stdev = ['20ms', '25ms', '30ms', '35ms', '40ms', '45ms', '50ms']
lines = open("LogNorm.txt").readlines()
i = 0
num80 = {}
num90 = {}
num95 = {}
num99 = {}
waitTimes = {}
for d in Stdev:
    num80[d] = []
    num90[d] = []
    num95[d] = []
    num99[d] = []
    waitTimes[d] = []
    for j in range(9):
	    splits = lines[i].split()
	    i += 1
	    waitTimes[d].append(float(splits[1]) * 1000.0)
	    numRequestsCompleted = float(splits[17])
	    numRequestsCompleted = 100
	    num80[d].append(float(splits[9]) / numRequestsCompleted)
	    num90[d].append(float(splits[11]) / numRequestsCompleted)
	    num95[d].append(float(splits[13]) / numRequestsCompleted)
	    num99[d].append(float(splits[15]) / numRequestsCompleted)
    
Y80 = []
Y90 = []
X = []
for d in Stdev:
	  Y80.append(num80[d])
	  Y90.append(num90[d])
	  X.append(waitTimes[d])
plotter.PlotN(X, Y80, 
    X='Wait Time (ms)', Y='Fraction of Requests', \
    labels=Stdev, legendSize=12, legendTitle='St. Dev.',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsNorm-80perc")


plotter.PlotN(X, Y90, 
    X='Wait Time (ms)', Y='Fraction of Requests', \
    labels=Stdev, legendSize=12, legendTitle='St. Dev.',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsNorm-90perc")
