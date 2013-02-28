#!/usr/bin/env python
import sys
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import os
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")

lines = open("SimulationsNormalDiffRequestsSameTimeout.txt").readlines()
X = []
Ic200 = []
Ic220 = []
Ic240 = []
i = 0
j = 0

print len(lines)
while i < len(lines):
  if '#' in lines[i]:
  	print lines[i]
  	j += 1
  	i += 1
  	continue	
  splits = lines[i].split()
  print j
  if j == 1:
  	X.append(1000 * float(splits[0]))
  	Ic200.append(float(splits[1]))
  if j == 2:
  	Ic220.append(float(splits[1]))
  if j == 3:
  	Ic240.append(float(splits[1]))
  i += 1

print Ic200

print Ic220

print Ic240

plotter.PlotN([X, X, X], [Ic200, Ic220, Ic240], Y="Utility", X="Wait Time", \
  labels=['200ms', '220ms', '240ms'], outputFile="SimulationsNormalDiffRequestsSameTimeout", \
  xAxis=[0, 150], yAxis=[0, 40], ext="pdf", legendLoc = 'lower right')

