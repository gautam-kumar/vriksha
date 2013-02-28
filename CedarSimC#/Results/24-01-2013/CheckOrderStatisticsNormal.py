#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import random
import scipy.stats as stats

k = 40
X = []
DiffMean = []; DiffSigma = []
Mean = []; Sigma = []
a = 0.375
for j in range(40):
  responses = []
  for i in range(40):
    responses.append(random.normalvariate(150, 50))
  responses = sorted(responses)
  sum1 = 0
  prev = 0
  ePrev = stats.norm.ppf((1 - a) / (k - 2 * a + 1))
  rPrev = responses[0]
  estMean = 0; sumMean = 0
  estSigma = 0; sumSigma = 0
  for i in range(1, 20):
  	e = stats.norm.ppf((i + 1 - a) / (k - 2 * a + 1))
  	r = responses[i]
  	sigma = (r - rPrev) / (e - ePrev)
  	mean = r - e * sigma
  	ePrev = e
  	rPrev = r
  	sumMean += mean
  	sumSigma += sigma
  estMean = sumMean / 19.0
  estSigma = sumSigma / 19.0
  print estMean, estSigma
  Mean.append(estMean); DiffMean.append(abs(estMean - 150))
  Sigma.append(estSigma); DiffSigma.append(abs(estSigma - 30))

print "Mean:", sum(Mean) / len(Mean)
print "Sigma:", sum(Sigma) / len(Sigma)
m = sum(Mean) / len(Mean); s = sum(Sigma) / len(Sigma)
#print "90%: ", Diff[90 * len(Diff) / 100 - 1]
print "StdevMean:", math.sqrt(sum([(x - m) * (x - m) for x in Mean]) / len(Mean))
print "StdevSigma:", math.sqrt(sum([(x - s) * (x - s) for x in Sigma]) / len(Sigma))
#print "99%: ", Diff[99 * len(Diff) / 100 - 1]
