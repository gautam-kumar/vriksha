#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

T = ['10', '20', '30', '40', '50']
for t in T:
  lines = open("LogNormalDataT" + t + ".txt").readlines()
  D = ['75ms', '90ms', '100ms', '120ms', '150ms']
  i = 0
  num80 = {}
  num90 = {}
  num95 = {}
  num99 = {}
  waitTimes = {}
  for d in D:
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
	    num80[d].append(float(splits[9]) / numRequestsCompleted)
	    num90[d].append(float(splits[11]) / numRequestsCompleted)
	    num95[d].append(float(splits[13]) / numRequestsCompleted)
	    num99[d].append(float(splits[15]) / numRequestsCompleted)
    
  Y80 = []
  Y90 = []
  X = []
  for d in D:
	  Y80.append(num80[d])
	  Y90.append(num90[d])
	  X.append(waitTimes[d])
  plotter.PlotN(X, Y80, 
    X='Wait Time (ms)', Y='Fraction of Requests', \
    labels=D, legendSize=12, legendTitle='Deadline',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsT" + t + "-80perc")


  plotter.PlotN(X, Y90, 
    X='Wait Time (ms)', Y='Fraction of Requests', \
    labels=D, legendSize=12, legendTitle='Deadline',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsT" + t + "-90perc")
