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

D = [5, 10, 15, 20, 25, 30, 35, 40, 45, 50]
lines = open("LogFbGoogleHalfTopology155.txt").readlines()
for l in lines:
  if "Improvement" in l:
    s = l.split()
    ImpHalf.append(float(s[2]))

lines = open("LogFbGooglePropTopology155.txt").readlines()
for l in lines:
  if "Improvement" in l:
    s = l.split()
    ImpProp.append(float(s[2]))

lines = open("LogFbGoogleOurTopology155.txt").readlines()
for l in lines:
  if "Improvement" in l:
    s = l.split()
    ImpOur.append(float(s[2]))

i1 = [ 100.0 * (y - x) / x for x, y in zip(ImpHalf, ImpOur)]
i2 = [ 100.0 * (y - x) / x for x, y in zip(ImpProp, ImpOur)]

print i1
print i2

plotter.PlotN([D], [i1], 
    X='Fanout', Y='% Improvement in \n Response Quality', \
    labels=['Baseline: D/2-Split'], legendSize=16, legendLoc='lower right', \
    lWidth=4, mSize=6, \
    yAxis=[0, 400.1], #xAxis=[149.99, 190.0],\
    outputFile="PercImpD-2FbGoogleVaryingTopology155")

plotter.PlotN([D], [i2], 
    X='Fanout', 
    Y=None, \
    labels=['Baseline: Prop-Split'], legendSize=15, \
    lWidth=4, mSize=6, \
    yAxis=[0, 100.1], #xAxis=[149.99, 190.0],\
    outputFile="PercImpFbPropGoogleVaryingTopology155")










