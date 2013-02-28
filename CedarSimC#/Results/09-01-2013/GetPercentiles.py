#!/usr/bin/env python
import sys

import random

X = []
n = 100000
for i in range(n):
#  X.append(random.lognormvariate(3.5, 0.55))
	X.append(random.lognormvariate(3.8, 0.55))
X = sorted(X)
print "Mean", sum(X) / len(X)
print "50%", X[n/2 - 1]
print "90%", X[9*n/10 - 1]
print "99%", X[99*n/100 - 1]
print "99.9%", X[999*n/1000 - 1]


