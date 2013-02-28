#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Policies = ['Deadline / 2', 'Prop', 'Vriksha']
perc50 = {}
perc60 = {}
perc70 = {}
perc80 = {}
perc90 = {}
perc95 = {}
perc98 = {}
perc99 = {}
perc99_9 = {}
Deadlines = [150.0, 175.0, 200.0, 225.0]

numRequestsToSimulate = 1 
policy = Policies[0]
lines = open("LogPrunedDataHalf.txt").readlines()
i = 0
perc50[policy] = []
perc60[policy] = []
perc70[policy] = []
perc80[policy] = []
perc90[policy] = []
perc95[policy] = []
perc98[policy] = []
perc99[policy] = []
perc99_9[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  assert len(splits) == 32
	  i += 1
	  perc50[policy].append(float(splits[5]) / numRequestsToSimulate)
	  perc60[policy].append(float(splits[7]) / numRequestsToSimulate)
	  perc70[policy].append(float(splits[9]) / numRequestsToSimulate)
	  perc80[policy].append(float(splits[11]) / numRequestsToSimulate)
	  perc90[policy].append(float(splits[13]) / numRequestsToSimulate)
	  perc95[policy].append(float(splits[15]) / numRequestsToSimulate)
	  perc98[policy].append(float(splits[17]) / numRequestsToSimulate)
	  perc99[policy].append(float(splits[19]) / numRequestsToSimulate)
	  perc99_9[policy].append(float(splits[21]) / numRequestsToSimulate)



policy = Policies[1]
lines = open("LogPrunedDataProp.txt").readlines()
i = 0
perc50[policy] = []
perc60[policy] = []
perc70[policy] = []
perc80[policy] = []
perc90[policy] = []
perc95[policy] = []
perc98[policy] = []
perc99[policy] = []
perc99_9[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  assert len(splits) == 32
	  i += 1
	  perc50[policy].append(float(splits[5]) / numRequestsToSimulate)
	  perc60[policy].append(float(splits[7]) / numRequestsToSimulate)
	  perc70[policy].append(float(splits[9]) / numRequestsToSimulate)
	  perc80[policy].append(float(splits[11]) / numRequestsToSimulate)
	  perc90[policy].append(float(splits[13]) / numRequestsToSimulate)
	  perc95[policy].append(float(splits[15]) / numRequestsToSimulate)
	  perc98[policy].append(float(splits[17]) / numRequestsToSimulate)
	  perc99[policy].append(float(splits[19]) / numRequestsToSimulate)
	  perc99_9[policy].append(float(splits[21]) / numRequestsToSimulate)

 


policy = Policies[2]
lines = open("LogPrunedDataDynamicVriksha.txt").readlines()
i = 0
perc50[policy] = []
perc60[policy] = []
perc70[policy] = []
perc80[policy] = []
perc90[policy] = []
perc95[policy] = []
perc98[policy] = []
perc99[policy] = []
perc99_9[policy] = []
for d in Deadlines:
	  splits = lines[i].split()
	  assert len(splits) == 32
	  i += 1
	  perc50[policy].append(float(splits[5]) / numRequestsToSimulate)
	  perc60[policy].append(float(splits[7]) / numRequestsToSimulate)
	  perc70[policy].append(float(splits[9]) / numRequestsToSimulate)
	  perc80[policy].append(float(splits[11]) / numRequestsToSimulate)
	  perc90[policy].append(float(splits[13]) / numRequestsToSimulate)
	  perc95[policy].append(float(splits[15]) / numRequestsToSimulate)
	  perc98[policy].append(float(splits[17]) / numRequestsToSimulate)
	  perc99[policy].append(float(splits[19]) / numRequestsToSimulate)
	  perc99_9[policy].append(float(splits[21]) / numRequestsToSimulate)

Y50 = []
Y60 = []
Y70 = []
Y80 = []
Y90 = []
Y95 = []
Y98 = []
Y99 = []
Y99_9 = []
X = []
for p in Policies:
	  Y50.append(perc50[p])
	  Y60.append(perc60[p])
	  Y70.append(perc70[p])
	  Y80.append(perc80[p])
	  Y90.append(perc90[p])
	  Y95.append(perc95[p])
	  Y98.append(perc98[p])
	  Y99.append(perc99[p])
	  Y99_9.append(perc99_9[p])
	  X.append(Deadlines)

plotter.PlotN(X, Y50, 
    X='Deadline (ms)', Y='Quality', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 50.01], \
    outputFile="PercentilesPruned50perc")
plotter.PlotN(X, Y60, 
    X='Deadline (ms)', Y='Quality', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 50.01], \
    outputFile="PercentilesPruned60perc")
plotter.PlotN(X, Y70, 
    X='Deadline (ms)', Y='Quality', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 50.01], \
    outputFile="PercentilesPruned70perc")
plotter.PlotN(X, Y80, 
    X='Deadline (ms)', Y='Quality', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 50.01], \
    outputFile="PercentilesPruned80perc")
plotter.PlotN(X, Y90, 
    X='Deadline (ms)', Y='Quality', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 50.01], \
    outputFile="PercentilesPruned90perc")
plotter.PlotN(X, Y95, 
    X='Deadline (ms)', Y='Quality', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 50.01], \
    outputFile="PercentilesPruned95perc")
plotter.PlotN(X, Y98, 
    X='Deadline (ms)', Y='Quality', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 50.01], \
    outputFile="PercentilesPruned98perc")
plotter.PlotN(X, Y99, 
    X='Deadline (ms)', Y='Quality', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 50.01], \
    outputFile="PercentilesPruned99perc")
plotter.PlotN(X, Y99_9, 
    X='Deadline (ms)', Y='Quality', \
    labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 50.01], \
    outputFile="PercentilesPruned99_9perc")










