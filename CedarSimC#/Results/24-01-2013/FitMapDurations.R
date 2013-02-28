require(rriskDistributions)
p1=c(0.5, 0.75, 0.9, 0.95, 0.99, 0.999)
q1=c(87.27, 155.52, 336.76, 572.01, 1260.53, 5518.2)
fit.perc(p=p1,q=q1,show.output=TRUE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 0.3, 1, 1, 1))
