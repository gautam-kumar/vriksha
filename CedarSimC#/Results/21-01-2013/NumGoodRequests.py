#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

mean = 2.94
stdev = 0.55

D = ['60', '75', '90', '100', '120', '150']
for d in D:
  lines = open("LogNormalDataD" + d + "ms.txt").readlines()
  num80 = []
  num90 = []
  num95 = []
  num99 = []
  waitTimes = []
  for l in lines:
	  splits = l.split()
	  waitTimes.append(float(splits[1]) * 1000.0)
	  numRequestsCompleted = float(splits[17])
	  num80.append(float(splits[9]) / numRequestsCompleted)
	  num90.append(float(splits[11]) / numRequestsCompleted)
	  num95.append(float(splits[13]) / numRequestsCompleted)
	  num99.append(float(splits[15]) / numRequestsCompleted)
  plotter.PlotN([waitTimes, waitTimes, waitTimes, waitTimes], [num80, num90, num95, num99], X='Wait Time (ms)', Y='Fraction of Requests', \
    labels=['> 0.80', '> 0.90', '> 0.95', '> 0.99'], legendSize=12, legendTitle='Quality Threshold',\
    yAxis=[0, 1.01], \
    outputFile="NumGoodRequestsD"+d+"ms")

