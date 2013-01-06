#ifndef MYMACROS_H
#define MYMACROS_H

// Balajee
#define PRINT_STATE(msg)                                                                                                    \
  cout << Simulator::Now ().GetSeconds () << ": Node " << m_node->GetId() << ": TCP STATE: " << msg << endl                 \

#define PRINT_STUFF(msg)                                                                                                    \
  cout << Simulator::Now ().GetSeconds () << ": Node " << m_node->GetId() << ": " << msg << endl                            \

#define PRINT_SIMPLE(msg)                                                                                                    \
  cout << Simulator::Now ().GetSeconds () << ": " << msg << endl                                                             \

#endif
