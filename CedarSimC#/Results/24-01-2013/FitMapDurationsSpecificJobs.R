require(rriskDistributions)
p1=c(0.5, 0.75, 0.9, 0.95, 0.99, 0.999)

q1=c(23, 40, 87, 119, 181, 231)
fit.perc(p=p1,q=q1,show.output=FALSE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 0.3, 1, 1, 1))

q1=c(83, 107, 136, 158, 193, 256)
fit.perc(p=p1,q=q1,show.output=FALSE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 0.3, 1, 1, 1))

q1=c(384, 450, 486, 503, 532, 606)
fit.perc(p=p1,q=q1,show.output=FALSE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 0.3, 1, 1, 1))

q1=c(19, 27.8, 53.6, 71.06, 92.34, 140.86)
fit.perc(p=p1,q=q1,show.output=FALSE, tolPlot=0.1, tolConv=0.001, fit.weights=c(0.3, 0.3, 0.3, 1, 1, 1))
