/* -*- Mode:C++; c-file-style:"gnu"; indent-tabs-mode:nil; -*- */
/*
 * Created by Gautam Kumar (gautamk@outlook.com)
 */

#ifndef ECNQUEUE_H
#define ECNQUEUE_H

#include <queue>
#include "ns3/packet.h"
#include "ns3/queue.h"
#include "ipv4-header.h"
#include "ns3/ppp-header.h"

#define BUFFER_SIZE_BYTES 500000 
#define ECN_THRESHOLD 0.6

namespace ns3 {

class TraceContainer;

/**
 * \brief A FIFO packet queue that drops tail-end packets on overflow
 */
class EcnQueue : public Queue {
public:
  static TypeId GetTypeId (void);
  /**
   * Creates a droptail queue with a maximum size of 100 packets by default
   */
  EcnQueue();

  virtual ~EcnQueue();

private:
  virtual bool DoEnqueue (Ptr<Packet> p);
  virtual Ptr<Packet> DoDequeue (void);
  virtual Ptr<const Packet> DoPeek (void) const;
  bool MarkEcn (Ptr<Packet> p); 

  std::queue<Ptr<Packet> > m_packets;
  uint32_t m_maxPackets;
  uint32_t m_maxBytes;
  double m_ecnThreshold;
  TracedValue<uint32_t> m_bytesInQueue; 
};

} // namespace ns3

#endif /* ECNQUEUE_H */
