#!/usr/bin/env python
import sys
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import os
import math
import scipy.stats as stats

lines = open("StaticWaitTimes.txt").readlines()
X = []
Y = []

print len(lines)
i = 0
while i < len(lines):
  if '#' in lines[i]:
  	i += 1
  	print lines[i]
  	continue	
  if 'Completed' in lines[i]:	
    splits = lines[i].split()
    X.append(1000 * float(splits[0]))
    Y.append(float(splits[7]))
  i += 1

plotter.PlotN([X], [Y], X='Static wait time (ms)', \
    Y='Utility',
    xAxis=[0, 160], yAxis=[0, 40],
    outputFile="StaticWaitTimes")

