import figure
import plotter
import parser

X = [1000.0 / x for x in range(50, 11, -2)]

print X
lines = open('results-30ms.txt').readlines()
Y = parser.ParseTimeAndIc(lines)
plotter.PlotN([X, X, X, X, X], Y[0:5], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="time30", ext="pdf") 
plotter.PlotN([X, X, X, X, X], Y[5:10], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="ic30", ext="pdf") 
Y = parser.ParseNumCompleted(lines)
plotter.PlotN([X, X, X, X, X], Y, labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="nc30", ext="pdf") 


lines = open('flows-30ms.txt').readlines()
Y = parser.ParseFlowTimes(lines)
plotter.PlotN([X, X, X, X, X], Y[0:5], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-99-30", ext="pdf") 
plotter.PlotN([X, X, X, X, X], Y[5:10], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-mean-30", ext="pdf")
plotter.PlotN([X, X, X, X, X], Y[10:15], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-99_5-30", ext="pdf") 


lines = open('results-20ms.txt').readlines()
Y = parser.ParseTimeAndIc(lines)
plotter.PlotN([X, X, X, X, X], Y[0:5], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="time20", ext="pdf") 
plotter.PlotN([X, X, X, X, X], Y[5:10], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="ic20", ext="pdf") 
Y = parser.ParseNumCompleted(lines)
plotter.PlotN([X, X, X, X, X], Y, labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="nc20", ext="pdf")


lines = open('flows-20ms.txt').readlines()
Y = parser.ParseFlowTimes(lines)
plotter.PlotN([X, X, X, X, X], Y[0:5], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-99-20", ext="pdf") 
plotter.PlotN([X, X, X, X, X], Y[5:10], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-mean-20", ext="pdf")
plotter.PlotN([X, X, X, X, X], Y[10:15], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-99_5-20", ext="pdf")


lines = open('results-10ms.txt').readlines()
Y = parser.ParseTimeAndIc(lines)
plotter.PlotN([X, X, X, X, X], Y[0:5], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="time10", ext="pdf") 
plotter.PlotN([X, X, X, X, X], Y[5:10], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="ic10", ext="pdf") 
Y = parser.ParseNumCompleted(lines)
plotter.PlotN([X, X, X, X, X], Y, labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="nc10", ext="pdf")


lines = open('flows-10ms.txt').readlines()
Y = parser.ParseFlowTimes(lines)
plotter.PlotN([X, X, X, X, X], Y[0:5], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-99-10", ext="pdf") 
plotter.PlotN([X, X, X, X, X], Y[5:10], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-mean-10", ext="pdf")
plotter.PlotN([X, X, X, X, X], Y[10:15], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-99_5-10", ext="pdf")


lines = open('results-infinite.txt').readlines()
Y = parser.ParseTimeAndIc(lines)
plotter.PlotN([X, X, X, X, X], Y[0:5], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="timeInf", ext="pdf") 
plotter.PlotN([X, X, X, X, X], Y[5:10], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="icInf", ext="pdf") 
Y = parser.ParseNumCompleted(lines)
plotter.PlotN([X, X, X, X, X], Y, labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="ncInf", ext="pdf")


lines = open('flows-infinite.txt').readlines()
Y = parser.ParseFlowTimes(lines)
plotter.PlotN([X, X, X, X, X], Y[0:5], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-99-Inf", ext="pdf") 
plotter.PlotN([X, X, X, X, X], Y[5:10], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-mean-Inf", ext="pdf")
plotter.PlotN([X, X, X, X, X], Y[10:15], labels=['TCP', 'EDF-NP', 'EDF-P', 'Vriksha', 'SJF-P'], outputFile="flow-99_5-Inf", ext="pdf")
