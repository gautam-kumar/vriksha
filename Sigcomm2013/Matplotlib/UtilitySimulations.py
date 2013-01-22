#!/usr/bin/env python
import plotter
import os
import sys
import math
import scipy.stats as stats
sys.path.append("~/Work/Templates/Matplotlib")

lines = open("UtilitySimulations.dat").readlines()
i = 0
val1 = float(lines[i].split()[0])
num = int(lines[i].split()[1])
X1 = []
Y1 = []
i += 1
for j in range(num):
  X1.append(1000 * float(lines[i].split()[0]))
  Y1.append(float(lines[i].split()[1]))
  i += 1

val2 = float(lines[i].split()[0])
num = int(lines[i].split()[1])
X2 = []
Y2 = []
i += 1
for j in range(num):
  X2.append(1000 * float(lines[i].split()[0]))
  Y2.append(float(lines[i].split()[1]))
  i += 1

  
plotter.PlotN([X1, X2], [Y1, Y2], Y="Expected Utility", X="Wait Time", labels=[str(val1) + 'ms', str(val2) + 'ms'], outputFile="UtilitySimulationsExp", xAxis=[0, 150], yAxis=[0, 40], ext="pdf")


val1 = float(lines[i].split()[0])
num = int(lines[i].split()[1])
X1 = []
Y1 = []
i += 1
for j in range(num):
  X1.append(1000 * float(lines[i].split()[0]))
  Y1.append(float(lines[i].split()[1]))
  i += 1

val2 = float(lines[i].split()[0])
num = int(lines[i].split()[1])
X2 = []
Y2 = []
i += 1
for j in range(num):
  X2.append(1000 * float(lines[i].split()[0]))
  Y2.append(float(lines[i].split()[1]))
  i += 1
  
plotter.PlotN([X1, X2], [Y1, Y2], Y="Expected Utility", X="Wait Time", labels=[str(val1) + 'ms', str(val2) + 'ms'], outputFile="UtilitySimulationsNormal", xAxis=[0, 150], yAxis=[0, 40], ext="pdf")
