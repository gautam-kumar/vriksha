#!/usr/bin/env python
import sys
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import os
import math
import scipy.stats as stats

lines = open("OptimalWaitTimes.txt").readlines()
Xn = []
Yn = []
Zn = []
Dict = {}
print len(lines)
i = 0
while i < len(lines):
  if '#' in lines[i]:
  	i += 1
  	print lines[i]
  	continue	
  splits = lines[i].split()
  x = 1000 * float(splits[0]);
  y = 1000 * float(splits[1]);
  z = float(splits[2]);

  Xn.append(x)
  Yn.append(y)
  Zn.append(z)
  if x not in Dict:
  	Dict[x] = [[], []]

  Dict[x][0].append(y)
  Dict[x][1].append(z)
  i += 1

plotter.Plot3D(Xn, Yn, Zn, X='Mid-Level Stage Time (ms)', \
    Y='High-Level Stage Time (ms)', Z='Optimal Wait Time (ms)',
    xAxis=[25, 75], yAxis=[55, 145], zAxis=[40, 100], outputFile="OptimalWaitTimes")

X = []
Y = []
Z = []
for x, yz in Dict.iteritems():
	X.append(x)
	Y.append(yz[0])
	Z.append(yz[1])

plotter.PlotN(Y, Z, Y='HLA Compute Time', X='Optimal Wait Time', \
    labels=['30ms', '35ms', '40ms', '45ms', '50ms', '55ms', '60ms', '65ms', '70ms'], \
    outputFile="OptimalWaitTimes2D")


