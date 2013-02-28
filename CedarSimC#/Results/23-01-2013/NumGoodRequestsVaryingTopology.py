#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

num80 = {}
num90 = {}
num95 = {}
num99 = {}
waitTimes = {}
 
T = ['10', '20', '30', '40', '50']
for t in T:
  lines = open("LogNormalDataT" + t + ".txt").readlines()
  D = ['75ms', '90ms', '100ms', '120ms', '150ms']
  i = 0
  for d in D:
    num80[(t, d)] = []
    num90[(t, d)] = []
    num95[(t, d)] = []
    num99[(t, d)] = []
    waitTimes[(t, d)] = []
    for j in range(9):
	    splits = lines[i].split()
	    i += 1
	    waitTimes[(t, d)].append(float(splits[1]) * 1000.0)
	    numRequestsCompleted = float(splits[17])
	    num80[(t, d)].append(float(splits[9]) / numRequestsCompleted)
	    num90[(t, d)].append(float(splits[11]) / numRequestsCompleted)
	    num95[(t, d)].append(float(splits[13]) / numRequestsCompleted)
	    num99[(t, d)].append(float(splits[15]) / numRequestsCompleted)
    
Y80 = []
Y90 = []
X = []
for t in T:
	Y80.append(num80[(t, '100ms')])
	Y90.append(num90[(t, '100ms')])
	X.append(waitTimes[(t, '100ms')])
plotter.PlotN(X, Y80, 
    X='Wait Time (ms)', Y='Fraction of Requests', \
    labels=T, legendSize=12, legendTitle='Fanout',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsD100ms-80perc")

plotter.PlotN(X, Y90, 
    X='Wait Time (ms)', Y='Fraction of Requests', \
    labels=T, legendSize=12, legendTitle='Fanout',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsD100ms-90perc")
