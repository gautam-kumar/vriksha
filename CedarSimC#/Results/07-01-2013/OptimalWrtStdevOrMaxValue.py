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
  utilityMean = float(splits[3]);
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
	Stdev.append(str(w) + " ms")
	WaitTime.append(xyz[0])
	UtilityMean.append(xyz[1])
	Utility99.append(xyz[2])

print UtilityMean

plotter.PlotN(WaitTime, UtilityMean, X='Wait Time', Y='Utility Mean', \
    labels=Stdev, legendSize=12, legendTitle='Stdev',\
    outputFile="OptimalWrtStdevMeanNormal")

plotter.PlotN(WaitTime, Utility99, X='Wait Time', Y='Utility 99%', \
    labels=Stdev, legendSize=12, legendTitle='Stdev',\
    outputFile="OptimalWrtStdev99Normal")

lines = open("OptimalWrtMaxValueExp.txt").readlines()
Dict = {}
print len(lines)
i = 0
while i < len(lines):
  if '#' in lines[i]:
  	i += 1
  	print lines[i]
  	continue	
  splits = lines[i].split()
  maxValue = 1000 * float(splits[0]);
  wait = 1000 * float(splits[1]);
  utilityMean = float(splits[3]);
  utility99 = float(splits[5])

  if maxValue not in Dict:
  	Dict[maxValue] = [[], [], []]

  Dict[maxValue][0].append(wait)
  Dict[maxValue][1].append(utilityMean)
  Dict[maxValue][2].append(utility99)
  i += 1


MaxValue = []
WaitTime = []
UtilityMean = []
Utility99 = []
for w in sorted(Dict.iterkeys()):
	xyz = Dict[w]
	MaxValue.append(str(w) + " ms")
	WaitTime.append(xyz[0])
	UtilityMean.append(xyz[1])
	Utility99.append(xyz[2])


plotter.PlotN(WaitTime, UtilityMean, X='Wait Time', Y='Utility Mean', \
    labels=MaxValue, legendSize=12, legendTitle='MaxValue',\
    outputFile="OptimalWrtMaxValueMeanExp")

plotter.PlotN(WaitTime, Utility99, X='Wait Time', Y='Utility 99%', \
    labels=MaxValue, legendSize=12, legendTitle='MaxValue',\
    outputFile="OptimalWrtMaxValue99Exp")


