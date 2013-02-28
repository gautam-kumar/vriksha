#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

Prop50 = []
Our50 = []
Opt50 = []

T = [5.0, 10.0, 15.0, 20.0, 25.0, 30.0, 35.0, 40.0, 45.0, 50.0]
lines = open("PropTopology145.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Prop50.append(float(s[5]))

lines = open("OurTopology145.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Our50.append(float(s[5]))

lines = open("OptTopology145.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Opt50.append(float(s[5]))



R = [ t / 50.0 for t in T]
i1 = [ 100.0 * (y - x) / x for x, y in zip(Prop50, Our50) ]
i2 = [ 100.0 * (y - x) / x for x, y in zip(Prop50, Opt50) ]

plotter.PlotN([T, T], [i1, i2], 
    X='Fanout at both stages $k_1 = k_2$', Y='% Improvement in \n Response Quality', \
    labels=['Cedar', 'Ideal'], legendSize=18, legendLoc='lower right',\
    lWidth=4, mSize=6, \
    yAxis=[0, 100.1], #xAxis=[139.99, 170.01],\
    outputFile="PercImpTopology")


