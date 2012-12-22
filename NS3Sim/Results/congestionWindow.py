#!/usr/bin/env python
import figure
import os
import sys

if len(sys.argv) < 3:
  print "Usage:", sys.argv[0], "<input_file> <output_file>"
  sys.exit(1)

try:
  input_file = open(sys.argv[1])
  output_file = sys.argv[2]
except:
  print "Error opening", sys.argv[1]
  sys.exit(1)

lines = input_file.readlines()[1:]

X1 = []
X2 = []
Y1 = []
Y2 = []

for line in lines:
  splits = line.split()
  if 'Socket0' in line:
    X1.append(float(splits[2]))
    Y1.append(float(splits[3]))
  elif 'Socket1' in line:
    X2.append(float(splits[2]))
    Y2.append(float(splits[3]))
 
fig = figure.Figure()
ax1 = fig.add_subplot()
ax1.set_ylabel("Y Axis")
ax1.set_xlabel("X Axis")

ax1.plot(X1, Y1, 
    linewidth=2, color='#33B5E5',
    label='Socket 1', clip_on=False)

ax1.plot(X2, Y2,
    linewidth=2, color='#99CC00',
    label='Socket 2', clip_on=False)

ax1.set_ylim([0, max(max(Y2), max(Y1))])
ax1.set_xlim([0, max(max(X2), max(X1))])

fig.set_legend(ax1, 'upper left')
fig.plt.savefig(output_file + ".pdf", format="pdf", bbox_inches='tight')
input_file.close()
