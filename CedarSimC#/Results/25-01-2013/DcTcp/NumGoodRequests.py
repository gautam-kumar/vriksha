#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter


T = ['10', '20', '30', '50']
for t in T:
  lines = open("LogDcTcpT" + t + ".txt").readlines()
  D = ['3']
  i = 0
  for d in D:
    num80 = []
    num90 = []
    num95 = []
    num99 = []
    waitTimes = []
    for j in range(9):
	    splits = lines[i].split()
	    i += 1
	    waitTimes.append(float(splits[1]) * 1000.0)
	    numRequestsCompleted = float(splits[17])
	    num80.append(float(splits[9]) / numRequestsCompleted)
	    num90.append(float(splits[11]) / numRequestsCompleted)
	    num95.append(float(splits[13]) / numRequestsCompleted)
	    num99.append(float(splits[15]) / numRequestsCompleted)
    plotter.PlotN([waitTimes, waitTimes, waitTimes, waitTimes], [num80, num90, num95, num99], X='Wait Time (ms)', Y='Fraction of Requests', \
      labels=['> 0.80', '> 0.90', '> 0.95', '> 0.99'], legendSize=12, legendTitle='Quality Threshold',\
      yAxis=[0, 1.01], \
      outputFile="NumGoodRequestsD" + d + "msT" + t)
