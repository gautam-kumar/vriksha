#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import random



mean = 2.94
sigma = 0.55
numSamples = 100000
durations = []
X = []
for i in range(numSamples):
  durations.append(random.lognormvariate(mean, sigma))
  X.append(1.0 * i / numSamples)
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
  xAxis=[0, 120], 
  yAxis=[0, 1.0], \
  outputFile="GoogleDurations")

