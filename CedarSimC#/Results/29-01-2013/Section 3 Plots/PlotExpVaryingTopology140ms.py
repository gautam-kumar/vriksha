#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

T = []
PercImp = []
lines = open("LogExpVaryingTopology140ms.txt").readlines()
for l in lines:
	s = l.split()
	T.append(float(s[0]))
	PercImp.append(float(s[1]))

plotter.PlotN([T], [PercImp], 
    X='Fan-out at mid-level aggregator', Y='% Improvement in \n Response Quality', \
    #labels=Policies, legendSize=12, legendTitle='Policy',\
    lWidth=4, mSize=6, \
    yAxis=[0, 50.01], xAxis=[4.99, 50.01], \
    outputFile="PercImpExpVaryingTopology")










