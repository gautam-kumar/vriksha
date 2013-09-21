for d in 140000 145000 150000 155000 160000 165000 170000
do 
  f1=Results_"$d"_Prop.txt
  f2=Results_"$d"_Cedar.txt
  for i in {1..15}
  do
    ./run spark.examples.CedarExp spark://ec2-54-211-194-70.compute-1.amazonaws.com:7077 $d 0 >> $f1 2>&1
    echo $i $d Prop
    sleep 10
  done
  for i in {1..15}
  do
    ./run spark.examples.CedarExp spark://ec2-54-211-194-70.compute-1.amazonaws.com:7077 $d 1 >> $f2 2>&1
    echo $i $d Cedar
    sleep 10
  done
done
