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

googleMean = 2.94
googleStdev = 0.55
def GoogleCdf(t):
  if t < 0.1:
  	return 0
  return stats.norm.cdf((math.log(t) - googleMean) / googleStdev)

facebookMean = 5.21
facebookStdev = 1.47
def FacebookCdf(t):
  if t < 0.1:
  	return 0
  return stats.norm.cdf((math.log(t) - facebookMean) / facebookStdev)

PlotX = []
PlotY = []

k = 50.0
Deadlines = [150.0, 250.0, 350.0, 450.0, 550.0, 650.0, 750.0]
increment = 1.0
for D in Deadlines:
  t = 1.0
  utility = 0
  Times = []
  Utility = []
  while (t < D):
    u1 = (FacebookCdf(t + increment) - FacebookCdf(t)) * GoogleCdf(D - t - increment) 
    loss = (FacebookCdf(t) - math.pow(FacebookCdf(t), k)) * (GoogleCdf(D - t) - GoogleCdf(D - t - increment))
    utility = utility + (u1 - loss) 
    t += increment
    Times.append(t)
    Utility.append(utility)
  print D, Times[IndexOfMax(Utility)], max(Utility)
  PlotX.append(Times)
  PlotY.append(Utility)

#plotter.PlotN(PlotX, PlotY, X='Wait Time', Y='Utility Mean', \
#  labels=Fanout, legendSize=12, legendTitle='Fanout',\
#  yAxis=[0, 1.01], \
#  outputFile="DeltaUAnalysisForFanout")

