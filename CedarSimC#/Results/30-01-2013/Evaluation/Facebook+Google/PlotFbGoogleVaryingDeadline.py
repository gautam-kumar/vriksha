#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

D = []
Prop50 = []
Our50 = []
Prop75 = []
Our75 = []
Prop90 = []
Our90 = []


D = [140.0, 145.0, 150.0, 155.0, 160.0, 165.0, 170.0]
lines = open("PropDeadline.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Prop50.append(float(s[5]))
    Prop75.append(float(s[9]))
    Prop90.append(float(s[13]))

lines = open("OurDeadline.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Our50.append(float(s[5]))
    Our75.append(float(s[9]))
    Our90.append(float(s[13]))

print Our50


i1 = [ 100.0 * (y - x) / x for x, y in zip(Prop50, Our50) ]
i2 = [ 100.0 * (y - x) / x for x, y in zip(Prop75, Our75) ]
i3 = [ 100.0 * (y - x) / x for x, y in zip(Prop90, Our90) ]

plotter.PlotN([D, D], [i1[:-2]], 
    X='Deadline at high-level aggregator (ms)', Y='% Improvement in \n Response Quality', \
    labels=['Average', '75 Percentile'], legendSize=16, legendLoc='upper right', \
    lWidth=3, mSize=5, \
    yAxis=[0, 100.1], xAxis=[139.99, 170.01],\
    outputFile="PercImpDeadlineEval")

