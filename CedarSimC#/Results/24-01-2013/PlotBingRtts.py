#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

lines = open("rtt_measurement.txt").readlines()
durations = []
X = []
i = 0
for l in lines:
	i += 1.0
	durations.append(1000.0 * float(l.split()[1]))
	X.append(i / len(lines))
print max(durations)

durations = sorted(durations)
N = len(durations)
print "Mean:", sum(durations) / N
print "50:", durations[N / 2 - 1]
print "75:", durations[3 * N / 4 - 1]
print "90:", durations[9 * N / 10 - 1]
print "95:", durations[95 * N / 100 - 1]
print "99:", durations[99 * N / 100 - 1]
print "99.9", durations[999 * N / 1000 - 1]

print len(durations)
plotter.PlotNLine([durations], [X], \
  X='Time (ms)', Y='CDF', \
  xAxis=[0, 2], 
  yAxis=[0, 1.0], \
  outputFile="BingCdf1")

newX = []
newD = []
for i in range(len(X)):
  if X[i] >= 0.90 and durations[i] < 20.0:
    #if durations[i] <= 20.0:
      newX.append(X[i])
      newD.append(durations[i])

plotter.PlotNLine([newD], [newX], \
  X='Time (ms)', Y='CDF', \
  xAxis=[0, 19], yAxis=[0.90, 1.0], \
  outputFile="BingCdf2")

