lines = open("MapDurationsPerJob.txt").readlines()
numJobs = int(lines[0])
Means = []
Perc80 = []
Medians = []
Output = []
j = 1
for i in range(numJobs):
  durations = []
  numTasks = int(lines[j])
  j += 1
  for k in range(numTasks):
    durations.append(float(lines[j]))
    j += 1
  durations = sorted(durations)
  Means.append(sum(durations) / len(durations))
  Medians.append(durations[len(durations) / 2 - 1])
  if durations[len(durations) / 2 - 1] < 180.3:
  	Output.append(durations)
  Perc80.append(durations[8 * len(durations) / 10 - 1])
Means = sorted(Means)
Perc80 = sorted(Perc80)
Medians = sorted(Medians)

print len(Output)
f = open("MapDurationsPruned.txt", "w")
f.write(str(len(Output)) + "\n")
for o in Output:	
  f.write(str(len(o)) + "\n")
  for d in o:
  	f.write(str(d) + "\n")

f = open("MapDurationsMeans.txt", "w")
for m in Means:
	f.write(str(m) + "\n")
f.close()


f = open("MapDurationsPerc80.txt", "w")
for m in Perc80:
	f.write(str(m) + "\n")
f.close()

f = open("MapDurationsMedian.txt", "w")
for m in Medians:
	f.write(str(m) + "\n")
f.close()



