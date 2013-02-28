#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Mean = ['35ms', '40ms', '45ms', '50ms', '55ms', '60ms']
lines = open("LogExp.txt").readlines()
i = 0
num80 = {}
num90 = {}
num95 = {}
num99 = {}
waitTimes = {}
for d in Mean:
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
for d in Mean:
	  Y80.append(num80[d])
	  Y90.append(num90[d])
	  X.append(waitTimes[d])
plotter.PlotN(X, Y80, 
    X='Wait Time (ms)', Y='Fraction of Requests', \
    labels=Mean, legendSize=12, legendTitle='Mean',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsExp-80perc")


plotter.PlotN(X, Y90, 
    X='Wait Time (ms)', Y='Fraction of Requests', \
    labels=Mean, legendSize=12, legendTitle='Mean',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsExp-90perc")
