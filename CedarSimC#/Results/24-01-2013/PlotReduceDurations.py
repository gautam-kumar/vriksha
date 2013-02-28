#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

lines = open("reduce-durations.txt").readlines()
reduce_durations = []
X = []
i = 0
for l in lines:
	i += 1.0
	reduce_durations.append(max(0.0, float(l)))
	X.append(i / len(lines))
print max(reduce_durations)

reduce_durations = sorted(reduce_durations)
N = len(reduce_durations)
print "Mean:", sum(reduce_durations) / N
print "50:", reduce_durations[N / 2 - 1]
print "75:", reduce_durations[3 * N / 4 - 1]
print "90:", reduce_durations[9 * N / 10 - 1]
print "95:", reduce_durations[95 * N / 100 - 1]
print "99:", reduce_durations[99 * N / 100 - 1]
print "99.9", reduce_durations[999 * N / 1000 - 1]

print len(reduce_durations)
plotter.PlotNLine([reduce_durations], [X], \
  X='Time (s)', Y='CDF', \
  xAxis=[0, 2000], yAxis=[0, 1.0], \
  outputFile="ReduceDurations")

newX = []
newD = []
for i in range(len(X)):
  if X[i] >= 0.99:
    newX.append(X[i])
    newD.append(reduce_durations[i])

plotter.PlotNLine([newD], [newX], \
  X='Time (s)', Y='CDF', \
  xAxis=[0, 8000], yAxis=[0.99, 1.0], \
  outputFile="ReduceDurations0.99Above")

