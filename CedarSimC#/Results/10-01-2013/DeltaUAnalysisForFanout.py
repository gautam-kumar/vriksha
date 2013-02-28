#!/usr/bin/env python
import sys
import os
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

mean = 2.94
stdev = 0.55

def IndexOfMax(A):
  max = A[0]
  ind = 0
  for i in range(len(A)):
    if A[i] > max:
    	max = A[i]
    	ind = i
  return ind

#def Cdf(t):
#  global mean, stdev
#  return stats.norm.cdf((t - mean) / stdev)

def Cdf(t):
  if t < 0.1:
  	return 0
  global mean, stdev
  return stats.norm.cdf((math.log(t) - mean) / stdev)

Fanout = []
PlotX = []
PlotY = []

def Eval(t):
  return k * (Cdf(t) - math.pow(Cdf(t), k)) * (Cdf(D - t))


k = 10.0
D = 90.0
increment = 1.0
while (k <= 100.0):
  t = 1.0
  utility = 0
  Times = []
  Utility = []
  while (t <= 140.0):
    u1 = (Cdf(t + increment) - Cdf(t)) * Cdf(D - t - increment) 
    loss = (Cdf(t) - math.pow(Cdf(t), k)) * (Cdf(D - t) - Cdf(D - t - increment))
    #l1 = k * Cdf(D - t) * ((Cdf(t + increment) - Cdf(t)) - (math.pow(Cdf(t + increment), k) - math.pow(Cdf(t), k)))
    #loss -= l1
    #u2 = (1 - math.pow(Cdf(t), k)) * (Eval(t + increment) - Eval(t))
#* (1 - math.pow(Cdf(t), k)) 
    #`u2 = 0
    utility = utility + (u1 - loss) 
    t += increment
    Times.append(t)
    Utility.append(utility)
  s = str(int(k))
  Fanout.append(s)
  print s, IndexOfMax(Utility)
  k += 10.0
  PlotX.append(Times)
  PlotY.append(Utility)

plotter.PlotN(PlotX, PlotY, X='Wait Time', Y='Utility Mean', \
  labels=Fanout, legendSize=12, legendTitle='Fanout',\
  yAxis=[0, 1.01], \
  outputFile="DeltaUAnalysisForFanout")

