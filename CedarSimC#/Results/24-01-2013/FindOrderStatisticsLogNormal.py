#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import random
import scipy.stats as stats


k = 50 
N = 100000
Orders = []
for i in range(k):
	Orders.append(0)
for j in range(N):
  responses = []
  for i in range(k):
  	responses.append(random.lognormvariate(0, 1))
  responses = sorted(responses)
  for i in range(k):
  	Orders[i] += responses[i]
for i in range(k):
 Orders[i] /= N
 print i, ": ", Orders[i]
