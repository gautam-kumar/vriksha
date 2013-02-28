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

mean1 = 4.4 
stdev1 = 2.0 
def Cdf1(t):
  if t < 0.1:
  	return 0
  return stats.norm.cdf((math.log(t) - mean1) / stdev1)

mean2 = 2.94
stdev2 = 0.55 
def Cdf2(t):
  if t < 0.1:
  	return 0
  return stats.norm.cdf((math.log(t) - mean2) / stdev2)




PlotX = []
PlotY = []

S = [0.01 * x for x in range(90, 140)]
for s in S:
  print s,
  global stdev1
  stdev1 = s
  k = 50.0
  Deadlines = [200]
  increment = 1.0
  for D in Deadlines:
    t = 0.0
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
    a = math.exp(mean1 + stdev1 * stdev1 / 2)
    #a = math.exp(mean1)
    #print a
    #b = math.exp(mean2)
    b = math.exp(mean2 + stdev2 * stdev2 / 2)
    #print b
    prop = int(D * (a / (a + b)))
    print D, Times[IndexOfMax(Utility)], max(Utility), prop, Utility[prop],
    opt = max(Utility)
    propU = Utility[prop]
    print 100.0 * (opt - propU) / propU

#plotter.PlotN(PlotX, PlotY, X='Wait Time', Y='Utility Mean', \
#  labels=Fanout, legendSize=12, legendTitle='Fanout',\
#  yAxis=[0, 1.01], \
#  outputFile="DeltaUAnalysisForFanout")

