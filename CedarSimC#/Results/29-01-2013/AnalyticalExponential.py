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

mean1 = 0.1
def Cdf1(t):
  if t < 0.0001:
  	return 0
  return stats.expon.cdf(t, scale=mean1)

mean2 = 0.01
def Cdf2(t):
  if t < 0.0001:
  	return 0
  return stats.expon.cdf(t, scale=mean2)

PlotX = []
PlotY = []

print Cdf1(50.0)

k = 50.0
Deadlines = [.120, .125, .130, .135, .140, .145, .150, .155, .160]
increment = 0.001
for D in Deadlines:
  t = 0
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
  a = mean1
  b = mean2
  prop = int(1000 * D * (a / (a + b)))
  print D, Times[IndexOfMax(Utility)], max(Utility), prop, Utility[prop],
  print 100.0 * (max(Utility) - Utility[prop]) / Utility[prop]
  PlotX.append(Times)
  PlotY.append(Utility)


#plotter.PlotN(PlotX, PlotY, X='Wait Time', Y='Utility Mean', \
#  labels=Fanout, legendSize=12, legendTitle='Fanout',\
#  yAxis=[0, 1.01], \
#  outputFile="DeltaUAnalysisForFanout")

