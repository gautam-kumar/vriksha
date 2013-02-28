#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Stdev = []
Prop = []
Our = []
OurAgg = []


D = [140, 145, 150, 155, 160, 165, 170]
lines = open("OurWithAggregationPerformsBadDeadline.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    OurAgg.append(float(s[5]))

lines = open("OurDeadline.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Our.append(float(s[5]))
Our = Our[:-2]

lines = open("PropDeadline.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Prop.append(float(s[5]))
Prop = Prop[:-2]

i1 = [ 100.0 * (y - x) / x for x, y in zip(Prop, Our) ]
i2 = [ 100.0 * (y - x) / x for x, y in zip(Prop, OurAgg) ]

plotter.PlotN([D, D], [i1, i2], 
    X='Wait-time at mid-level aggregator (ms)', Y='% Improvement in \n Response Quality', \
    labels=['Cedar', 'Cedar (with estimate pooling)'], legendSize=16, legendLoc='upper right', \
    lWidth=3, mSize=5, \
    yAxis=[0, 100.1], #xAxis=[, 2.401],\
    outputFile="WithWithoutAggregation")

