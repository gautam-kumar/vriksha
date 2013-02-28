#!/usr/bin/env python
import sys
import os
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

mean = 100.0
stdev = 10.0

def IndexOfMax(A):
  max = A[0]
  ind = 0
  for i in range(len(A)):
    if A[i] > max:
    	max = A[i]
    	ind = i
  return ind

def Cdf(t):
  global mean, stdev
  return stats.norm.cdf((t - mean) / stdev)

Stdev = []
PlotX = []
PlotY = []

def Eval(t):
  return k * (Cdf(t) - math.pow(Cdf(t), k)) * (Cdf(D - t))

while (stdev <= 90.0):
  t = 0.0
  utility = 0
  increment = 1.0
  k = 40.0
  D = 280.0
  Times = []
  Utility = []
  while (t <= 270.0):
    u1 = k * (Cdf(t + increment) - Cdf(t)) * Cdf(D - t - increment) 
    loss = k * (Cdf(t) - math.pow(Cdf(t), k)) * (Cdf(D - t) - Cdf(D - t - increment))
    #l1 = k * Cdf(D - t) * ((Cdf(t + increment) - Cdf(t)) - (math.pow(Cdf(t + increment), k) - math.pow(Cdf(t), k)))
    #loss -= l1
    #u2 = (1 - math.pow(Cdf(t), k)) * (Eval(t + increment) - Eval(t))
#* (1 - math.pow(Cdf(t), k)) 
    #`u2 = 0
    utility = utility + (u1 - loss) 
    t += increment
    Times.append(t)
    Utility.append(utility)
  s = str(stdev) + " ms"
  Stdev.append(s)
  print s, IndexOfMax(Utility)
  stdev += 10.0
  PlotX.append(Times)
  PlotY.append(Utility)

plotter.PlotN(PlotX, PlotY, X='Wait Time', Y='Utility Mean', \
  labels=Stdev, legendSize=12, legendTitle='Stdev',\
  yAxis=[0, 41], \
  outputFile="CheckDeltaUAnalysis")

