#!/usr/bin/python
import re


def ParseTimeAndIc(lines):
  Perc99Tcp = []; Perc99EdfNp = []; Perc99EdfP = []; Perc99Vriksha = []; Perc99SjfP = [];
  IcTcp = []; IcEdfNp = []; IcEdfP = []; IcVriksha = []; IcSjfP = [];
  i = 0;
  for l in lines:
    regex = '; |, |\ |\*| \s+'
    s = re.split(regex, l)    
    if "TCP" in l:
      Perc99Tcp.append(float(s[13]));
      IcTcp.append(float(s[15]));
    if "EDF-NP" in l:
      Perc99EdfNp.append(float(s[13]));
      IcEdfNp.append(float(s[15]));
    if "EDF-P" in l:
      Perc99EdfP.append(float(s[13])); 
      IcEdfP.append(float(s[15]));
    if "Vriksha" in l:
      Perc99Vriksha.append(float(s[13]));
      IcVriksha.append(float(s[15]));
    if "SJF-P" in l:
      Perc99SjfP.append(float(s[13]));
      IcSjfP.append(float(s[15]));
  return [Perc99Tcp, Perc99EdfNp, Perc99EdfP, Perc99Vriksha, Perc99SjfP, \
    IcTcp, IcEdfNp, IcEdfP, IcVriksha, IcSjfP]

def ParseFlowTimes(lines):  
  Tcp99 = []; EdfNp99 = []; EdfP99 = []; Vriksha99 = []; SjfP99 = [];
  TcpMean = []; EdfNpMean = []; EdfPMean = []; VrikshaMean = []; SjfMean = [];
  Tcp99_5 = []; EdfNp99_5 = []; EdfP99_5 = []; Vriksha99_5 = []; SjfP99_5 = [];
  for l in lines:
    regex = '; |, |\ |\*| \s+'
    s = re.split(regex, l)    
    if "TCP" in l:
      Tcp99.append(float(s[9]));
      TcpMean.append(float(s[5]));
      Tcp99_5.append(float(s[11]));
    if "EDF-NP" in l:
      EdfNp99.append(float(s[9]));
      EdfNpMean.append(float(s[5]));
      EdfNp99_5.append(float(s[11]));
    if "EDF-P" in l:
      EdfP99.append(float(s[9]));
      EdfPMean.append(float(s[5]));
      EdfP99_5.append(float(s[11]));
    if "Vriksha" in l:
      Vriksha99.append(float(s[9]));
      VrikshaMean.append(float(s[5]));
      Vriksha99_5.append(float(s[11]));
    if "SJF-P" in l:
      SjfP99.append(float(s[9]));
      SjfMean.append(float(s[5]));
      SjfP99_5.append(float(s[11]));
      
  return [Tcp99, EdfNp99, EdfP99, Vriksha99, SjfP99, \
      TcpMean, EdfNpMean, EdfPMean, VrikshaMean, SjfMean, \
      Tcp99_5, EdfNp99_5, EdfP99_5, Vriksha99_5, SjfP99_5]

