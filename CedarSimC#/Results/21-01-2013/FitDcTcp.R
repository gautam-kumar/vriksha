require(rriskDistributions)
p1=c(0.5, 0.6, 0.7, 0.8, 0.9, 0.95, 0.99, 0.999, 0.9999)
q1=c(0.338, 00.445, 0.647, 0.82, 1.114, 5.013, 14.37, 28.93, 212.27)
fit.perc(p=p1,q=q1,show.output=FALSE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 0.3, 0.3, 0.3, 1, 1, 1, 0.1))
