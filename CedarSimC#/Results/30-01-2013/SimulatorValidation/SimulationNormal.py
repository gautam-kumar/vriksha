#!/usr/bin/env python
import sys
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import os
import math
import scipy.stats as stats

lines = open("OptimalWrtStdevNormal.txt").readlines()
Dict = {}
print len(lines)
i = 0
while i < len(lines):
  if '#' in lines[i]:
  	i += 1
  	print lines[i]
  	continue	
  splits = lines[i].split()
  stdev = 1000 * float(splits[0]);
  wait = 1000 * float(splits[1]);
  utilityMean = float(splits[3]) / 40.0;
  utility99 = float(splits[5])

  if stdev not in Dict:
  	Dict[stdev] = [[], [], []]

  Dict[stdev][0].append(wait)
  Dict[stdev][1].append(utilityMean)
  Dict[stdev][2].append(utility99)
  i += 1


Stdev = []
WaitTime = []
UtilityMean = []
Utility99 = []
for w in sorted(Dict.iterkeys()):
  xyz = Dict[w]
  if w >= 9.9 and w < 62:
	  Stdev.append(str(w) + " ms")
	  WaitTime.append(xyz[0])
	  UtilityMean.append(xyz[1])
	  Utility99.append(xyz[2])

print UtilityMean

plotter.PlotN(WaitTime, UtilityMean, X='Wait time at  aggregator (ms)', Y='Average \nResponse Quality', \
    mSize=3, lWidth=2, \
    yAxis=[0, 1.001], \
    labels=Stdev, legendSize=14, legendTitle='Stdev',\
    outputFile="OptimalWrtStdevMeanNormal")

