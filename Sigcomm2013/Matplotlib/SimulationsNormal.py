#!/usr/bin/env python
import plotter
import os
import sys
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")

lines = open("SimulationsNormal.txt").readlines()
X = []
TimeMean = []
Time99 = []
IcMean = []
Ic99 = []
i = 0
while i < len(lines):
  print "i in loop: " + str(i)
  #0.01 IC Mean 1.145825 99% 0.75
  #0.01 Time Mean 0.210597999999986 99% 0.210999999999977
  splits = lines[i].split()
  X.append(1000 * float(splits[0]))
  IcMean.append(float(splits[3]))
  Ic99.append(float(splits[5]))
  i = i + 1
  splits = lines[i].split()
  TimeMean.append(1000 * float(splits[3]))
  Time99.append(1000 * float(splits[5]))
  i = i + 1

print TimeMean
print IcMean

  
plotter.PlotN([X, X], [TimeMean, Time99], Y="Response Time", X="Wait Time", \
  labels=['Mean', '99 Percentile'], outputFile="SimulationsNormalResponseTime", \
  xAxis=[0, 150], yAxis=[0, 250], ext="pdf", legendLoc = 'lower right')

plotter.PlotN([X, X], [IcMean, Ic99], Y="Utility", X="Wait Time", \
  labels=['Mean', '99 Percentile'], outputFile="SimulationsNormalUtility", \
  xAxis=[0, 150], yAxis=[0, 40], ext="pdf")
