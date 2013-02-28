#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Stdev = []
PercImp = []
lines = open("LogFbGoogleVaryingStdevMLA175ms.txt").readlines()
for l in lines:
  if "Improvement" in l:
	  s = l.split()
	  Stdev.append(float(s[1]))
	  PercImp.append(float(s[2]))

plotter.PlotN([Stdev], [PercImp], 
    X=r'$\sigma$ for log-normal distribution ($X_2$)', Y='% Improvement in \n Response Quality', \
    #labels=Policies, legendSize=12, legendTitle='Policy',\
    lWidth=4, mSize=6, \
    yAxis=[0, 100], \
    outputFile="PercImpFbGoogleVaryingStdevMLA")










