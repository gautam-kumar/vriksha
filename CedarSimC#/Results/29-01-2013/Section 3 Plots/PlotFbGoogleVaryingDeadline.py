#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

D = []
ImpHalf = []
ImpProp = []
ImpOur = []

D = [150.0, 155.0, 160.0, 165.0, 170.0, 175.0, 180.0, 185.0, 190.0, 195.0, 200.0]
lines = open("LogFbGoogleHalfDeadline.txt").readlines()
for l in lines:
  if "Improvement" in l:
    s = l.split()
    ImpHalf.append(float(s[1]))

lines = open("LogFbGooglePropDeadline.txt").readlines()
for l in lines:
  if "Improvement" in l:
    s = l.split()
    ImpProp.append(float(s[1]))

lines = open("LogFbGoogleOurDeadline.txt").readlines()
for l in lines:
  if "Improvement" in l:
    s = l.split()
    ImpOur.append(float(s[1]))

i1 = [ 100.0 * (y - x) / x for x, y in zip(ImpHalf, ImpOur)]
i2 = [ 100.0 * (y - x) / x for x, y in zip(ImpProp, ImpOur)]

plotter.PlotN([D[:-2], D[:-2]], [i1[:-2], i2[:-2]], 
    X='Deadline at high-level aggregator (ms)', Y='% Improvement in \n Response Quality', \
    labels=['D/2-Split', 'Prop-Split'], legendSize=16,\
    lWidth=4, mSize=6, \
    yAxis=[0, 100.1], xAxis=[149.99, 190.0],\
    outputFile="PercImpFbGoogleVaryingDeadline")










