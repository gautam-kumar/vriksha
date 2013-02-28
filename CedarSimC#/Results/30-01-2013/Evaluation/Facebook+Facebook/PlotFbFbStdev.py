#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Stdev = []
Prop = []
Our = []
Opt = []

Stdev = [2.0, 2.05, 2.10, 2.15, 2.2, 2.25, 2.3]
lines = open("VaryingStdevProp.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Prop.append(float(s[5]))

lines = open("VaryingStdevOpt.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Opt.append(float(s[5]))

lines = open("VaryingStdevVriksha.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Our.append(float(s[5]))


i1 = [ 100.0 * (y - x) / x for x, y in zip(Prop, Our) ]
i2 = [ 100.0 * (y - x) / x for x, y in zip(Prop, Opt) ]

plotter.PlotN([Stdev[:-1], Stdev[:-1]], [i2[:-1], i1[:-1]], 
    X='$\sigma$ parameter of $X_1$', Y='% Improvement in \n Response Quality', \
    labels=['Cedar', 'Ideal'], legendSize=18, legendLoc='upper left', \
    lWidth=3, mSize=5, \
    yAxis=[0, 100.1], xAxis=[2., 2.251],\
    outputFile="FbFbStdev")

