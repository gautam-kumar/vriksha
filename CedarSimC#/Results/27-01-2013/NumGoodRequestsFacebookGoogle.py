#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Policies = ['Deadline / 2', 'Static Timeout', 'Dynamic Timeout']
num80 = {}
num90 = {}
num95 = {}
num99 = {}
Deadlines = [150.0, 250.0, 350.0, 450.0, 550.0, 650.0, 750.0]

numRequestsToSimulate = 2000
policy = Policies[0]
lines = open("LogFacebookGoogleHalfDeadline.txt").readlines()
i = 0
num80[policy] = []
num90[policy] = []
num95[policy] = []
num99[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  i += 1
	  num80[policy].append(float(splits[9]) / numRequestsToSimulate)
	  num90[policy].append(float(splits[11]) / numRequestsToSimulate)
	  num95[policy].append(float(splits[13]) / numRequestsToSimulate)
	  num99[policy].append(float(splits[15]) / numRequestsToSimulate)

policy = Policies[1]
lines = open("LogFacebookGoogleStaticTimeout.txt").readlines()
i = 0
num80[policy] = []
num90[policy] = []
num95[policy] = []
num99[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  i += 1
	  num80[policy].append(float(splits[9]) / numRequestsToSimulate)
	  num90[policy].append(float(splits[11]) / numRequestsToSimulate)
	  num95[policy].append(float(splits[13]) / numRequestsToSimulate)
	  num99[policy].append(float(splits[15]) / numRequestsToSimulate)

policy = Policies[2]
lines = open("LogFacebookGoogleDynamicAcrossAggregators.txt").readlines()
i = 0
num80[policy] = []
num90[policy] = []
num95[policy] = []
num99[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  i += 1
	  num80[policy].append(float(splits[9]) / numRequestsToSimulate)
	  num90[policy].append(float(splits[11]) / numRequestsToSimulate)
	  num95[policy].append(float(splits[13]) / numRequestsToSimulate)
	  num99[policy].append(float(splits[15]) / numRequestsToSimulate)
 

Y80 = []
Y90 = []
Y95 = []
Y99 = []
X = []
for p in Policies:
	  Y80.append(num80[p])
	  Y90.append(num90[p])
	  Y95.append(num95[p])
	  Y99.append(num99[p])
	  X.append(Deadlines)

plotter.PlotN(X, Y80, 
    X='Deadline (ms)', Y='Fraction of Requests', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsFacebookGoogle80perc")

plotter.PlotN(X, Y90, 
    X='Deadline (ms)', Y='Fraction of Requests', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsFacebookGoogle90perc")

plotter.PlotN(X, Y95, 
    X='Deadline (ms)', Y='Fraction of Requests', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsFacebookGoogle95perc")

plotter.PlotN(X, Y99, 
    X='Deadline (ms)', Y='Fraction of Requests', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsFacebookGoogle99perc")

