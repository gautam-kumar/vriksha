#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import random

k = 40
X = []
Diff = []
for j in range(100000):
  responses = []
  for i in range(40):
    responses.append(random.expovariate(0.1))
  responses = sorted(responses)
  sum1 = 0
  sum2 = 0
  prev = 0
  for i in range(20):
    a = math.log(1.0 * k / (k - i)) / responses[i]
    sum1 += a
  estLambda = sum1 / 20.0
  X.append(estLambda)
  Diff.append(abs(estLambda - 0.1))
Diff = sorted(Diff)
mean = sum(X) / len(X)
print "Mean:", mean
print "Stdev:", math.sqrt(sum([(x - mean) * (x - mean) for x in X]) / len(X))
print "90%: ", Diff[90 * len(Diff) / 100 - 1]
print "99%: ", Diff[99 * len(Diff) / 100 - 1]
