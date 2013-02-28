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
  if (len(durations) >= 2500) and (durations[len(durations) / 2 - 1] < 180.3):
    Means.append(sum(durations) / len(durations))
    Medians.append(durations[len(durations) / 2 - 1])
    Perc80.append(durations[8 * len(durations) / 10 - 1])
    Output.append(durations)
Means = sorted(Means)
Perc80 = sorted(Perc80)
Medians = sorted(Medians)

print len(Output)
for o in Output:
  print len(o)
  for d in o:
    print d



