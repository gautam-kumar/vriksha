#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Stdev = []
Prop = []
Our = []
w = []


Stdev = [0.75, 0.8, 0.85, 0.9, 0.95, 1.0, 1.05, 1.1, 1.15, 1.2, 1.25, 1.3, 1.35, 1.4, 1.45, 1.5]
lines = open("waitTimeWrtStdev.txt").readlines()
for l in lines:
    s = l.split()
    w.append(1000 * float(s[1]))
plotter.PlotN([Stdev], [w],
    X=r'$\sigma$ parameter of $X_1$', Y='Optimal wait-duration \n (ms)', \
    lWidth=3, mSize=5, \
    yAxis=[50, 150.1], xAxis=[0.75, 1.5],\
    outputFile="WaitTimeStdev")

