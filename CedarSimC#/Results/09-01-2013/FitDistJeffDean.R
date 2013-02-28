require(rriskDistributions)
p1=c(0.5, 0.9, 0.99, 0.999)
q1=c(19, 35, 67, 108)
fit.perc(p=p1,q=q1,show.output=FALSE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 1, 1))
