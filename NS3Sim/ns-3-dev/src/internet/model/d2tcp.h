/* -*- Mode:C++; c-file-style:"gnu"; indent-tabs-mode:nil; -*- */
/* DCTCP Ported to NS3
 * 
 * Author: Gautam Kumar <gautamk@outlook.com>
 */

#ifndef D2TCP_H
#define D2TCP_H

#include <cmath>
#include "tcp-socket-base.h"


namespace ns3 {

#define  GAMMA_WEIGHT (1.0/4.0)

/**
 * \ingroup socket
 * \ingroup tcp
 *
 * \brief An implementation of DCTCP
 *
 */
class D2Tcp : public TcpSocketBase
{
public:
  static TypeId GetTypeId (void);
  /**
   * Create an unbound tcp socket.
   */
  D2Tcp (void);
  D2Tcp (const D2Tcp& sock);
  virtual ~D2Tcp (void);

  // From TcpSocketBase
  virtual int Connect (const Address &address);
  virtual int Listen (void);

protected:
  virtual uint32_t Window (void); // Return the max possible number of unacked bytes
  virtual Ptr<TcpSocketBase> Fork (void); // Call CopyObject<TcpNewReno> to clone me
  
  virtual void ConnectionSucceeded (void); // Schedule-friendly wrapper for Socket::NotifyConnectionSucceeded()
  
  
  // Functions that need to be modified due to Ecn prototype
  virtual void DoForwardUp (Ptr<Packet> packet, Ipv4Header header, uint16_t port, 
    Ptr<Ipv4Interface> incomingInterface);
  virtual void SendEmptyPacket (uint8_t flags); // Send a empty packet that carries a flag, e.g. ACK
  virtual void PeerClose (Ptr<Packet>, const TcpHeader&); // Received a FIN from peer, notify rx buffer
  
  
  virtual void ProcessSynSent (Ptr<Packet> packet, const TcpHeader& tcpHeader);
  virtual void ProcessSynRcvd (Ptr<Packet> packet, const TcpHeader& tcpHeader, 
    const Address& fromAddress, const Address& toAddress);
  virtual void ProcessWait (Ptr<Packet>, const TcpHeader&); // Received a packet upon CLOSE_WAIT, FIN_WAIT_1, FIN_WAIT_2
   
  virtual void ProcessEstablished (Ptr<Packet> packet, const TcpHeader& tcpHeader, bool isEcnMarked);
  virtual void ProcessLastAck (Ptr<Packet>, const TcpHeader&); // Received a packet upon LAST_ACK
  virtual void ReceivedData (Ptr<Packet> p, const TcpHeader& tcpHeader, bool isEcnMarked);
  virtual void ReceivedAck (Ptr<Packet> packet, const TcpHeader& tcpHeader, bool isEcnMarked);
  virtual void DelAckTimeout (void);  // Action upon delay ACK timeout, i.e. send an ACK
  // Functions that TcpNewReno also overrides
  void ModifyCWnd(); // D2Tcp logic
  virtual void NewAck (SequenceNumber32 const& seqNum, bool hasEce, uint32_t packetSize); // D2Tcp New ACK Handling, Inc cWnd and call NewAck() of parent
  virtual void DupAck (const TcpHeader& t, uint32_t count);  // Halving cwnd and reset nextTxSequence
  virtual void Retransmit (void); // Exit fast recovery upon retransmit timeout

  // Implementing ns3::TcpSocket -- Attribute get/set
  virtual void     SetSegSize (uint32_t size);
  virtual void     SetSSThresh (uint32_t threshold);
  virtual uint32_t GetSSThresh (void) const;
  virtual void     SetInitialCwnd (uint32_t cwnd);
  virtual uint32_t GetInitialCwnd (void) const;
private:
  void InitializeCwnd (void);            // set m_cWnd when connection starts

protected:
  TracedValue<uint32_t>  m_cWnd;         //< Congestion window
  uint32_t               m_ssThresh;     //< Slow Start Threshold
  uint32_t               m_initialCWnd;  //< Initial cWnd value
  SequenceNumber32       m_recover;      //< Previous highest Tx seqnum for fast recovery
  uint32_t               m_retxThresh;   //< Fast Retransmit threshold
  bool                   m_inFastRec;    //< currently in fast recovery
  bool                   m_limitedTx;    //< perform limited transmit
	uint32_t							 m_dcTcpRecordedSeqNum; //< SeqNum recorded for DCTCP
	double								 m_dcTcpAlpha;			 //< Alpha maintained by DCTCP
	uint32_t						 	 m_dcTcpBytesInterval;
	uint32_t						 	 m_dcTcpBytesIntervalWithEce;
	double								 m_dcTcpG;
	SequenceNumber32			 m_dcTcpTxWindow;
	uint32_t						 	 m_dcTcpSeqRecorded;
  double                 m_gamma;	
	bool                   m_ceLastPacket;     //< CE tag on the last packet seen
  Time                   m_absDeadline;
};

} // namespace ns3

#endif /* D2TCP_H */
