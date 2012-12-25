/* -*- Mode:C++; c-file-style:"gnu"; indent-tabs-mode:nil; -*- */
/*
 * Copyright (c) 2010 Adrian Sai-wah Tam
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
 *
 * Author: Adrian Sai-wah Tam <adrian.sw.tam@gmail.com>
 */

#define NS_LOG_APPEND_CONTEXT \
  if (m_node) { std::clog << Simulator::Now ().GetSeconds () << " [node " << m_node->GetId () << "] "; }

#include "tcp-d2.h"
#include "ns3/log.h"
#include "ns3/trace-source-accessor.h"
#include "ns3/simulator.h"
#include "ns3/abort.h"
#include "ns3/node.h"

#include "ns3/ecn-tag.h"
#include "ns3/packet.h"

NS_LOG_COMPONENT_DEFINE ("TcpD2");

namespace ns3 {

NS_OBJECT_ENSURE_REGISTERED (TcpD2);

TypeId
TcpD2::GetTypeId (void)
{
  static TypeId tid = TypeId ("ns3::TcpD2")
    .SetParent<TcpSocketBase> ()
    .AddConstructor<TcpD2> ()
    .AddAttribute ("ReTxThreshold", "Threshold for fast retransmit",
                    UintegerValue (3),
                    MakeUintegerAccessor (&TcpD2::m_retxThresh),
                    MakeUintegerChecker<uint32_t> ())
    .AddAttribute ("LimitedTransmit", "Enable limited transmit",
		    BooleanValue (false),
		    MakeBooleanAccessor (&TcpD2::m_limitedTx),
		    MakeBooleanChecker ())
    .AddTraceSource ("CongestionWindow",
                     "The TCP connection's congestion window",
                     MakeTraceSourceAccessor (&TcpD2::m_cWnd))
  ;
  return tid;
}

TcpD2::TcpD2 (void)
  : m_retxThresh (3), // mute valgrind, actual value set by the attribute system
    m_inFastRec (false),
    m_limitedTx (false), // mute valgrind, actual value set by the attribute system
    m_first_ack_on_established (true),
    m_tot_acks(1),
    m_ecn_acks(0),
    m_alpha_last(0.0),
    m_alpha(0.0),
    m_gamma_last(1.0),
    m_gamma(1.0),
    m_absDeadline(Seconds(0))
{
  NS_LOG_FUNCTION (this);
}

TcpD2::TcpD2 (const TcpD2& sock)
  : TcpSocketBase (sock),
    m_cWnd (sock.m_cWnd),
    m_ssThresh (sock.m_ssThresh),
    m_initialCWnd (sock.m_initialCWnd),
    m_retxThresh (sock.m_retxThresh),
    m_inFastRec (false),
    m_limitedTx (sock.m_limitedTx),
    m_first_ack_on_established (true),
    m_tot_acks(1),
    m_ecn_acks(0),
    m_alpha_last(0.0),
    m_alpha(0.0),
    m_gamma_last(1.0),
    m_gamma(1.0),
    m_absDeadline(Seconds(0))
{
  NS_LOG_FUNCTION (this);
  NS_LOG_LOGIC ("Invoked the copy constructor");
}

TcpD2::~TcpD2 (void)
{
}

/** We initialize m_cWnd from this function, after attributes initialized */
int
TcpD2::Listen (void)
{
  NS_LOG_FUNCTION (this);
  InitializeCwnd ();
  return TcpSocketBase::Listen ();
}

/** We initialize m_cWnd from this function, after attributes initialized */
int
TcpD2::Connect (const Address & address)
{
  NS_LOG_FUNCTION (this << address);
  InitializeCwnd ();
  return TcpSocketBase::Connect (address);
}

/** Limit the size of in-flight data by cwnd and receiver's rxwin */
uint32_t
TcpD2::Window (void)
{
  NS_LOG_FUNCTION (this);
  return std::min (m_rWnd.Get (), m_cWnd.Get ());
}

void
TcpD2::SetAlpha (Time deadline,Time RTT,uint32_t bytes)
{
  m_alpha_last = (double)m_ecn_acks/(double)m_tot_acks;

  m_alpha = ((1 - ALPHA_WEIGHT) * m_alpha) + (ALPHA_WEIGHT * m_alpha_last);
  

  Time Current = Simulator::Now();
  double m_tc1, m_tc2;
  int64_t m_deadline_remaining = ((deadline-Current).GetInteger())/(RTT.GetInteger()) - 1; 
  int64_t m_cWnd_in_mss        = m_cWnd / m_segmentSize;
  int64_t m_bytes_in_mss       = floor ( ((double)bytes / (double)m_segmentSize) + 0.50);
  double  gamma1, gamma2;
  
  m_tc1 = std::sqrt( std::pow(((double)m_cWnd_in_mss + 1.0)/2.0,2.0) + 2.0 * (double)m_bytes_in_mss) - ((double)m_cWnd_in_mss + 1.0)/2.0;
  if (m_deadline_remaining > 0 && m_tc1 > 0.0)
    gamma1 = m_tc1 / (double) m_deadline_remaining;
  else
    gamma1 = 0.0;
  
  m_tc2 = (double)m_bytes_in_mss / (3.0 * (double)m_cWnd_in_mss / 4.0 + 0.5);
  if (m_deadline_remaining > 0 && m_tc2 > 0.0)
    gamma2 = m_tc2 / (double) m_deadline_remaining;
  else
    gamma2 = 0.0;
  
  m_gamma_last = std::max(gamma1, gamma2);
  
  m_gamma = ((1 - GAMMA_WEIGHT) * m_gamma) + (GAMMA_WEIGHT * m_gamma_last);
  
  // cap m_gamma
  if (m_gamma <= 0.4)
    m_gamma = 0.4;
  else if (m_gamma >= 2.0)
    m_gamma = 2.0;

  if (!m_absDeadline.GetInteger()) {
    //PRINT_STUFF("Long Flow");
    m_gamma = 1.0;
  }

  double m_p;
  if (m_gamma != 0.0)
    m_p = std::pow(m_alpha,m_gamma);
  else
    m_p = 0.0;

  int value = floor( ((double) m_cWnd * (1.0 - m_p/2.0)) + 0.5);
  
  //PRINT_STUFF("alpha is " << m_alpha << " Gamma is " << m_gamma << " p is " << m_p << " Congestion Window modified from " << m_cWnd << " to " << value << " Deadline is " << m_deadline_remaining << " and Tc1 is " << m_tc1 << " Tc2 is " << m_tc2 << " RTT is " << RTT);
  m_cWnd    = value;

  // reset the counts for the next window!
  m_tot_acks = 1; // to avoid division by 0
  m_ecn_acks = 0;

}

Ptr<TcpSocketBase>
TcpD2::Fork (void)
{
  return CopyObject<TcpD2> (this);
}

void
TcpD2::DoForwardUp (Ptr<Packet> packet, Ipv4Header header, uint16_t port,
                            Ptr<Ipv4Interface> incomingInterface)
{
  // Peel off TCP header and do validity checking
  TcpHeader tcpHeader;
  packet->PeekHeader (tcpHeader);

  uint8_t tcpflags = tcpHeader.GetFlags (); //remove junk
  
  //Filter out ECE flags that should not be responded to
  if (tcpflags & TcpHeader::ECE)
    {
      if (tcpflags & (TcpHeader::FIN|TcpHeader::SYN|TcpHeader::RST|TcpHeader::PSH|TcpHeader::URG))
        {
          // we only care about ECN for data/ack packets, any other flag just clear ECN
          tcpflags = tcpflags & ~(TcpHeader::ECE);
          tcpHeader.SetFlags(tcpflags);
        } 

      else 
        {
          NS_ASSERT(tcpflags & (TcpHeader::ACK));
        }
    }
  

  //Add ecn tag containing Ip flags
  EcnTag etag;
  etag.SetEcn(header.GetTos() & 0x03);
  packet->AddPacketTag(etag);

  TcpSocketBase::DoForwardUp(packet, header, port, incomingInterface);
}

void
TcpD2::ModifyCwnd ()
{
  //PRINT_STUFF("modifying window by ECN");
  SetAlpha(m_absDeadline, m_rtt->GetCurrentEstimate(), m_txBuffer.Size());
  NS_LOG_UNCOND("Changing Congestion Window, Absolute: " << m_absDeadline << " Original: " << GetDeadline());
  //Time m_now = Simulator::Now();
  //if (m_deadline.GetInteger()) return;
  //if ((m_deadline - m_now).IsStrictlyNegative() && m_state == ESTABLISHED)
  //{
  //  PRINT_STUFF("Deadline2 not met!");
  //  //return;
  //}

  // reschedule for the next window
  if (m_rtt->GetCurrentEstimate() < MicroSeconds(10))
  {
    m_ecnmarkEvent = Simulator::Schedule (MicroSeconds(10), &TcpD2::ModifyCwnd, this);
  }
  else
  {
    m_ecnmarkEvent = Simulator::Schedule (m_rtt->GetCurrentEstimate (), &TcpD2::ModifyCwnd, this);
  }
}

/** Received a packet upon ESTABLISHED state. This function is mimicking the
    role of tcp_rcv_established() in tcp_input.c in Linux kernel. */
void
TcpD2::ProcessEstablished (Ptr<Packet> packet, const TcpHeader& tcpHeader)
{
  // Extract the flags. PSH and URG are not honoured.
  uint8_t tcpflags = tcpHeader.GetFlags () & ~(TcpHeader::PSH | TcpHeader::URG);
  
  // Different flags are different events
  if ((tcpflags == TcpHeader::ACK) || (tcpflags == (TcpHeader::ACK | TcpHeader::ECE)))
    {
      ReceivedAck (packet, tcpHeader);
      if((m_first_ack_on_established)&&(packet->GetSize() == 0))
        {
          Simulator::ScheduleNow(&TcpD2::ModifyCwnd,this);
          m_first_ack_on_established = false;
        }
    }
  else 
    {
      TcpSocketBase::ProcessEstablished(packet, tcpHeader);
    }
}

/** Process the newly received ACK */
void
TcpD2::ReceivedAck (Ptr<Packet> packet, const TcpHeader& tcpHeader)
{
  NS_LOG_FUNCTION (this << tcpHeader);

  // Received ACK. Compare the ACK number against highest unacked seqno
  if (0 == (tcpHeader.GetFlags () & TcpHeader::ACK))
    { // Ignore if no ACK flag
    }
  else if (tcpHeader.GetAckNumber () < m_txBuffer.HeadSequence ())
    { // Case 1: Old ACK, ignored.
      NS_LOG_LOGIC ("Ignored ack of " << tcpHeader.GetAckNumber ());
    }
  else if (tcpHeader.GetAckNumber () == m_txBuffer.HeadSequence ())
    { // Case 2: Potentially a duplicated ACK
      if (tcpHeader.GetAckNumber () < m_nextTxSequence)
        {
          NS_LOG_LOGIC ("Dupack of " << tcpHeader.GetAckNumber ());
          DupAck (tcpHeader, ++m_dupAckCount);
        }
      // otherwise, the ACK is precisely equal to the nextTxSequence
      NS_ASSERT (tcpHeader.GetAckNumber () <= m_nextTxSequence);
    }
  else if (tcpHeader.GetAckNumber () > m_txBuffer.HeadSequence ())
    { // Case 3: New ACK, reset m_dupAckCount and update m_txBuffer
      NS_LOG_LOGIC ("New ack of " << tcpHeader.GetAckNumber ());
      NewAck (tcpHeader.GetAckNumber (), tcpHeader.GetFlags() & TcpHeader::ECE);
      m_dupAckCount = 0;
    }
  // If there is any data piggybacked, store it into m_rxBuffer
  if (packet->GetSize () > 0)
    {
      ReceivedData (packet, tcpHeader);
    }
}

void
TcpD2::ProcessSynSent (Ptr<Packet> packet, const TcpHeader& tcpHeader) 
{  // Extract the flags. PSH and URG are not honoured.
  uint8_t tcpflags = tcpHeader.GetFlags () & ~(TcpHeader::PSH | TcpHeader::URG);
  
  if((tcpflags == 0) || (tcpflags == (TcpHeader::SYN | TcpHeader::ACK)))
    {
      m_absDeadline = MilliSeconds(GetDeadline()) + Simulator::Now();
      NS_LOG_UNCOND("Deadline is " << MilliSeconds(GetDeadline()));
    }
  
  TcpSocketBase::ProcessSynSent(packet,tcpHeader);
}

void
TcpD2::ProcessSynRcvd (Ptr<Packet> packet, const TcpHeader& tcpHeader,
                       const Address& fromAddress, const Address& toAddress)
{
  // Extract the flags. PSH and URG are not honoured.
  uint8_t tcpflags = tcpHeader.GetFlags () & ~(TcpHeader::PSH | TcpHeader::URG);
  
  if (tcpflags == 0
      || (tcpflags == TcpHeader::ACK
          && m_nextTxSequence + SequenceNumber32 (1) == tcpHeader.GetAckNumber ()))
    {
      m_absDeadline = MilliSeconds(GetDeadline()) + Simulator::Now();
      NS_LOG_UNCOND("Deadline is " << MilliSeconds(GetDeadline()));
    }

  TcpSocketBase::ProcessSynRcvd(packet,tcpHeader,fromAddress,toAddress);
}

/** New ACK (up to seqnum seq) received. Increase cwnd and call TcpSocketBase::NewAck() */
void
TcpD2::NewAck (const SequenceNumber32& seq, bool hasEcn)
{
  NS_LOG_FUNCTION (this << seq);
  NS_LOG_UNCOND ("TcpD2 receieved ACK for seq " << seq <<
                " cwnd " << m_cWnd <<
                " ssthresh " << m_ssThresh <<
                " Deadline " << GetDeadline() <<
                " Alpha " << m_alpha);

  // increment the counters
  m_tot_acks++;
  if(hasEcn) {
    m_ecn_acks++;
  }

  // Check for exit condition of fast recovery
  if (m_inFastRec && seq < m_recover)
    { // Partial ACK, partial window deflation (RFC2582 sec.3 bullet #5 paragraph 3)
      m_cWnd -= seq - m_txBuffer.HeadSequence ();
      m_cWnd += m_segmentSize;  // increase cwnd
      NS_LOG_INFO ("Partial ACK in fast recovery: cwnd set to " << m_cWnd);
      TcpSocketBase::NewAck (seq); // update m_nextTxSequence and send new data if allowed by window
      DoRetransmit (); // Assume the next seq is lost. Retransmit lost packet
      return;
    }
  else if (m_inFastRec && seq >= m_recover)
    { // Full ACK (RFC2582 sec.3 bullet #5 paragraph 2, option 1)
      m_cWnd = std::min (m_ssThresh, BytesInFlight () + m_segmentSize);
      m_inFastRec = false;
      NS_LOG_INFO ("Received full ACK. Leaving fast recovery with cwnd set to " << m_cWnd);
    }

  // Increase of cwnd based on current phase (slow start or congestion avoidance)
  if (m_cWnd < m_ssThresh)
    { // Slow start mode, add one segSize to cWnd. Default m_ssThresh is 65535. (RFC2001, sec.1)
      m_cWnd += m_segmentSize;
      NS_LOG_INFO ("In SlowStart, updated to cwnd " << m_cWnd << " ssthresh " << m_ssThresh);
    }
  else
    { // Congestion avoidance mode, increase by (segSize*segSize)/cwnd. (RFC2581, sec.3.1)
      // To increase cwnd for one segSize per RTT, it should be (ackBytes*segSize)/cwnd
      double adder = static_cast<double> (m_segmentSize * m_segmentSize) / m_cWnd.Get ();
      adder = std::max (1.0, adder);
      m_cWnd += static_cast<uint32_t> (adder);
      NS_LOG_INFO ("In CongAvoid, updated to cwnd " << m_cWnd << " ssthresh " << m_ssThresh);
    }

  // Complete newAck processing
  TcpSocketBase::NewAck (seq);
}

/** Cut cwnd and enter fast recovery mode upon triple dupack */
void
TcpD2::DupAck (const TcpHeader& t, uint32_t count)
{
  NS_LOG_FUNCTION (this << count);
  if (count == m_retxThresh && !m_inFastRec)
    { // triple duplicate ack triggers fast retransmit (RFC2582 sec.3 bullet #1)
      m_ssThresh = std::max (2 * m_segmentSize, BytesInFlight () / 2);
      m_cWnd = m_ssThresh + 3 * m_segmentSize;
      m_recover = m_highTxMark;
      m_inFastRec = true;
      NS_LOG_INFO ("Triple dupack. Enter fast recovery mode. Reset cwnd to " << m_cWnd <<
                   ", ssthresh to " << m_ssThresh << " at fast recovery seqnum " << m_recover);
      DoRetransmit ();
    }
  else if (m_inFastRec)
    { // Increase cwnd for every additional dupack (RFC2582, sec.3 bullet #3)
      m_cWnd += m_segmentSize;
      NS_LOG_INFO ("Dupack in fast recovery mode. Increase cwnd to " << m_cWnd);
      SendPendingData (m_connected);
    }
  else if (!m_inFastRec && m_limitedTx && m_txBuffer.SizeFromSequence (m_nextTxSequence) > 0)
    { // RFC3042 Limited transmit: Send a new packet for each duplicated ACK before fast retransmit
      NS_LOG_INFO ("Limited transmit");
      uint32_t sz = SendDataPacket (m_nextTxSequence, m_segmentSize, true);
      m_nextTxSequence += sz;                    // Advance next tx sequence
    };
}


// Receipt of new packet, put into Rx buffer
void
TcpD2::ReceivedData (Ptr<Packet> p, const TcpHeader& tcpHeader)
{
  NS_LOG_FUNCTION (this << tcpHeader);
  NS_LOG_LOGIC ("seq " << tcpHeader.GetSequenceNumber () <<
                " ack " << tcpHeader.GetAckNumber () <<
                " pkt size " << p->GetSize () );

  EcnTag etag;
  NS_ASSERT(p->PeekPacketTag(etag));
  
  uint8_t flags = 0;
  if(etag.GetEcn() == 0x03) {
    flags = TcpHeader::ECE;
  }

  // Put into Rx buffer
  SequenceNumber32 expectedSeq = m_rxBuffer.NextRxSequence ();
  if (!m_rxBuffer.Add (p, tcpHeader))
    { // Insert failed: No data or RX buffer full
      SendEmptyPacket (TcpHeader::ACK |flags);
      return;
    }
  // Now send a new ACK packet acknowledging all received and delivered data
  if (m_rxBuffer.Size () > m_rxBuffer.Available () || m_rxBuffer.NextRxSequence () > expectedSeq + p->GetSize ())
    { // A gap exists in the buffer, or we filled a gap: Always ACK
      SendEmptyPacket (TcpHeader::ACK|flags);
    }
  else
    { // In-sequence packet: ACK if delayed ack count allows
      if (++m_delAckCount >= m_delAckMaxCount)
        {
          m_delAckEvent.Cancel ();
          m_delAckCount = 0;
          SendEmptyPacket (TcpHeader::ACK|flags);
        }
      else if (m_delAckEvent.IsExpired ())
        {
          m_delAckEvent = Simulator::Schedule (m_delAckTimeout,
                                               &TcpD2::DelAckTimeout, this);
          NS_LOG_LOGIC (this << " scheduled delayed ACK at " << (Simulator::Now () + Simulator::GetDelayLeft (m_delAckEvent)).GetSeconds ());
        }
    }
  // Notify app to receive if necessary
  if (expectedSeq < m_rxBuffer.NextRxSequence ())
    { // NextRxSeq advanced, we have something to send to the app
      if (!m_shutdownRecv)
        {
          NotifyDataRecv ();
        }
      // Handle exceptions
      if (m_closeNotified)
        {
          NS_LOG_WARN ("Why TCP " << this << " got data after close notification?");
        }
      // If we received FIN before and now completed all "holes" in rx buffer,
      // invoke peer close procedure
      if (m_rxBuffer.Finished () && (tcpHeader.GetFlags () & TcpHeader::FIN) == 0)
        {
          DoPeerClose ();
        }
    }
}

/** Retransmit timeout */
void
TcpD2::Retransmit (void)
{
  NS_LOG_FUNCTION (this);
  NS_LOG_LOGIC (this << " ReTxTimeout Expired at time " << Simulator::Now ().GetSeconds ());
  m_inFastRec = false;

  // If erroneous timeout in closed/timed-wait state, just return
  if (m_state == CLOSED || m_state == TIME_WAIT) return;
  // If all data are received (non-closing socket and nothing to send), just return
  if (m_state <= ESTABLISHED && m_txBuffer.HeadSequence () >= m_highTxMark) return;

  // According to RFC2581 sec.3.1, upon RTO, ssthresh is set to half of flight
  // size and cwnd is set to 1*MSS, then the lost packet is retransmitted and
  // TCP back to slow start
  m_ssThresh = std::max (2 * m_segmentSize, BytesInFlight () / 2);
  m_cWnd = m_segmentSize;
  m_nextTxSequence = m_txBuffer.HeadSequence (); // Restart from highest Ack
  NS_LOG_INFO ("RTO. Reset cwnd to " << m_cWnd <<
               ", ssthresh to " << m_ssThresh << ", restart from seqnum " << m_nextTxSequence);
  m_rtt->IncreaseMultiplier ();             // Double the next RTO
  DoRetransmit ();                          // Retransmit the packet
}


void
TcpD2::CancelAllTimers ()
{
  m_ecnmarkEvent.Cancel ();
  TcpSocketBase::CancelAllTimers();
}

void
TcpD2::SetSegSize (uint32_t size)
{
  NS_ABORT_MSG_UNLESS (m_state == CLOSED, "TcpD2::SetSegSize() cannot change segment size after connection started.");
  m_segmentSize = size;
}

void
TcpD2::SetSSThresh (uint32_t threshold)
{
  m_ssThresh = threshold;
}

uint32_t
TcpD2::GetSSThresh (void) const
{
  return m_ssThresh;
}

void
TcpD2::SetInitialCwnd (uint32_t cwnd)
{
  NS_ABORT_MSG_UNLESS (m_state == CLOSED, "TcpD2::SetInitialCwnd() cannot change initial cwnd after connection started.");
  m_initialCWnd = cwnd;
}

uint32_t
TcpD2::GetInitialCwnd (void) const
{
  return m_initialCWnd;
}

void 
TcpD2::InitializeCwnd (void)
{
  /*
   * Initialize congestion window, default to 1 MSS (RFC2001, sec.1) and must
   * not be larger than 2 MSS (RFC2581, sec.3.1). Both m_initiaCWnd and
   * m_segmentSize are set by the attribute system in ns3::TcpSocket.
   */
  m_cWnd = m_initialCWnd * m_segmentSize;
}

} // namespace ns3
