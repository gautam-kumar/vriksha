#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")

lines = open("rtt_measurement.txt").readlines()
durations = []
X = []
i = 0
for l in lines:
  i += 1.0
  if float(l.split()[1]) < 0.01:
	  durations.append(1000000.0 * float(l.split()[1]))
	#X.append(i / len(lines))
print max(durations)
durations = sorted(durations)
N = len(durations)
print "Mean:", sum(durations) / N
print "50:", durations[N / 2 - 1]
print "75:", durations[3 * N / 4 - 1]
print "90:", durations[9 * N / 10 - 1]
print "95:", durations[95 * N / 100 - 1]
print "99:", durations[99 * N / 100 - 1]
print "99.9",durations[999 * N / 1000 - 1]
