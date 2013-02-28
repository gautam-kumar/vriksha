#!/usr/bin/env python
import sys
import os
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")
import plotter


def IndexOfMax(A):
  max = A[0]
  ind = 0
  for i in range(len(A)):
    if A[i] > max:
    	max = A[i]
    	ind = i
  return ind

mean1 = 120.0
stdev1 = 100.0
def Cdf1(t):
  if t < 0.1:
  	return 0
  return stats.norm.cdf((t - mean1) / stdev1)

mean2 = 40.0
stdev2 = 20.0
def Cdf2(t):
  if t < 0.1:
  	return 0
  return stats.norm.cdf((t - mean2) / stdev2)


PlotX = []
PlotY = []

k = 50.0
Deadlines = [170, 175, 180, 185, 190, 195, 200, 205, 210]
increment = 1.0
for D in Deadlines:
  t = 1.0
  utility = 0
  Times = []
  Utility = []
  while (t < D):
    u1 = (Cdf1(t + increment) - Cdf1(t)) * Cdf2(D - t - increment) 
    loss = (Cdf1(t) - math.pow(Cdf1(t), k)) * (Cdf2(D - t) - Cdf2(D - t - increment))
    utility = utility + (u1 - loss) 
    t += increment
    Times.append(t)
    Utility.append(utility)
  prop = int(D * mean1 / (mean1 + mean2))
  print D, Times[IndexOfMax(Utility)], max(Utility), prop, Utility[prop]
  print 100.0 * (max(Utility) - Utility[prop]) / Utility[prop]
  PlotX.append(Times)
  PlotY.append(Utility)

#plotter.PlotN(PlotX, PlotY, X='Wait Time', Y='Utility Mean', \
#  labels=Fanout, legendSize=12, legendTitle='Fanout',\
#  yAxis=[0, 1.01], \
#  outputFile="DeltaUAnalysisForFanout")

