#!/usr/bin/env python
import sys
import os
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import random

def GetClosestFromList(mean, L):
  closest = L[0]
  diff = abs(mean - L[0])
  for l in L:
    if (abs(mean - l)) < diff:
      closest = l
      diff = abs(mean - l)
  return closest


def GetIndex(x, X):
  i = 0
  while i < len(X):
    if X[i] == x:
      return i
  return -1

def GetUtility(m1, m2, timeout):
  global UtilityDict, T
  return UtilityDict[(m1, m2)].utilities[timeout]

def GetOptimalUtility(m1, m2):
  #print UtilityDict[(m1, m2)].bestUtility
  return UtilityDict[(m1, m2)].bestUtility

class UtilityStructure:
  def __init__(self):
    self.bestTimeout = 0
    self.bestUtility = 0
    self.utilities = {} 
  
def ReadUtilityDictFromFile(file):
  global T
  lines = open(file).readlines()
  d = {}
  i = 0
  while i < len(lines):
    #print i
    l = lines[i]
    splits = l.split()
    m1 = float(splits[0])
    m2 = float(splits[1])
    utilityValues = UtilityStructure()
    utilityValues.bestTimeout = float(splits[2])
    utilityValues.bestUtility = float(splits[3])
    u = {} 
    i += 2
    l = (lines[i])[1:-1]
    j = 0
    for s in l.split():
      u[T[j]] = float(s[:-1])
      j += 1
    utilityValues.utilities = u
    i += 1
    d[(m1, m2)] = utilityValues
  return d

T = [(1.0 + x) for x in range(140)]
UtilityDict = ReadUtilityDictFromFile("UtilityAsFunctionOfTimeoutD75.txt")


ListOfMean1 = []
ListOfMean2 = [] 
for tu in UtilityDict.keys():
  ListOfMean1.append(tu[0])
  ListOfMean2.append(tu[1])

mean1 = 2.9
mean2 = 3.4
stdev = 1.5
NumRequests = 1000

Totals = []
Optimals = []

M1 = []
M2 = []
OptimalUtility = 0
for i in range(NumRequests):
  m1 = max(2.5, min(random.normalvariate(mean1, stdev), 4.0))
  m1 = GetClosestFromList(m1, ListOfMean1)  
  m2 = m1
  M1.append(m1)
  M2.append(m2)
  OptimalUtility += GetOptimalUtility(m1, m2)

i = 0
for t in T:
  TotalUtility = 0
  for i in range(NumRequests):
    m1 = M1[i]; m2 = M2[i]
    TotalUtility += GetUtility(m1, m2, t)
  Totals.append(TotalUtility / NumRequests)
  Optimals.append(OptimalUtility / NumRequests)

i = 0
for t in T:
	print t, Totals[i], Optimals[i]
	i += 1
