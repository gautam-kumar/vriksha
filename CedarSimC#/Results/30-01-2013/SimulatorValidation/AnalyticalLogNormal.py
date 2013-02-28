#!/usr/bin/env python
import sys
import os
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

mean = 3.0

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
  return stats.norm.cdf((math.log(t) - mean) / stdev)

Stdev = []
PlotX = []
PlotY = []

def Eval(t):
  return k * (Cdf(t) - math.pow(Cdf(t), k)) * (Cdf(D - t))

S = [0.75, 1.0, 1.25]
for stdev in S:
  t = 0.01
  utility = 0
  increment = 1.0 
  k = 50.0
  D = 100.0
  Times = []
  Utility = []
  while (t <= 90.0):
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
    Utility.append(utility / k)

  s = str(stdev) + " ms"
  Stdev.append(s)
  print s, IndexOfMax(Utility)
  stdev += 10.0
  PlotX.append(Times[10:])
  PlotY.append(Utility[10:])

X2 = []
Y2 = []
St = ['0.75', '1', '1.25']
f = open("SimLogNormal.txt").readlines()
j = 0
for s in St:
  A = []; Tim = []
  for i in range(9):
    A.append(float(f[j].split()[2]))
    Tim.append(1000.0 * float(f[j].split()[1]))
    j += 1
  X2.append(Tim)
  Y2.append(A)
Stdev = ['A: 0.75', 'A: 1.0', 'A: 1.25', 'S: 0.75', 'S: 1.0', 'S: 1.25']

plotter.PlotLineNLine([PlotX, X2], [PlotY, Y2], X='Wait-time at aggregator (ms)', Y='Expected \n Response Quality', \
  labels=Stdev, legendSize=14, legendTitle='Stdev', legendLoc='lower center',\
  yAxis=[0, 1.0], xAxis=[10, 90], \
  outputFile="AnalyticalLogNormal")

