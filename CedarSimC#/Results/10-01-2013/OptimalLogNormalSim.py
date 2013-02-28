#!/usr/bin/env python
import sys
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import os
import math
import scipy.stats as stats

lines = open("OptimalLogNormal.txt").readlines()
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
  z = float(splits[3]);

  Xn.append(x)
  Yn.append(y)
  Zn.append(z)
  if x not in Dict:
  	Dict[x] = [[], []]

  Dict[x][0].append(y)
  Dict[x][1].append(z)
  i += 1

X = []
Y = []
Z = []
for x, yz in sorted(Dict.iteritems()):
	X.append(str(x) + " ms")
	Y.append(yz[0])
	Z.append(yz[1])

plotter.PlotN(Y, Z, Y='Mean Utility', X='Wait Time', \
    labels=X, legendSize=12, legendTitle='Deadline', \
    outputFile="OptimalLogNormalSim")


