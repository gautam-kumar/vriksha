#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

lines = open("Map-Skew.txt").readlines()
map_durations = []
X = []
i = 0
for l in lines:
	i += 1.0
	map_durations.append(max(0.0, float(l.split()[3])))
	X.append(i / len(lines))

map_durations = sorted(map_durations)
N = len(map_durations)
print "Mean:", sum(map_durations) / N
print "50:", map_durations[N / 2 - 1]
print "75:", map_durations[3 * N / 4 - 1]
print "90:", map_durations[9 * N / 10 - 1]
print "95:", map_durations[95 * N / 100 - 1]
print "99:", map_durations[99 * N / 100 - 1]
print "99.9", map_durations[999 * N / 1000 - 1]

print len(map_durations)
plotter.PlotNLine([map_durations], [X], \
  X='Time (s)', Y='CDF', \
  xAxis=[0, 1000], yAxis=[0, 1.0], \
  outputFile="MapDurations")
