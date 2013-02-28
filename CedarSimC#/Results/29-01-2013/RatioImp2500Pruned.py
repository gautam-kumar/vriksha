#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Policies = ['Prop', 'Vriksha']
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

policy = Policies[0]
lines = open("Log2500PrunedProp.txt").readlines()
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
	  perc50[policy].append(float(splits[5]))
	  perc60[policy].append(float(splits[7]))
	  perc70[policy].append(float(splits[9]))
	  perc80[policy].append(float(splits[11]))
	  perc90[policy].append(float(splits[13]))
	  perc95[policy].append(float(splits[15]))
	  perc98[policy].append(float(splits[17]))
	  perc99[policy].append(float(splits[19]))
	  perc99_9[policy].append(float(splits[21]))

 


policy = Policies[1]
lines = open("Log2500PrunedDyn.txt").readlines()
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
	  perc50[policy].append(float(splits[5]))
	  perc60[policy].append(float(splits[7]))
	  perc70[policy].append(float(splits[9])) 
	  perc80[policy].append(float(splits[11]))
	  perc90[policy].append(float(splits[13]))
	  perc95[policy].append(float(splits[15]))
	  perc98[policy].append(float(splits[17]))
	  perc99[policy].append(float(splits[19]))
	  perc99_9[policy].append(float(splits[21]))


p1 = Policies[0]
p2 = Policies[1]
Y50 = [[y / x for x, y in zip(perc50[p1], perc50[p2])]]
Y60 = [[y / x for x, y in zip(perc60[p1], perc60[p2])]]
Y70 = [[y / x for x, y in zip(perc70[p1], perc70[p2])]]
Y80 = [[y / x for x, y in zip(perc80[p1], perc80[p2])]]
Y90 = [[y / x for x, y in zip(perc90[p1], perc90[p2])]]
Y95 = [[y / x for x, y in zip(perc95[p1], perc95[p2])]]
Y98 = [[y / x for x, y in zip(perc98[p1], perc98[p2])]]
Y99 = [[y / x for x, y in zip(perc99[p1], perc99[p2])]]
Y99_9 = [[y / x for x, y in zip(perc99_9[p1], perc99_9[p2])]]
X = [Deadlines]

plotter.PlotN(X, Y50, 
    X='Deadline (ms)', Y='% Improvment', \
    #labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 2.0], \
    outputFile="RatioImp2500Pruned50perc")
plotter.PlotN(X, Y60, 
    X='Deadline (ms)', Y='% Improvment', \
    #labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 2.01], \
    outputFile="RatioImp2500Pruned60perc")
plotter.PlotN(X, Y70, 
    X='Deadline (ms)', Y='% Improvment', \
    #labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 2.01], \
    outputFile="RatioImp2500Pruned70perc")
plotter.PlotN(X, Y80, 
    X='Deadline (ms)', Y='% Improvment', \
    #labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 2.01], \
    outputFile="RatioImp2500Pruned80perc")
plotter.PlotN(X, Y90, 
    X='Deadline (ms)', Y='% Improvment', \
    #labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 2.01], \
    outputFile="RatioImp2500Pruned90perc")
plotter.PlotN(X, Y95, 
    X='Deadline (ms)', Y='% Improvment', \
    #labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 2.01], \
    outputFile="RatioImp2500Pruned95perc")
plotter.PlotN(X, Y98, 
    X='Deadline (ms)', Y='% Improvment', \
    #labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 2.01], \
    outputFile="RatioImp2500Pruned98perc")
plotter.PlotN(X, Y99, 
    X='Deadline (ms)', Y='% Improvment', \
    #labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 2.01], \
    outputFile="RatioImp2500Pruned99perc")
plotter.PlotN(X, Y99_9, 
    X='Deadline (ms)', Y='% Improvment', \
    #labels=Policies, legendSize=12, legendTitle='Policy',\
    yAxis=[0, 2.01], \
    outputFile="RatioImp2500Pruned99_9perc")


