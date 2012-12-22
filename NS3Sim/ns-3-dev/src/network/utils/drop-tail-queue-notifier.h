/* -*- Mode:C++; c-file-style:"gnu"; indent-tabs-mode:nil; -*- */
/*
 * Created by Gautam Kumar (gautamk@outlook.com)
 */

#ifndef DROPTAILNOTIFIER_H
#define DROPTAILNOTIFIER_H

#include <queue>
#include "ns3/packet.h"
#include "ns3/queue.h"

namespace ns3 {

class TraceContainer;

/**
 * \brief A FIFO packet queue that drops tail-end packets on overflow
 */
class DropTailQueueNotifier : public Queue {
public:
  static TypeId GetTypeId (void);
  /**
   * Creates a droptail queue with a maximum size of 100 packets by default
   */
  DropTailQueueNotifier();

  virtual ~DropTailQueueNotifier();

  /**
   * Set the operating mode of this device.
   */
  void SetMode (DropTailQueueNotifier::QueueMode mode);

  /**
   * Get the encapsulation mode of this device.
   */
  DropTailQueueNotifier::QueueMode GetMode (void);

private:
  virtual bool DoEnqueue (Ptr<Packet> p);
  virtual Ptr<Packet> DoDequeue (void);
  virtual Ptr<const Packet> DoPeek (void) const;

  std::queue<Ptr<Packet> > m_packets;
  uint32_t m_maxPackets;
  uint32_t m_maxBytes;
  TracedValue<uint32_t> m_bytesInQueue; 
  QueueMode m_mode;
};

} // namespace ns3

#endif /* DROPTAIL_H */
