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

def Cdf(t):
  if t < 0.1:
  	return 0
  global mean, stdev
  return stats.norm.cdf((math.log(t) - mean) / stdev)

def Cdf2(t):
  global ratio
  return Cdf(t / ratio)

def Eval(t):
  return k * (Cdf(t) - math.pow(Cdf(t), k)) * (Cdf(D - t))

k = 40.0
D = 20.0
increment = 1.0

mean1 = 21.96
ratio = 0.2

OptimalTimeouts = []
Ratios = []
Deadlines = []
while (ratio <= 5.0):
  #D = ((1 + ratio) / 2.0) * 70.0
  D = 70.0
  t = 1.0
  utility = 0
  Times = []
  Utility = []
  #print D
  while (t < D):
    #print t
    u1 = (Cdf(t + increment) - Cdf(t)) * Cdf2(D - t - increment) 
    loss = (Cdf(t) - math.pow(Cdf(t), k)) * (Cdf2(D - t) - Cdf2(D - t - increment))
    utility = utility + (u1 - loss) 
    Times.append(t)
    Utility.append(utility)
    t += increment
  
  OptimalTimeout = Times[IndexOfMax(Utility)]
  n = (1/(1 + ratio)) * D
  print D, OptimalTimeout, Utility[IndexOfMax(Utility)], n, Utility[int(n) - 1] 
  Ratios.append(ratio)
  OptimalTimeouts.append(OptimalTimeout)
  Deadlines.append(D)
  ratio += 0.1

NaiveT = [ (1/(1 + ratio)) * ((1 + ratio) / 2.0) * 90 for ratio in Ratios]

plotter.PlotN([Ratios, Ratios], [NaiveT, OptimalTimeouts], X='Ratio', Y='OptimalTimeout', \
  labels=['Naive', 'Optimal'], legendSize=12, legendTitle='Policy',\
  yAxis=[0, 101], \
  outputFile="OptimalTimeoutVsDiffStageTimes")

