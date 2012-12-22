/* -*- Mode:C++; c-file-style:"gnu"; indent-tabs-mode:nil; -*- */
/* DCTCP Ported to NS3
 * 
 * Author: Gautam Kumar <gautamk@outlook.com>
 */

#ifndef DCTCP_H
#define DCTCP_H

#include "tcp-socket-base.h"

namespace ns3 {

/**
 * \ingroup socket
 * \ingroup tcp
 *
 * \brief An implementation of DCTCP
 *
 */
class DcTcp : public TcpSocketBase
{
public:
  static TypeId GetTypeId (void);
  /**
   * Create an unbound tcp socket.
   */
  DcTcp (void);
  DcTcp (const DcTcp& sock);
  virtual ~DcTcp (void);

  // From TcpSocketBase
  virtual int Connect (const Address &address);
  virtual int Listen (void);

protected:
  virtual uint32_t Window (void); // Return the max possible number of unacked bytes
  virtual Ptr<TcpSocketBase> Fork (void); // Call CopyObject<TcpNewReno> to clone me
  virtual void NewAck (SequenceNumber32 const& seqNum, bool hasEce, uint32_t packetSize); // DcTcp New ACK Handling, Inc cWnd and call NewAck() of parent
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
};

} // namespace ns3

#endif /* DCTCP_H */