#!/usr/bin/env python
import sys
import os
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")
import plotter


lines = open('SylviaResults.txt').readlines()
Time = []
Static = []
Optimal = []
for l in lines:
	s = l.split()
	Time.append(float(s[0]))
	Static.append(float(s[1]) / 40.0)
	Optimal.append(float(s[2]) / 40.0)


plotter.PlotN([Time, Time], [Static, Optimal], X='Static Timeout', Y='Mean Utility', \
  labels=['Static', 'Optimal'], legendSize=12,\
  yAxis=[0, 1.01], \
  outputFile="SylviaResults")

