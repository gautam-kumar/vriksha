#!/usr/bin/python

import figure

def ReadQueue(filename):
	X = []
	Y = []
	dc = open(filename).readlines()
	for l in dc:
		if "Queue" in l:
			s = l.split()
			X.append(float(s[1]))
			Y.append(float(s[2]))
	return [X, Y]

def ReadSocket(filename, socketNum):
  X = []
  Y = []
  dc = open(filename).readlines()
  for l in dc:
  	if "Socket" + str(socketNum) in l:
  		s = l.split()
  		X.append(float(s[2]))
  		Y.append(float(s[3]))
  return [X, Y]
