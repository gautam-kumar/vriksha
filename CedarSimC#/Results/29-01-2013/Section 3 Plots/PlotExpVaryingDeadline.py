#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

D = []
PercImpHalf = []
PercImpProp = []
lines = open("LogExpVaryingDeadlines.txt").readlines()
for l in lines:
  if "Improvement" in l:
    s = l.split()
    D.append(1000.0 * float(s[1]))
    PercImpHalf.append(float(s[2]))
    PercImpProp.append(float(s[3]))

plotter.PlotN([D, D], [PercImpHalf, PercImpProp], 
    X='Deadline at high-level aggregator (ms)', Y='% Improvement in \n Response Quality', \
    labels=['D/2-Split', 'Prop-Split'], legendSize=16,\
    lWidth=4, mSize=6, \
    yAxis=[0, 50.1], \
    outputFile="PercImpExpVaryingDeadline")










