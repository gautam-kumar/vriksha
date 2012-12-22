/* -*- Mode:C++; c-file-style:"gnu"; indent-tabs-mode:nil; -*- */
/*
 * Created by Gautam Kumar (gautamk@outlook.com)
 */

#include "ns3/log.h"
#include "ns3/enum.h"
#include "ns3/uinteger.h"
#include "ns3/traced-value.h"
#include "ns3/trace-source-accessor.h"
#include "ecn-queue.h"

NS_LOG_COMPONENT_DEFINE ("EcnQueue");

namespace ns3 {

NS_OBJECT_ENSURE_REGISTERED (EcnQueue);

TypeId EcnQueue::GetTypeId (void) 
{
  static TypeId tid = TypeId ("ns3::EcnQueue")
    .SetParent<Queue> ()
    .AddConstructor<EcnQueue> ()
    .AddAttribute ("MaxBytes", 
                   "The maximum number of bytes accepted by this ECNQueue.",
                   UintegerValue (BUFFER_SIZE_BYTES),
                   MakeUintegerAccessor (&EcnQueue::m_maxBytes),
                   MakeUintegerChecker<uint32_t> ())
    .AddAttribute ("EcnThreshold",
                   "The threshold at which you start marking ECN.",
                   DoubleValue(ECN_THRESHOLD),
									 MakeDoubleAccessor (&EcnQueue::m_ecnThreshold),
                   MakeDoubleChecker<double> ())
    .AddTraceSource ("QueueOccupancy",
                     "Tracing the Queue occupancy for this queue.",
                     MakeTraceSourceAccessor (&EcnQueue::m_bytesInQueue)
                     );

  return tid;
}

EcnQueue::EcnQueue () :
  Queue (),
  m_packets (),
  m_bytesInQueue (0)
{
  NS_LOG_FUNCTION_NOARGS ();
}

EcnQueue::~EcnQueue ()
{
  NS_LOG_FUNCTION_NOARGS ();
}


bool
EcnQueue::MarkEcn (Ptr<Packet> p)
{
  NS_LOG_FUNCTION (this << p);
  Ipv4Header iph;
	PppHeader pp;
	
  p->RemoveHeader(pp);
	p->RemoveHeader(iph);
	//NS_LOG_UNCOND("HeaderRemoved");
	iph.SetEcn(Ipv4Header::CE);
  p->AddHeader(iph);
	p->AddHeader(pp);
	//NS_LOG_UNCOND("HeaderRemoved");
  return true;
}


bool 
EcnQueue::DoEnqueue (Ptr<Packet> p)
{
	//std::cout << "Enqueued: "; p->Print(std::cout); std::cout << std::endl;
  if (m_bytesInQueue + p->GetSize () >= m_maxBytes) {
    NS_LOG_LOGIC ("Queue full (packet would exceed max bytes) -- droppping pkt");
    Drop (p);
    return false; 
  }

  m_bytesInQueue += p->GetSize ();
 
  // Ecn is marked while enqueuing a packet.
  if (m_bytesInQueue >= m_ecnThreshold * m_maxBytes) {
    MarkEcn(p);
  }
  m_packets.push (p);

  NS_LOG_LOGIC ("Number packets " << m_packets.size ());
  NS_LOG_LOGIC ("Number bytes " << m_bytesInQueue);

  return true;
}

Ptr<Packet>
EcnQueue::DoDequeue (void)
{
  NS_LOG_FUNCTION (this);
	

  if (m_packets.empty ()) {
    NS_LOG_LOGIC ("Queue empty");
    return 0;
  }

  Ptr<Packet> p = m_packets.front ();
  m_packets.pop ();
  m_bytesInQueue -= p->GetSize ();

  NS_LOG_LOGIC ("Popped " << p);
	//std::cout << "Dequeued: "; p->Print(std::cout); std::cout << std::endl;
  NS_LOG_LOGIC ("Number packets " << m_packets.size ());
  NS_LOG_LOGIC ("Number bytes " << m_bytesInQueue);

  return p;
}

Ptr<const Packet>
EcnQueue::DoPeek (void) const
{
  NS_LOG_FUNCTION (this);

  if (m_packets.empty ()) {
    NS_LOG_LOGIC ("Queue empty");
    return 0;
  }

  Ptr<Packet> p = m_packets.front ();

  NS_LOG_LOGIC ("Number packets " << m_packets.size ());
  NS_LOG_LOGIC ("Number bytes " << m_bytesInQueue);

  return p;
}

} // namespace ns3

