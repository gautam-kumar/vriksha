
for i in {1..15}
do  
  for d in 140000 145000 150000 155000 160000 165000 170000
  do 
    f1=Results_"$d"PropRerun.txt
    ./run spark.examples.CedarExp spark://ec2-184-73-67-64.compute-1.amazonaws.com:7077 $d 0 0 >> $f1 2>&1
    echo $i $d Prop 
    sleep 10
  done
done
