#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")

if len(sys.argv) >= 2:
  input_file_path_name = sys.argv[1]
else:
  print "Usage: " + sys.argv[0] + " <input_file>"
  sys.exit(1)

lines = open(input_file_path_name).readlines()
durations = []
X = []
i = 0
for l in lines:
	i += 1.0
	durations.append(float(l.split()[0]))
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
print "99.9",durations[999 * N / 1000 - 1]

