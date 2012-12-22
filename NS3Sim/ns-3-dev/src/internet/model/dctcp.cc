


/* -*- Mode:C++; c-file-style:"gnu"; indent-tabs-mode:nil; -*- */
/*
 * 
 * Author: Gautam Kumar <gautamk@outlook.com>
 */

#define NS_LOG_APPEND_CONTEXT \
  if (m_node) { std::clog << Simulator::Now ().GetSeconds () << " [node " << m_node->GetId () << "] "; }

#include "dctcp.h"
#include "ns3/log.h"
#include "ns3/trace-source-accessor.h"
#include "ns3/simulator.h"
#include "ns3/abort.h"
#include "ns3/node.h"

NS_LOG_COMPONENT_DEFINE ("DcTcp");

namespace ns3 {

NS_OBJECT_ENSURE_REGISTERED (DcTcp);

TypeId
DcTcp::GetTypeId (void)
{
  static TypeId tid = TypeId ("ns3::DcTcp")
    .SetParent<TcpSocketBase> ()
    .AddConstructor<DcTcp> ()
    .AddAttribute ("ReTxThreshold", "Threshold for fast retransmit",
                    UintegerValue (3),
                    MakeUintegerAccessor (&DcTcp::m_retxThresh),
                    MakeUintegerChecker<uint32_t> ())
    .AddAttribute ("LimitedTransmit", "Enable limited transmit",
		    BooleanValue (false),
		    MakeBooleanAccessor (&DcTcp::m_limitedTx),
		    MakeBooleanChecker ())
    .AddTraceSource ("CongestionWindow",
                     "The TCP connection's congestion window",
                     MakeTraceSourceAccessor (&DcTcp::m_cWnd))
  ;
  return tid;
}

DcTcp::DcTcp (void)
  : m_retxThresh (3), // mute valgrind, actual value set by the attribute system
    m_inFastRec (false),
    m_limitedTx (false), // mute valgrind, actual value set by the attribute system
		m_dcTcpRecordedSeqNum (0),
		m_dcTcpAlpha (1.0),
		m_dcTcpBytesInterval (0),
		m_dcTcpBytesIntervalWithEce (0),
		m_dcTcpG (1.0 / 16.0),
		m_dcTcpTxWindow (0),
		m_dcTcpSeqRecorded (0)
{
  NS_LOG_FUNCTION (this);
}

DcTcp::DcTcp (const DcTcp& sock)
  : TcpSocketBase (sock),
    m_cWnd (sock.m_cWnd),
    m_ssThresh (sock.m_ssThresh),
    m_initialCWnd (sock.m_initialCWnd),
    m_retxThresh (sock.m_retxThresh),
    m_inFastRec (false),
    m_limitedTx (sock.m_limitedTx)

{
  NS_LOG_FUNCTION (this);
  NS_LOG_LOGIC ("Invoked the copy constructor");
}

DcTcp::~DcTcp (void)
{
}

/** We initialize m_cWnd from this function, after attributes initialized */
int
DcTcp::Listen (void)
{
  NS_LOG_FUNCTION (this);
  InitializeCwnd ();
  return TcpSocketBase::Listen ();
}

/** We initialize m_cWnd from this function, after attributes initialized */
int
DcTcp::Connect (const Address & address)
{
  NS_LOG_FUNCTION (this << address);
  InitializeCwnd ();
  return TcpSocketBase::Connect (address);
}

/** Limit the size of in-flight data by cwnd and receiver's rxwin */
uint32_t
DcTcp::Window (void)
{
  NS_LOG_FUNCTION (this);
  return std::min (m_rWnd.Get (), m_cWnd.Get ());
}

Ptr<TcpSocketBase>
DcTcp::Fork (void)
{
  return CopyObject<DcTcp> (this);
}


/** New ACK (up to seqnum seq) received. Increase cwnd and call TcpSocketBase::NewAck() */
void
DcTcp::NewAck(const SequenceNumber32& seq, bool hasEce, uint32_t packetSize)
{
	//uint32_t seq = (uint32_t) seqNum.GetValue();
	NS_LOG_INFO ("New ACK DCTCP with alpha" << m_dcTcpAlpha);
  NS_LOG_FUNCTION (this << seq);
  NS_LOG_LOGIC ("TcpNewReno receieved ACK for seq " << seq <<
                " cwnd " << m_cWnd <<
                " ssthresh " << m_ssThresh);
	// The DCTCP logic is only based on the ACKS without any data content
	if (packetSize <= 0) { // There is no data in this ACK
		uint32_t bytesInfo = seq.GetValue() - m_dcTcpSeqRecorded;
		m_dcTcpSeqRecorded = seq.GetValue();
		m_dcTcpBytesInterval += bytesInfo;
		if (hasEce) {
			m_dcTcpBytesIntervalWithEce += bytesInfo;
		}
		
		// Process the change for a full window
		if (seq >= m_dcTcpTxWindow) {
			m_dcTcpTxWindow = m_highTxMark;
			// Update alpha
			m_dcTcpAlpha = (1 - m_dcTcpG) * m_dcTcpAlpha 
				+ (m_dcTcpG * m_dcTcpBytesIntervalWithEce) / m_dcTcpBytesInterval;
			m_dcTcpBytesInterval = 0;
			m_dcTcpBytesIntervalWithEce = 0;
		}
		
		// Decrease the congestion window for a marked ACK
		if (hasEce) {
			NS_LOG_INFO("Decreasing Congestion Window with m_dcTcpAlpha" << m_dcTcpAlpha);
			m_cWnd = m_cWnd * (1 - m_dcTcpAlpha / 2.0);
		}
	}
	
	
  // Check for exit condition of fast recovery
  if (m_inFastRec && seq < m_recover)
    { // Partial ACK, partial window deflation (RFC2582 sec.3 bullet #5 paragraph 3)
      m_cWnd -= seq - m_txBuffer.HeadSequence ();
      m_cWnd += m_segmentSize;  // increase cwnd
      NS_LOG_INFO ("Partial ACK in fast recovery: cwnd set to " << m_cWnd);
      TcpSocketBase::NewAck (seq, hasEce, packetSize); // update m_nextTxSequence and send new data if allowed by window
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
      NS_LOG_INFO ("In CongAvoid, Previous cwnd " << m_cWnd);
      m_cWnd += static_cast<uint32_t> (adder);
      NS_LOG_INFO ("In CongAvoid, updated to cwnd " << m_cWnd << " ssthresh " << m_ssThresh);
    }

  // Complete newAck processing
  TcpSocketBase::NewAck (seq, hasEce, packetSize);
}

/** Cut cwnd and enter fast recovery mode upon triple dupack */
void
DcTcp::DupAck (const TcpHeader& t, uint32_t count)
{
  NS_LOG_FUNCTION (this << count);
  if (count == m_retxThresh && !m_inFastRec)
    { // triple duplicate ack triggers fast retransmit (RFC2582 sec.3 bullet #1)
      m_ssThresh = std::max (2 * m_segmentSize, BytesInFlight () / 2);
      //m_cWnd = m_ssThresh + 3 * m_segmentSize;
			m_cWnd = m_cWnd * (1 - m_dcTcpAlpha / 2.0);
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

/** Retransmit timeout */
void
DcTcp::Retransmit (void)
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
DcTcp::SetSegSize (uint32_t size)
{
  NS_ABORT_MSG_UNLESS (m_state == CLOSED, "DcTcp::SetSegSize() cannot change segment size after connection started.");
  m_segmentSize = size;
}

void
DcTcp::SetSSThresh (uint32_t threshold)
{
  m_ssThresh = threshold;
}

uint32_t
DcTcp::GetSSThresh (void) const
{
  return m_ssThresh;
}

void
DcTcp::SetInitialCwnd (uint32_t cwnd)
{
  NS_ABORT_MSG_UNLESS (m_state == CLOSED, "TcpNewReno::SetInitialCwnd() cannot change initial cwnd after connection started.");
  m_initialCWnd = cwnd;
}

uint32_t
DcTcp::GetInitialCwnd (void) const
{
  return m_initialCWnd;
}

void 
DcTcp::InitializeCwnd (void)
{
  /*
   * Initialize congestion window, default to 1 MSS (RFC2001, sec.1) and must
   * not be larger than 2 MSS (RFC2581, sec.3.1). Both m_initiaCWnd and
   * m_segmentSize are set by the attribute system in ns3::TcpSocket.
   */
  m_cWnd = m_initialCWnd * m_segmentSize;
}

} // namespace ns3
