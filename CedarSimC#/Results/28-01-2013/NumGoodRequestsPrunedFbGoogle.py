#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Policies = ['Deadline / 2', 'Prop', 'Vriksha']
num80 = {}
num90 = {}
num95 = {}
num99 = {}
Deadlines = [150.0, 175.0, 200.0, 225.0]

numRequestsToSimulate = 1600
policy = Policies[0]
lines = open("LogPrunedDataHalf.txt").readlines()
i = 0
num80[policy] = []
num90[policy] = []
num95[policy] = []
num99[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  assert len(splits) == 32
	  i += 1
	  num80[policy].append(float(splits[23]) / numRequestsToSimulate)
	  num90[policy].append(float(splits[25]) / numRequestsToSimulate)
	  num95[policy].append(float(splits[27]) / numRequestsToSimulate)
	  num99[policy].append(float(splits[29]) / numRequestsToSimulate)

policy = Policies[1]
lines = open("LogPrunedDataProp.txt").readlines()
i = 0
num80[policy] = []
num90[policy] = []
num95[policy] = []
num99[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  assert len(splits) == 32
	  i += 1
	  num80[policy].append(float(splits[23]) / numRequestsToSimulate)
	  num90[policy].append(float(splits[25]) / numRequestsToSimulate)
	  num95[policy].append(float(splits[27]) / numRequestsToSimulate)
	  num99[policy].append(float(splits[29]) / numRequestsToSimulate)

policy = Policies[2]
lines = open("LogPrunedDataDynamicVriksha.txt").readlines()
i = 0
num80[policy] = []
num90[policy] = []
num95[policy] = []
num99[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  assert len(splits) == 32
	  i += 1
	  num80[policy].append(float(splits[23]) / numRequestsToSimulate)
	  num90[policy].append(float(splits[25]) / numRequestsToSimulate)
	  num95[policy].append(float(splits[27]) / numRequestsToSimulate)
	  num99[policy].append(float(splits[29]) / numRequestsToSimulate)
 

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
    outputFile="NumGoodRequestsPruned80perc")

plotter.PlotN(X, Y90, 
    X='Deadline (ms)', Y='Fraction of Requests', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsPruned90perc")

plotter.PlotN(X, Y95, 
    X='Deadline (ms)', Y='Fraction of Requests', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsPruned95perc")

plotter.PlotN(X, Y99, 
    X='Deadline (ms)', Y='Fraction of Requests', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsPruned99perc")

