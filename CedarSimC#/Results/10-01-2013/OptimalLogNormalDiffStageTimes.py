#!/usr/bin/env python
import sys
import os
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

mean1 = 2.94
sigma1 = 0.55
mean2 = 3.5
sigma2 = 0.55

def IndexOfMax(A):
  max = A[0]
  ind = 0
  for i in range(len(A)):
    if A[i] > max:
    	max = A[i]
    	ind = i
  return ind

def Cdf(t, mean, stdev):
  if t < 0.1:
  	return 0
  return stats.norm.cdf((math.log(t) - mean) / stdev)

def Cdf1(t):
  return Cdf(t, mean1, sigma1)

def Cdf2(t):
  return Cdf(t, mean2, sigma2)



Deadlines = []
PlotX = []
PlotY = []

k = 40.0
D = 20.0
increment = 1.0
while (D <= 200.0):
  t = 1.0
  utility = 0
  Times = []
  Utility = []
  while (t <= 140.0):
    u1 = k * (Cdf1(t + increment) - Cdf1(t)) * Cdf2(D - t - increment) 
    loss = k * (Cdf1(t) - math.pow(Cdf1(t), k)) * (Cdf2(D - t) - Cdf2(D - t - increment))
    #l1 = k * Cdf(D - t) * ((Cdf(t + increment) - Cdf(t)) - (math.pow(Cdf(t + increment), k) - math.pow(Cdf(t), k)))
    #loss -= l1
    #u2 = (1 - math.pow(Cdf(t), k)) * (Eval(t + increment) - Eval(t))
#* (1 - math.pow(Cdf(t), k)) 
    #`u2 = 0
    utility = utility + (u1 - loss) 
    t += increment
    Times.append(t)
    Utility.append(utility)
  s = str(D) + " ms"
  Deadlines.append(s)
  print s, IndexOfMax(Utility)
  D += 20.0
  PlotX.append(Times)
  PlotY.append(Utility)

plotter.PlotN(PlotX, PlotY, X='Wait Time', Y='Utility Mean', \
  labels=Deadlines, legendSize=12, legendTitle='Deadline',\
  yAxis=[0, 41], \
  outputFile="OptimalLogNormalDiffStageTimes")

