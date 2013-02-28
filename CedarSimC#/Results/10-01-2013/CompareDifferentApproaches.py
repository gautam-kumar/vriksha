#!/usr/bin/env python
import sys
import os
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import random


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


random.seed(1234)

k = 40.0
D = 75.0
increment = 1.0
mean1 = 2.0
sigma1 = 0.55
mean2 = 2.0
sigma2 = 0.55
while mean1 <= 4.0:
    t = 1.0
    utility = 0
    Times = []
    Utility = []
    while (t <= 140.0):
      gain = k * (Cdf1(t + increment) - Cdf1(t)) * Cdf2(D - t - increment) 
      loss = k * (Cdf1(t) - math.pow(Cdf1(t), k)) * (Cdf2(D - t) - Cdf2(D - t - increment))
      utility += (gain - loss) 
      t += increment
      Times.append(t)
      Utility.append(utility)
    print mean1, mean2, Times[IndexOfMax(Utility)], Utility[IndexOfMax(Utility)]
    print Times
    print Utility
    mean2 += 0.01
    mean1 += 0.01

