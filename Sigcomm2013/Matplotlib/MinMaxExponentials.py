#!/usr/bin/env python
import plotter
import os
import sys
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")

k = 40
l1 = 1.0 / 80.0
l2 = 1.0 / 100.0
T = [x for x in range(1001)]

def ExpCdf(l, t):
  return 1 - math.exp(-l * t);

def MinExpCdf(l, t):
  return 1 - math.pow(1 - ExpCdf(l, t), k);

def MaxExpCdf(l, t):
  return math.pow(ExpCdf(l, t), k);

YExp = [ExpCdf(l2, t) for t in T]
YMinExp1 = [MinExpCdf(l1, t) for t in T]
YMinExp2 = [MinExpCdf(l2, t) for t in T]
YMaxExp1 = [MaxExpCdf(l1, t) for t in T]
YMaxExp2 = [MaxExpCdf(l2, t) for t in T]
plotter.PlotN([T, T, T, T], [YMinExp1, YMinExp2, YMaxExp1, YMaxExp2], Y="CDF", X="Time", \
  labels=['Min 80ms', 'Min 100ms', 'Max 80ms', 'Max 100ms'], outputFile="MinMaxExponentials", \
  xAxis=[0, 1000], yAxis=[0, 1.0], ext="pdf")


for i in range(len(T)):
  if YMinExp1[i] > 0.99:
  	print "MinExp1", i
  	break;
for i in range(len(T)):
  if YMinExp2[i] > 0.99:
  	print "MinExp2", i
  	break;
for i in range(len(T)):
  if YMaxExp1[i] > 0.99:
  	print "MaxExp1", i
  	break;
for i in range(len(T)):
  if YMaxExp2[i] > 0.99:
  	print "MaxExp2", i
  	break;





