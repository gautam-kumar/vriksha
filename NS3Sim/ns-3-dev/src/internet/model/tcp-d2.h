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

#ifndef TCP_D2_H
#define TCP_D2_H

#include <cmath>
#include "tcp-socket-base.h"

namespace ns3 {


#define  ALPHA_WEIGHT (1.0/16.0)
#define  GAMMA_WEIGHT (1.0/4.0)
  

/**
 * \ingroup socket
 * \ingroup tcp
 *
 * \brief An implementation of a stream socket using TCP.
 *
 * This class contains the D2 implementation of TCP, as of RFC2582.
 */
class TcpD2 : public TcpSocketBase
{
public:
  static TypeId GetTypeId (void);
  /**
   * Create an unbound tcp socket.
   */
  TcpD2 (void);
  TcpD2 (const TcpD2& sock);
  virtual ~TcpD2 (void);

  void NewAck (SequenceNumber32 const& seq, bool hasEcn); // Inc cwnd and call NewAck() of parent
  void SetAlpha (Time deadline,Time RTT,uint32_t bytes); //Balajee
  void ModifyCwnd ();

  // From TcpSocketBase
  virtual int Connect (const Address &address);
  virtual int Listen (void);

protected:
  virtual uint32_t Window (void); // Return the max possible number of unacked bytes
  virtual Ptr<TcpSocketBase> Fork (void); // Call CopyObject<TcpD2> to clone me
  virtual void DoForwardUp (Ptr<Packet> packet, Ipv4Header header, uint16_t port, Ptr<Ipv4Interface> incomingInterface);
  virtual void ProcessEstablished (Ptr<Packet> packet, const TcpHeader& tcpHeader);
  virtual void ReceivedAck (Ptr<Packet> packet, const TcpHeader& tcpHeader);
  virtual void ProcessSynSent (Ptr<Packet> packet, const TcpHeader& tcpHeader);
  virtual void ProcessSynRcvd (Ptr<Packet> packet, const TcpHeader& tcpHeader, const Address& fromAddress, const Address& toAddress);
  virtual void ReceivedData (Ptr<Packet> p, const TcpHeader& tcpHeader);

  virtual void DupAck (const TcpHeader& t, uint32_t count);  // Halving cwnd and reset nextTxSequence
  virtual void Retransmit (void); // Exit fast recovery upon retransmit timeout

  virtual void CancelAllTimers (void);

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

  bool                   m_first_ack_on_established;

  // DCTCP stuff
  uint32_t               m_tot_acks;
  uint32_t               m_ecn_acks;
  double                 m_alpha_last;
  double                 m_alpha;
  
  // DATCP stuff
  double                 m_gamma_last;
  double                 m_gamma;

  Time                   m_absDeadline;

  EventId                m_ecnmarkEvent;    // when to update the ECN marking percentage
};

} // namespace ns3

#endif /* TCP_D2_H */
