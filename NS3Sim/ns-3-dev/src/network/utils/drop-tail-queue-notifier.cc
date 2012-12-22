/* -*- Mode:C++; c-file-style:"gnu"; indent-tabs-mode:nil; -*- */
/*
 * Created by Gautam Kumar (gautamk@outlook.com)
 */

#include "ns3/log.h"
#include "ns3/enum.h"
#include "ns3/uinteger.h"
#include "ns3/traced-value.h"
#include "ns3/trace-source-accessor.h"
#include "drop-tail-queue-notifier.h"

NS_LOG_COMPONENT_DEFINE ("DropTailQueueNotifier");

namespace ns3 {

NS_OBJECT_ENSURE_REGISTERED (DropTailQueueNotifier);

TypeId DropTailQueueNotifier::GetTypeId (void) 
{
  static TypeId tid = TypeId ("ns3::DropTailQueueNotifier")
    .SetParent<Queue> ()
    .AddConstructor<DropTailQueueNotifier> ()
    .AddAttribute ("Mode", 
                   "Whether to use bytes (see MaxBytes) or packets (see MaxPackets) as the maximum queue size metric.",
                   EnumValue (QUEUE_MODE_BYTES),
                   MakeEnumAccessor (&DropTailQueueNotifier::SetMode),
                   MakeEnumChecker (QUEUE_MODE_BYTES, "QUEUE_MODE_BYTES",
                                    QUEUE_MODE_PACKETS, "QUEUE_MODE_PACKETS"))
    .AddAttribute ("MaxPackets", 
                   "The maximum number of packets accepted by this DropTailQueueNotifier.",
                   UintegerValue (100),
                   MakeUintegerAccessor (&DropTailQueueNotifier::m_maxPackets),
                   MakeUintegerChecker<uint32_t> ())
    .AddAttribute ("MaxBytes", 
                   "The maximum number of bytes accepted by this DropTailQueueNotifier.",
                   UintegerValue (100 * 65535),
                   MakeUintegerAccessor (&DropTailQueueNotifier::m_maxBytes),
                   MakeUintegerChecker<uint32_t> ())
    .AddTraceSource ("QueueOccupancy",
                     "Tracing the Queue occupancy for this queue.",
                     MakeTraceSourceAccessor (&DropTailQueueNotifier::m_bytesInQueue))
  ;

  return tid;
}

DropTailQueueNotifier::DropTailQueueNotifier () :
  Queue (),
  m_packets (),
  m_bytesInQueue (0)
{
  NS_LOG_FUNCTION_NOARGS ();
}

DropTailQueueNotifier::~DropTailQueueNotifier ()
{
  NS_LOG_FUNCTION_NOARGS ();
}

void
DropTailQueueNotifier::SetMode (DropTailQueueNotifier::QueueMode mode)
{
  NS_LOG_FUNCTION (mode);
  m_mode = mode;
}

DropTailQueueNotifier::QueueMode
DropTailQueueNotifier::GetMode (void)
{
  NS_LOG_FUNCTION_NOARGS ();
  return m_mode;
}

bool 
DropTailQueueNotifier::DoEnqueue (Ptr<Packet> p)
{
  NS_LOG_FUNCTION (this << p);

  if (m_mode == QUEUE_MODE_PACKETS && (m_packets.size () >= m_maxPackets))
    {
      NS_LOG_LOGIC ("Queue full (at max packets) -- droppping pkt");
      Drop (p);
      return false;
    }

  if (m_mode == QUEUE_MODE_BYTES && (m_bytesInQueue + p->GetSize () >= m_maxBytes))
    {
      NS_LOG_LOGIC ("Queue full (packet would exceed max bytes) -- droppping pkt");
      Drop (p);
      return false;
    }

  m_bytesInQueue += p->GetSize ();
  m_packets.push (p);

  NS_LOG_LOGIC ("Number packets " << m_packets.size ());
  NS_LOG_LOGIC ("Number bytes " << m_bytesInQueue);

  return true;
}

Ptr<Packet>
DropTailQueueNotifier::DoDequeue (void)
{
  NS_LOG_FUNCTION (this);

  if (m_packets.empty ())
    {
      NS_LOG_LOGIC ("Queue empty");
      return 0;
    }

  Ptr<Packet> p = m_packets.front ();
  m_packets.pop ();
  m_bytesInQueue -= p->GetSize ();

  NS_LOG_LOGIC ("Popped " << p);

  NS_LOG_LOGIC ("Number packets " << m_packets.size ());
  NS_LOG_LOGIC ("Number bytes " << m_bytesInQueue);

  return p;
}

Ptr<const Packet>
DropTailQueueNotifier::DoPeek (void) const
{
  NS_LOG_FUNCTION (this);

  if (m_packets.empty ())
    {
      NS_LOG_LOGIC ("Queue empty");
      return 0;
    }

  Ptr<Packet> p = m_packets.front ();

  NS_LOG_LOGIC ("Number packets " << m_packets.size ());
  NS_LOG_LOGIC ("Number bytes " << m_bytesInQueue);

  return p;
}

} // namespace ns3

