#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import figure
import numpy as np

D = []
Prop50 = []
Our50 = []
Prop75 = []
Our75 = []
Prop90 = []
Our90 = []


D = [140, 145, 150, 155, 160, 165, 170]
lines = open("PropDeadline.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Prop50.append(float(s[5]))
    Prop75.append(float(s[9]))
    Prop90.append(float(s[13]))

lines = open("OurDeadline.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Our50.append(float(s[5]))
    Our75.append(float(s[9]))
    Our90.append(float(s[13]))


Opt50 = []
lines = open("OptDeadline.txt").readlines()
for l in lines:
  if "IC50" in l:
    s = l.split()
    Opt50.append(float(s[5]))




i1 = [ 100.0 * (y - x) / x for x, y in zip(Prop50, Our50) ]
i2 = [ 100.0 * (y - x) / x for x, y in zip(Prop75, Our75) ]
i3 = [ 100.0 * (y - x) / x for x, y in zip(Prop90, Our90) ]

fig = figure.Figure()
ax1 = fig.add_subplot()

ax1.set_xlim([0, 7.5])
ax1.set_ylim([0, 1.09]) 
ax1.set_ylabel("Avg. Response Quality")
ax1.set_xlabel("Deadline (ms)")

width = 0.25
offset = 0.5
N = len(D)
ind = np.arange(N)
  
ax1.bar(ind + offset, Prop50[:-2], width, color='#167bc2', edgecolor = 'white', label=r'Proportional-Split')
ax1.bar(ind + offset + width, Our50[:-2], width, color='#002b36', edgecolor = 'white', label='Cedar')
ax1.bar(ind + offset + 2 * width, Opt50, width, color='#A5B920', edgecolor = 'white', label='Ideal')



j = 0 
for i in ind:
  ax1.text(i + offset + 0.5 * width, Our50[j] + 0.01, str(int(i1[j])) + "%", fontsize = 16, weight='bold') 
#  ax1.text(i + offset + + width + width/5, t2_rack[j], '$B$', fontsize = 16) 
  j += 1


ax1.set_xticks(ind + offset + width)
ax1.set_xticklabels(D)

handles, labels = ax1.get_legend_handles_labels()
ax1.legend(handles, labels, loc='upper center',
    ncol=3, prop={'size':15})

#leg = fig.plt.gca().get_legend()
#leg.draw_frame(False)
fig.plt.grid(axis='x')
fig.plt.savefig("FbGoogleDeadlineBarChart.pdf", format="pdf", bbox_inches='tight') 

