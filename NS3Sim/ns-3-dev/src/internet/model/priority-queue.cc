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

#include <iostream>
#include "ns3/ptr.h"
#include "ipv4-header.h"
#include "tcp-header.h"
#include "ns3/header.h"
#include "ns3/log.h"
#include "ns3/enum.h"
#include "ns3/uinteger.h"
#include "priority-queue.h"

NS_LOG_COMPONENT_DEFINE ("PriorityQueue");

namespace ns3 {

//priority queue

NS_OBJECT_ENSURE_REGISTERED (PriorityQueue);

TypeId PriorityQueue::GetTypeId (void) 
{
  static TypeId tid = TypeId ("ns3::PriorityQueue")
    .SetParent<Queue> ()
    .AddConstructor<PriorityQueue> ()
    .AddAttribute ("MaxBytes", 
                   "The maximum number of bytes accepted by this PriorityQueue.",
                   UintegerValue (BUFFER_SIZE_BYTES),
                   MakeUintegerAccessor (&PriorityQueue::m_maxBytes),
                   MakeUintegerChecker<uint32_t> ())
  ;

  return tid;
}

PriorityQueue::PriorityQueue () :
  Queue (),
  m_totalPackets (0),
  m_bytesInQueue (0)
{
  for(int i = 0; i < NUM_PRIORITY_LEVELS; i++) {
    m_bytesInSubQueue[i] = 0;
  }
  NS_LOG_FUNCTION_NOARGS ();
}

PriorityQueue::~PriorityQueue ()
{
  NS_LOG_FUNCTION_NOARGS ();
}

uint16_t 
PriorityQueue::GetPriorityHeader(Ptr <const Packet> p)
{
  Ipv4Header *ipv4Header = GetIpv4Header(p);
  if (ipv4Header != NULL) {
    return (uint16_t)(ipv4Header->GetDscp() & 7);
  } else {
    return -1;
  }
}


Ipv4Header*
PriorityQueue::GetIpv4Header(Ptr <const Packet> p)
{
  // first, copy the packet
  Ptr<Packet> q = p->Copy();
  // use indicator to search the packet
  PacketMetadata::ItemIterator metadataIterator = q->BeginItem();
  PacketMetadata::Item item;
  while (metadataIterator.HasNext())
  {   
    item = metadataIterator.Next();
    //NS_LOG_FUNCTION("item name: " << item.tid.GetName());
    if((item.type==PacketMetadata::Item::HEADER)&&(item.tid.GetName() == "ns3::Ipv4Header"))
    {   
      NS_ASSERT(item.tid.HasConstructor());
      Callback<ObjectBase *> constructor = item.tid.GetConstructor();
      NS_ASSERT(!constructor.IsNull());
               
      // Ptr<> and DynamicCast<> won't work here as all headers are from ObjectBase, not Object
      ObjectBase *instance = constructor();
      NS_ASSERT(instance != 0); 
                
      Ipv4Header *h = dynamic_cast<Ipv4Header*> (instance);
      NS_ASSERT(h != NULL);
               
      h->Deserialize(item.current);
      return h;   
    }   
  }   
  return NULL;
}

bool 
PriorityQueue::DoEnqueue(Ptr<Packet> p)
{
  uint32_t priority = GetPriorityHeader(p);
  NS_ASSERT(priority >= 0 && priority < NUM_PRIORITY_LEVELS);  
  uint32_t bytesToRemove = m_bytesInQueue + p->GetSize() - m_maxBytes;
  if (bytesToRemove > 0) { // Something must be dropped
    uint32_t bytesBelow = 0;
    for (uint32_t i = NUM_PRIORITY_LEVELS - 1; i > priority; i--) {
      bytesBelow += m_bytesInSubQueue[i];
    }
    if (bytesBelow >= bytesToRemove) { // Packet can be successfully enqueued
      uint32_t bytesRemoved = 0;
      for (uint32_t i = NUM_PRIORITY_LEVELS - 1; i > priority; i--) {
        while (bytesRemoved < bytesToRemove && !m_packets[i].empty()) {
          bytesRemoved -= m_packets[i].back()->GetSize();
          m_bytesInSubQueue[i] -= m_packets[i].back()->GetSize();
          m_bytesInQueue -= m_packets[i].back()->GetSize();
          m_packets[i].pop_back();
        }
      }
     } else {
       // Not enough space in lower queue to drop
       return false;
     }
  }
  // If the code comes here it should have enough space
  NS_ASSERT(m_bytesInQueue + p->GetSize() <= m_maxBytes);
  m_packets[priority].push_back(p);
  m_bytesInSubQueue[priority] += p->GetSize();
  m_bytesInQueue += p->GetSize();
  m_totalPackets++;
 
  NS_LOG_LOGIC ("Number packets " << m_totalPackets);
  NS_LOG_LOGIC ("Number bytes " << m_bytesInQueue);

  return true;
}


Ptr<Packet>
PriorityQueue::DoDequeue (void)
{
  //dequeue based on priority
  for (int i = 0; i < NUM_PRIORITY_LEVELS; i++)
  {
    if (!m_packets[i].empty()) {
      Ptr<Packet> p = m_packets[i].front();
      m_packets[i].pop_front();
      m_bytesInSubQueue[i] -= p->GetSize();
      m_bytesInQueue -= p->GetSize();
      m_totalPackets--;
     
      NS_LOG_LOGIC ("Popped " << p);
      NS_LOG_LOGIC ("Number packets " << m_totalPackets);
      NS_LOG_LOGIC ("Number bytes " << m_bytesInQueue);
      return p;
    }
     
  }
  NS_LOG_LOGIC ("All queues empty");
  return 0;
}

/* Returns the packet that is next to be dequeued
 * without dequeueing the packet */
Ptr<const Packet>
PriorityQueue::DoPeek (void) const
{
  NS_LOG_FUNCTION (this);
  for (int i = 0; i < NUM_PRIORITY_LEVELS; i++)
  {
    if (!m_packets[i].empty ())
    {
      NS_LOG_LOGIC("Number packets: " << m_totalPackets);
      NS_LOG_LOGIC("Number bytes: " << m_bytesInQueue);
      return m_packets[i].front();
    }
  }
  NS_LOG_LOGIC ("All queues empty");
  return 0;
}





} // namespace ns3
