#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import figure
import numpy as np
ds = '0.145'
PropQualities = [] 
lines = open("Prop" + ds + ".txt").readlines()
for l in lines:
  PropQualities.append(float(l))


OurQualities = [] 
lines = open("Our" + ds + ".txt").readlines()
for l in lines:
  OurQualities.append(float(l))

i = 0
PercImp = []
for x in PropQualities:
  if x > 0.05:
    PercImp.append(100.0 * (OurQualities[i] - x) / x)
  i += 1

print len(PercImp)

#print PercImp
PercImp = sorted(PercImp)
n = len(PercImp)
X = ['50%', '75%', '90%', '95%']
Y = [PercImp[5 * n / 10 - 1], PercImp[75 * n / 100 - 1], PercImp[90 * n / 100 - 1], PercImp[95 * n / 100 - 1]]
print Y

fig = figure.Figure()
ax1 = fig.add_subplot()

ax1.set_xlim([0, 3.75]) 
  
ax1.set_ylabel("% Improvement in \n Response Quality")
ax1.set_xlabel("Percentiles")

width = 0.25
offset = 0.25 
N = len(X)
ind = np.arange(N)
  
ax1.bar(ind + offset, Y, width, color='#167bc2', edgecolor = 'white', label=r'Prop-Split')

ax1.set_xticks(ind + offset + width/2)
ax1.set_xticklabels(X)

fig.plt.grid(axis='x')
fig.plt.savefig("FbGooglePercentileCdf.pdf", format="pdf", bbox_inches='tight')











