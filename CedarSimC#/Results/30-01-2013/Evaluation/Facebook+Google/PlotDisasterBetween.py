#!/usr/bin/env python
import sys
import os
import math
sys.path.append("~/Work/Templates/Matplotlib")
import plotter
import numpy as np


Static = [0.1388, 0.2336, 0.332, 0.4764, 0.628, 0.7176, 0.7616]
Our = [0.7488, 0.7856, 0.8048, 0.828, 0.8468, 0.8672, 0.880000000000001]
D = [140, 145, 150, 155, 160, 165, 170]

plotter.PlotBarChart(D, [Static, Our], labels=['Static', 'Cedar'],\
    YTitle='Avg. Response Quality', XTitle='Deadline (ms)', \
    xAxis=[0, 7.501], yAxis=[0, 1.00001],
    isLegendOutside=True, bboxPos=[0.25, 1.2],
    outputFile='BarChartDemo')

