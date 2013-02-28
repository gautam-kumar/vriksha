require(rriskDistributions)
p1=c(0.5, 0.75, 0.9, 0.95, 0.99, 0.999)

q1=c(0.12, 0.15, 0.19, 0.21, 0.29, 0.37)
fit.perc(p=p1,q=q1,show.output=FALSE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 0.3, 1, 1, 1))

q1=c(0.67, 0.69, 0.72, 0.75, 0.80, 0.84)
fit.perc(p=p1,q=q1,show.output=FALSE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 0.3, 1, 1, 1))

q1=c(0.29, 0.40, 0.63, 0.76, 1.90, 4.87)
fit.perc(p=p1,q=q1,show.output=FALSE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 0.3, 1, 1, 1))

q1=c(0.55, 0.89, 1.52, 1.96, 4.48, 6.63)
fit.perc(p=p1,q=q1,show.output=FALSE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 0.3, 1, 1, 1))
