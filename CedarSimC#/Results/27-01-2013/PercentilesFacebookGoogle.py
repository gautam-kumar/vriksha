#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Policies = ['Deadline / 2', 'Static Timeout', 'Dynamic Timeout']
meanQuality = {}
Perc99 = {}
Perc99_9 = {}
Deadlines = [150.0, 250.0, 350.0, 450.0, 550.0, 650.0]

numRequestsToSimulate = 2000
policy = Policies[0]
lines = open("LogFacebookGoogleHalfDeadline.txt").readlines()
i = 0
meanQuality[policy] = []
Perc99[policy] = []
Perc99_9[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  i += 1
	  meanQuality[policy].append(float(splits[9]) / numRequestsToSimulate)
	  Perc99[policy].append(float(splits[11]) / numRequestsToSimulate)
	  Perc99_9[policy].append(float(splits[13]) / numRequestsToSimulate)

policy = Policies[1]
lines = open("LogFacebookGoogleStaticTimeout.txt").readlines()
i = 0
meanQuality[policy] = []
Perc99[policy] = []
Perc99_9[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  i += 1
	  meanQuality[policy].append(float(splits[9]) / numRequestsToSimulate)
	  Perc99[policy].append(float(splits[11]) / numRequestsToSimulate)
	  Perc99_9[policy].append(float(splits[13]) / numRequestsToSimulate)

policy = Policies[2]
lines = open("LogFacebookGoogleDynamic.txt").readlines()
i = 0
meanQuality[policy] = []
Perc99[policy] = []
Perc99_9[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  i += 1
	  meanQuality[policy].append(float(splits[9]) / numRequestsToSimulate)
	  Perc99[policy].append(float(splits[11]) / numRequestsToSimulate)
	  Perc99_9[policy].append(float(splits[13]) / numRequestsToSimulate)
 

YMean = []
Y99 = []
Y99_9 = []
X = []
for p in Policies:
	  YMean.append(meanQuality[p])
	  Y99.append(Perc99[p])
	  Y99_9.append(Perc99_9[p])
	  X.append(Deadlines)

plotter.PlotN(X, YMean, 
    X='Deadline (ms)', Y='Fraction of Requests', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 1.01], \
    outputFile="PercentilesFacebookGoogleMean")

plotter.PlotN(X, Y99, 
    X='Deadline (ms)', Y='Fraction of Requests', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 1.01], \
    outputFile="PercentilesFacebookGoogle99")

plotter.PlotN(X, Y99_9, 
    X='Deadline (ms)', Y='Fraction of Requests', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 1.01], \
    outputFile="PercentilesFacebookGoogle99_9")
