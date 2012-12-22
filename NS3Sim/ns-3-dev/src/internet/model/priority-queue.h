/* -*- Mode:C++; c-file-style:"gnu"; indent-tabs-mode:nil; -*- */
/*
 * Copyright (c) 2007 University of Washington
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2 as
 * published by the Free Software Foundation;
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

#ifndef DROPTAIL_H
#define DROPTAIL_H

#include <queue>
#include "ipv4-header.h"
#include "ns3/packet.h"
#include "ns3/queue.h"
#include "ns3/seq-ts-header.h"
#include "tcp-header.h"

#define NUM_PRIORITY_LEVELS 5  
#define BUFFER_SIZE_BYTES 1000000

namespace ns3 {

//defining priority queue
class PriorityQueue : public Queue {
public:
  static TypeId GetTypeId (void);
  /**
   * \brief PriorityQueue Constructor
   *
   * Creates a droptail queue with a maximum size of 100 packets by default
   */
  PriorityQueue ();

  virtual ~PriorityQueue();

private:
  virtual bool DoEnqueue (Ptr<Packet> p);
  virtual Ptr<Packet> DoDequeue (void);
  virtual Ptr<const Packet> DoPeek (void) const;
  bool DropPacket();
  Ipv4Header* GetIpv4Header(Ptr<const Packet> p);
  uint16_t GetPriorityHeader(Ptr<const Packet>);
  
  std::deque<Ptr<Packet> > m_packets[NUM_PRIORITY_LEVELS];
  uint32_t m_bytesInSubQueue[NUM_PRIORITY_LEVELS];
  uint32_t m_totalPackets;
  uint32_t m_maxBytes;
  uint32_t m_bytesInQueue;
};
} // namespace ns3

#endif /* DROPTAIL_H */
