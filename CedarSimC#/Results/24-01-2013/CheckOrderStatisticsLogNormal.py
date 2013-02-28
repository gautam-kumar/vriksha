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

Orders = [float(l) for l in open('OrderStatisticsLogNormal.txt').readlines()]

actMean = 2.54 
actSigma = 0.55
Estimates = []
for j in range(100):
  responses = []
  for i in range(40):
    responses.append(random.lognormvariate(actMean, actSigma))
  s = 0
  responses = sorted(responses)
  for i in range(20):
  	s += math.log(responses[i])
  estimate = s / 20
  print estimate
  Estimates.append(estimate)
  sum1 = 0
  prev = 0
  ePrev = math.log(Orders[0])
  rPrev = math.log(responses[0])
  estMean = 0; sumMean = 0
  estSigma = 0; sumSigma = 0
  for i in range(1, 20):
  	e = math.log(Orders[i])
  	r = math.log(responses[i])
  	sigma = (r - rPrev) / (e - ePrev)
  	mean = r - e * sigma
  	ePrev = e
  	rPrev = r
  	sumMean += mean
  	sumSigma += sigma
  estMean = sumMean / 19.0
  estSigma = sumSigma / 19.0
  #print estMean, estSigma
  Mean.append(estMean); DiffMean.append(abs(estMean - actMean))
  Sigma.append(estSigma); DiffSigma.append(abs(estSigma - actSigma))


print "RandomMean: ", sum(Estimates) / len(Estimates)
s = sum(Estimates) / len(Estimates)
print "RandomSigma: ", math.sqrt(sum([(x - s) * (x - s) for x in Estimates])) / len(Estimates)

print "Mean:", sum(Mean) / len(Mean)
print "Sigma:", sum(Sigma) / len(Sigma)
m = sum(Mean) / len(Mean); s = sum(Sigma) / len(Sigma)
#print "90%: ", Diff[90 * len(Diff) / 100 - 1]
print "StdevMean:", math.sqrt(sum([(x - m) * (x - m) for x in Mean]) / len(Mean))
print "StdevSigma:", math.sqrt(sum([(x - s) * (x - s) for x in Sigma]) / len(Sigma))
#print "99%: ", Diff[99 * len(Diff) / 100 - 1]
