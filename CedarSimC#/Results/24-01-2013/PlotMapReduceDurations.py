#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

lines = open("map-durations.txt").readlines()
map_durations = []
X = []
i = 0
for l in lines:
	i += 1.0
	map_durations.append(max(0.0, float(l)))
	X.append(i / len(lines))
print max(map_durations)

lines = open("reduce-durations.txt").readlines()
reduce_durations = []
X2 = []
i = 0
for l in lines:
	i += 1.0
	reduce_durations.append(max(0.0, float(l)))
	X2.append(i / len(lines))
print max(reduce_durations)

map_durations = sorted(map_durations)
reduce_durations = sorted(reduce_durations)
N = len(map_durations)
print "Mean:", sum(map_durations) / N
print "50:", map_durations[N / 2 - 1]
print "75:", map_durations[3 * N / 4 - 1]
print "90:", map_durations[9 * N / 10 - 1]
print "95:", map_durations[95 * N / 100 - 1]
print "99:", map_durations[99 * N / 100 - 1]
print "99.9", map_durations[999 * N / 1000 - 1]

plotter.PlotNLine([map_durations, reduce_durations], [X, X2], \
  X='Time (s)', Y='CDF', \
  labels=['Map Tasks', 'Reduce Tasks'], legendSize=16, legendLoc='lower right', \
  xAxis=[0, 2000], yAxis=[0, 1.0], \
  outputFile="FacebookMapReduceDurationsCdf")

newX = []
newD = []
for i in range(len(X)):
  if X[i] >= 0.99:
  	newX.append(X[i])
  	newD.append(map_durations[i])

plotter.PlotNLine([newD], [newX], \
  X='Time (s)', Y='CDF', \
  xAxis=[1000, 90000], yAxis=[0.99, 1.0], \
  outputFile="MapDurations0.99Above")

