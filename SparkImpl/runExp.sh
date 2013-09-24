
for i in {1..15}
do  
  for d in 140000 145000 150000 155000 160000 165000 170000
  do 
    f1=Results_"$d"_Cedar_Emp.txt
    ./run spark.examples.CedarExp spark://ec2-54-242-223-217.compute-1.amazonaws.com:7077 $d 1 0 >> $f1 2>&1
    echo $i $d Cedar 
    sleep 10
  done
done
