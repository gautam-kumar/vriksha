require(rriskDistributions)
p1=c(0.5, 0.75, 0.9, 0.95, 0.99, 0.999)
q1=c(0.195, 2.22, 7.51, 13.79, 27.31, 55.05)
fit.perc(p=p1,q=q1,show.output=FALSE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 0.3, 1, 1, 1))
