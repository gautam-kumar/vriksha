#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

mean = 2.94
stdev = 0.55
T = ['10', '20', '30', '50']
for t in T:
  lines = open("LogNormalDataT" + t + ".txt").readlines()
  D = ['75', '90', '100', '120', '150']
  i = 0
  for d in D:
    mean = []
    p99 = []
    p99_9 = []
    waitTimes = []
    for j in range(9):
	    splits = lines[i].split()
	    i += 1
	    waitTimes.append(float(splits[1]) * 1000.0)
	    mean.append(float(splits[3]))
	    p99.append(float(splits[5]))
	    p99_9.append(float(splits[7]))

    plotter.PlotN([waitTimes, waitTimes, waitTimes], [mean, p99, p99_9], \
      X='Wait Time (ms)', Y='Utility', \
      labels=['Mean', '99%', '99.9%'], legendSize=12, legendTitle='',\
      yAxis=[0, 40.01], \
      outputFile="PercentilesD" + d + "msT" + t)

