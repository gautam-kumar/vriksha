#!/usr/bin/env python
import sys
import os
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")
import plotter

import random

orders = []
f = open("OrderStatsLogNormal.txt").readlines()
for l in f:
	orders.append(float(l.split()[1]))

def GetStdev(samples):
  mean = sum(samples) / len(samples)
  var = sum([(x - mean) * (x - mean) for x in samples]) / len(samples)
  return math.sqrt(var)

def NaiveEstimatonSamples(samples):
  logSamples = [math.log(x) for x in samples]
  estimatedMeans = [logSamples[0]]
  estimatedSigma = [0]
  for i in range(1, len(logSamples)):
    seen = logSamples[:i + 1]
    estimatedMeans.append(sum(seen) / len(seen))
    estimatedSigma.append(GetStdev(seen))
  return estimatedMeans[1:], estimatedSigma[1:]

def MyEstimationSamples(samples):
  estimatedMeans = []
  estimatedSigma = []
  estimatedMeans.append(0)
  estimatedSigma.append(0)
  for i in range(1, len(samples)):
  	rPrev = math.log(samples[i - 1])
  	oPrev = math.log(orders[i - 1])
  	r = math.log(samples[i])
  	o = math.log(orders[i])
  	newSigma = (r - rPrev) / (o - oPrev)
  	newMean = r - o * newSigma
  	oldMean = estimatedMeans[i - 1]
  	oldSigma = estimatedSigma[i - 1]
  	estimatedMeans.append((newMean + oldMean * (i - 1)) / i )
  	estimatedSigma.append((newSigma + oldSigma * (i - 1)) / i )
  return estimatedMeans[1:], estimatedSigma[1:]

mean = 4.94
sigma = 1.15
K = 50
numRuns = 100

nSToEstMyMean = {}
nSToEstMySigma = {}
nSToEstNaiveMean = {}
nSToEstNaiveSigma = {}

for i in range(1, K):
  nSToEstMyMean[i] = [] 
  nSToEstMySigma[i] = []
  nSToEstNaiveMean[i] = []
  nSToEstNaiveSigma[i] = []




for i in range(numRuns):
  samples = []
  for i in range(K):
	  samples.append(random.lognormvariate(mean, sigma))
  samples = sorted(samples)

  estimatedMeans, estimatedSigma = MyEstimationSamples(samples)
  for i in range(1, K):
  	nSToEstMyMean[i].append(estimatedMeans[i - 1])
  	nSToEstMySigma[i].append(estimatedSigma[i - 1])
  naiveMeans, naiveSigma = NaiveEstimatonSamples(samples)
  for i in range(1, K):
  	nSToEstNaiveMean[i].append(naiveMeans[i - 1])
  	nSToEstNaiveSigma[i].append(naiveSigma[i - 1])


MyPercErrorInMeanAvg = {}
MyPercErrorInMeanStdev = {}
MyPercErrorInSigmaAvg = {}
MyPercErrorInSigmaStdev = {}

NaivePercErrorInMeanAvg = {}
NaivePercErrorInMeanStdev = {}
NaivePercErrorInSigmaAvg = {}
NaivePercErrorInSigmaStdev = {}


for i in range(1, K):
  MyPercErrorMean = [100.0 * abs(a - mean) / mean for a in nSToEstMyMean[i]]
  MyPercErrorSigma = [100.0 * abs(a - sigma) / sigma for a in nSToEstMySigma[i]]
  MyPercErrorInMeanAvg[i] = sum(MyPercErrorMean) / len(MyPercErrorMean)
  MyPercErrorInMeanStdev[i] = GetStdev(MyPercErrorMean)
  MyPercErrorInSigmaAvg[i] = sum(MyPercErrorSigma) / len(MyPercErrorSigma)
  MyPercErrorInSigmaStdev[i] = GetStdev(MyPercErrorSigma)
  
  
  NaivePercErrorMean = [100.0 * abs(a - mean) / mean for a in nSToEstNaiveMean[i]]
  NaivePercErrorSigma = [100.0 * abs(a - sigma) / sigma for a in nSToEstNaiveSigma[i]]
  NaivePercErrorInMeanAvg[i] = sum(NaivePercErrorMean) / len(NaivePercErrorMean)
  NaivePercErrorInMeanStdev[i] = GetStdev(NaivePercErrorMean)
  NaivePercErrorInSigmaAvg[i] = sum(NaivePercErrorSigma) / len(NaivePercErrorSigma)
  NaivePercErrorInSigmaStdev[i] = GetStdev(NaivePercErrorSigma)

  #print i + 1, kToEstMeanMean[i], kToEstSigmaMean[i], kToEstMeanStdev[i], kToEstSigmaStdev[i]
  print i + 1, MyPercErrorInMeanAvg[i], MyPercErrorInMeanStdev[i], MyPercErrorInSigmaAvg[i], MyPercErrorInSigmaStdev[i]
  print i + 1, NaivePercErrorInMeanAvg[i], NaivePercErrorInMeanStdev[i], NaivePercErrorInSigmaAvg[i], NaivePercErrorInSigmaStdev[i]

#Plotting
YMyMean = [MyPercErrorInMeanAvg[i] for i in range(1, K)]
YMyMeanErr = [MyPercErrorInMeanStdev[i] for i in range(1, K)]
YNaiveMean = [NaivePercErrorInMeanAvg[i] for i in range(1, K)]
YNaiveMeanErr = [NaivePercErrorInMeanStdev[i] for i in range(1, K)]
X = [i + 1 for i in range(1, K)]
plotter.PlotN([X, X], [YMyMean, YNaiveMean], X='#Completed processes', Y='% Error in \n Estimate', \
  labels=['Cedar', 'Empirical'], legendSize=18, legendLoc='upper right', \
  mSize=4, lWidth=3,
  yAxis=[0, 100.01], xAxis=[2, 50.01], \
  outputFile="ErrorInMeanEstimateLogNormal")



YMySigma = [MyPercErrorInSigmaAvg[i] for i in range(1, K)]
YMySigmaErr = [MyPercErrorInSigmaStdev[i] for i in range(1, K)]
YNaiveSigma = [NaivePercErrorInSigmaAvg[i] for i in range(1, K)]
YNaiveSigmaErr = [NaivePercErrorInSigmaStdev[i] for i in range(1, K)]
X = [i + 1 for i in range(1, K)]
plotter.PlotN([X, X], [YMySigma, YNaiveSigma], X='#Completed processes', Y='% Error in \n Estimate', \
  labels=['Cedar', 'Empirical'], legendSize=18, legendLoc='upper right', \
  mSize=4, lWidth=3,
  yAxis=[0, 100.01], xAxis=[2, 50.01], \
  outputFile="ErrorInSigmaEstimateLogNormal")

