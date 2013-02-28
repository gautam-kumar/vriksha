import figure
import plotter
import parser

Obj_026 = []
lines = open('Bharath_0.0260_Obj.dat').readlines()
for l in lines:
	splits = l.split(',')
	for s in splits:
		Obj_026.append(float(s))
X = range(len(Obj_026))

Obj_01 = []
lines = open('Bharath_0.01_Obj.dat').readlines()
for l in lines:
	splits = l.split(',')
	for s in splits:
		Obj_01.append(float(s))

Obj_005 = []
lines = open('Bharath_0.005_Obj.dat').readlines()
for l in lines:
	splits = l.split(',')
	for s in splits:
		Obj_005.append(float(s))

Obj_0014 = []
lines = open('Bharath_0.0014_Obj.dat').readlines()
for l in lines:
	splits = l.split(',')
	for s in splits:
		Obj_0014.append(float(s))

Obj_001 = []
lines = open('Bharath_0.001_Obj.dat').readlines()
for l in lines:
	splits = l.split(',')
	for s in splits:
		Obj_001.append(float(s))

Obj_0005 = []
lines = open('Bharath_0.0005_Obj.dat').readlines()
for l in lines:
	splits = l.split(',')
	for s in splits:
		Obj_0005.append(float(s))

Obj_0001 = []
lines = open('Bharath_0.0001_Obj.dat').readlines()
for l in lines:
	splits = l.split(',')
	for s in splits:
		Obj_0001.append(float(s))

Obj_00005 = []
lines = open('Bharath_0.00005_Obj.dat').readlines()
for l in lines:
	splits = l.split(',')
	for s in splits:
		Obj_00005.append(float(s))

Obj_00001 = []
lines = open('Bharath_0.00001_Obj.dat').readlines()
for l in lines:
	splits = l.split(',')
	for s in splits:
		Obj_00001.append(float(s))

Obj_000005 = []
lines = open('Bharath_0.000005_Obj.dat').readlines()
for l in lines:
	splits = l.split(',')
	for s in splits:
		Obj_000005.append(float(s))

Obj_0 = []
lines = open('Bharath_0_Obj.dat').readlines()
for l in lines:
  splits = l.split(',')
  for s in splits:
  	Obj_0.append(float(s))
Obj_0 = Obj_0[:1000]


plotter.PlotN([X, X, X, X], [Obj_026, Obj_01, Obj_005, Obj_0014], labels=['0.0260', '0.010', '0.005', '0.0014'], X='Iteration', Y='Objective Function Value', \
    xAxis=[0, 1000], \
    legendLoc='upper right', outputFile="ObjectiveFunctionNoConverge", ext="pdf") 

plotter.PlotN([X, X, X, X, X, X, X, X], [Obj_0014, Obj_001, Obj_0005, Obj_0001, Obj_00005, Obj_00001, Obj_000005, Obj_0], labels=['0.0014', '0.001', '0.0005', '0.0001', '0.00005', '0.00001', '0.000005', '0'], X='Iteration', Y='Objective Function Value', \
    xAxis=[0, 1000], \
    legendLoc='upper right', outputFile="ObjectiveFunctionConvergeAllIterations", ext="pdf") 

plotter.PlotN([X[:40], X[:40], X[:40], X[:40], X[:40], X[:40], X[:40], X[:40]], [Obj_0014[:40], Obj_001[:40], Obj_0005[:40], Obj_0001[:40], Obj_00005[:40], Obj_00001[:40], Obj_000005[:40], Obj_0[:40]], labels=['0.0014', '0.001', '0.0005', '0.0001', '0.00005', '0.00001', '0.000005', '0'], X='Iteration', Y='Objective Function Value', \
    xAxis=[0, 40], \
    legendLoc='upper right', outputFile="ObjectiveFunctionConvergeFirst40Iterations", ext="pdf") 


plotter.PlotN([X[-40:], X[-40:], X[-40:], X[-40:], X[-40:], X[-40:], X[-40:], X[-40:]], \
    [Obj_0014[-40:], Obj_001[-40:], Obj_0005[-40:], Obj_0001[-40:], Obj_00005[-40:], Obj_00001[-40:], Obj_000005[-40:], Obj_0[-40:]], labels=['0.0014', '0.001', '0.0005', '0.0001', '0.00005', '0.00001', '0.000005', '0'], X='Iteration', Y='Objective Function Value', \
    xAxis=[960, 1000],\
    legendLoc='upper right', outputFile="ObjectiveFunctionConvergeLast40Iterations", ext="pdf") 


 
