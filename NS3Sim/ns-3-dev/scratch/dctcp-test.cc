/* -*- Mode:C++; c-file-style:"gnu"; indent-tabs-mode:nil; -*- */
/*
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

/*Topology:
                   n5---n6--n7---n8---n10
                    |             |
           |------ n2-----|       |
 n0---n1---|              |-------n4--n9
           |------ n3-----|
*/
#include <iostream>
#include "ns3/header.h"
#include "ns3/ptr.h"
#include "ns3/log.h"
#include "ns3/core-module.h"
#include "ns3/network-module.h"
#include "ns3/internet-module.h"
#include "ns3/point-to-point-module.h"
#include "ns3/applications-module.h"
#include "ns3/ipv4-global-routing-helper.h"
#include "ns3/flow-monitor-helper.h"
#include "ns3/priority-queue.h"
#include "ns3/point-to-point-layout-module.h"
#include "ns3/seq-ts-header.h"


#define LINKS 13
#define NODES 12
#define MAXPACKETS 100
#define MAXBYTES 1024
#define BYTES 1024000000
#define PKTCOUNT 1

//#define SINGLE1 false
#define SINGLE2 false
#define SINGLE1 true
//#define SINGLE2 true


using namespace ns3;
using namespace std;


NS_LOG_COMPONENT_DEFINE ("TCPTopology");




class Sender : public Application
{
public:
    Sender();
    virtual ~Sender();
    
    void Setup(Ptr<Socket> socket, Address address, uint32_t packetSize);
    
private:
    
    virtual void StartApplication(void);
    virtual void StopApplication(void);
    
    void SendPacket(void);
    uint32_t GetPacketsSent();
    
    Ptr<Socket> m_socket;
    Address m_peer;
    uint32_t m_packetSize;
    uint32_t m_nPackets;
    DataRate m_dataRate;
    EventId m_sendEvent;
    bool m_running;
    uint32_t m_packetsSent;
};

Sender::Sender()
: m_socket(0),
m_peer(),
m_packetSize(0),
m_nPackets(0),
m_dataRate(0),
m_sendEvent(),
m_running(false),
m_packetsSent(0)
{
}

Sender::~Sender()
{
    m_socket=0;
}

void
Sender::Setup(Ptr<Socket> socket, Address address, uint32_t packetSize)
{
    m_socket = socket;
    m_peer = address;
    m_packetSize = packetSize;
}

void
Sender::StartApplication(void)
{
    m_running = true;
    m_packetsSent = 0;
    m_socket->Bind ();
    m_socket->Connect (m_peer);
    SendPacket ();
}

void
Sender::StopApplication(void)
{
    m_running = false;
    if (m_sendEvent.IsRunning())
    {
        Simulator::Cancel(m_sendEvent);
    }
    
    if (m_socket) {
        m_socket->Close();
    }
}

// Asynchronous callback to send the packet
void
Sender::SendPacket(void)
{
    uint8_t * buf = (uint8_t *) malloc(m_packetSize);
    int i;

    for(i=0;i<(int)m_packetSize;i++)
      buf[i] = 0;

   if(m_socket->Send(buf, m_packetSize, 0)==-1)
    {
      std::cout<<"\n\nError in sending\n\n";
    }
}


uint32_t
Sender::GetPacketsSent()
{
    return m_packetsSent;
}

class TcpReceiver : public Application
{
public:
    TcpReceiver();
    virtual ~TcpReceiver();
    uint32_t GetTotalRx () const;
    void Setup(Address address, uint32_t packetSize, ofstream* ofs);
    Ptr<Socket> GetListeningSocket (void) const;
    std::list<Ptr<Socket> > GetAcceptedSockets (void) const;
    
protected:
    virtual void DoDispose (void);
    
private:
    // inherited from Application base class.
    virtual void StartApplication (void);    // Called at time specified by Start
    virtual void StopApplication (void);     // Called at time specified by Stop
    
    void HandleRead (Ptr<Socket>);
    void HandleAccept (Ptr<Socket>, const Address& from);
    void HandlePeerClose (Ptr<Socket>);
    void HandlePeerError (Ptr<Socket>);
    
    // In the case of TCP, each socket accept returns a new socket, so the
    // listening socket is stored seperately from the accepted sockets
    Ptr<Socket>     m_socket;       // Listening socket
    std::list<Ptr<Socket> > m_socketList; //the accepted sockets
    
    Address         m_local;        // Local address to bind to
    uint32_t        m_totalRx;      // Total bytes received
    uint32_t        m_packetSize;
    ofstream*        m_ofs;
    TracedCallback<Ptr<const Packet>, const Address &> m_rxTrace;
    
};

TcpReceiver::TcpReceiver ()
{
    NS_LOG_FUNCTION (this);
    m_socket = 0;
    m_totalRx = 0;
}

TcpReceiver::~TcpReceiver()
{
    NS_LOG_FUNCTION (this);
}


void TcpReceiver::Setup(Address address, uint32_t packetSize, ofstream* ofs)
{
    m_local=address;
    m_packetSize = packetSize;
    m_ofs = ofs;

}
uint32_t TcpReceiver::GetTotalRx () const
{
    return m_totalRx;
}

Ptr<Socket>
TcpReceiver::GetListeningSocket (void) const
{
    NS_LOG_FUNCTION (this);
    return m_socket;
}

std::list<Ptr<Socket> >
TcpReceiver::GetAcceptedSockets (void) const
{
    NS_LOG_FUNCTION (this);
    return m_socketList;
}

void TcpReceiver::DoDispose (void)
{
    NS_LOG_FUNCTION (this);
    m_socket = 0;
    m_socketList.clear ();
    
    // chain up
    Application::DoDispose ();
}


// Application Methods
void TcpReceiver::StartApplication ()    // Called at time specified by Start
{
    NS_LOG_FUNCTION (this);
    // Create the socket if not already
    if (!m_socket)
    {
        m_socket = Socket::CreateSocket (GetNode (), TcpSocketFactory::GetTypeId());
        m_socket->Bind (m_local);
        m_socket->Listen ();
    }
    
    m_socket->SetRecvCallback (MakeCallback (&TcpReceiver::HandleRead, this));
    m_socket->SetAcceptCallback (
                                 MakeNullCallback<bool, Ptr<Socket>, const Address &> (),
                                 MakeCallback (&TcpReceiver::HandleAccept, this));
    m_socket->SetCloseCallbacks (
                                 MakeCallback (&TcpReceiver::HandlePeerClose, this),
                                 MakeCallback (&TcpReceiver::HandlePeerError, this));
}

void TcpReceiver::StopApplication ()     // Called at time specified by Stop
{
    NS_LOG_FUNCTION (this);
    while(!m_socketList.empty ()) //these are accepted sockets, close them
    {
        Ptr<Socket> acceptedSocket = m_socketList.front ();
        m_socketList.pop_front ();
        acceptedSocket->Close ();
    }
    if (m_socket)
    {
        m_socket->Close ();
        m_socket->SetRecvCallback (MakeNullCallback<void, Ptr<Socket> > ());
    }
}

void TcpReceiver::HandleRead (Ptr<Socket> socket)
{
    NS_LOG_FUNCTION (this << socket);
    Ptr<Packet> packet;
    Address from;
    while (packet = socket->RecvFrom(from))
    {
        if (packet->GetSize () == 0)
        { //EOF
            break;
        }
        m_totalRx += packet->GetSize ();
              if (InetSocketAddress::IsMatchingType (from))
        {
            NS_LOG_INFO ("At time " << Simulator::Now ().GetSeconds ()
                         << "s packet sink received "
                         <<  packet->GetSize () << " bytes from "
                         << InetSocketAddress::ConvertFrom(from).GetIpv4 ()
                         << " port " << InetSocketAddress::ConvertFrom (from).GetPort ()
                         << " total Rx " << m_totalRx << " bytes");
        }
        else if (Inet6SocketAddress::IsMatchingType (from))
        {
            NS_LOG_INFO ("At time " << Simulator::Now ().GetSeconds ()
                         << "s packet sink received "
                         <<  packet->GetSize () << " bytes from "
                         << Inet6SocketAddress::ConvertFrom(from).GetIpv6 ()
                         << " port " << Inet6SocketAddress::ConvertFrom (from).GetPort ()
                         << " total Rx " << m_totalRx << " bytes");
        }
        if(m_totalRx >= m_packetSize)
        {
            std::cout<<"\nReceived all data ("<<m_totalRx<<") at:"<<Simulator::Now().GetSeconds()<<" from "<<InetSocketAddress::ConvertFrom(from).GetIpv4 ()<<"\n";
            (*m_ofs)<<Simulator::Now().GetSeconds() - 1.0<<"\n";
        }
        
        if(m_totalRx > m_packetSize)
        {
            std::cout<<"\n\nAlso Received extra\n\n";
        }
        
    }
}


void TcpReceiver::HandlePeerClose (Ptr<Socket> socket)
{
    NS_LOG_FUNCTION (this << socket);
}

void TcpReceiver::HandlePeerError (Ptr<Socket> socket)
{
    NS_LOG_FUNCTION (this << socket);
}


void TcpReceiver::HandleAccept (Ptr<Socket> s, const Address& from)
{
    NS_LOG_FUNCTION (this << s << from);
    s->SetRecvCallback (MakeCallback (&TcpReceiver::HandleRead, this));
    m_socketList.push_back (s);
}


static void
QueueOccupancyChange(uint32_t oldQOcc, uint32_t newQOcc) 
{
  NS_LOG_UNCOND("QueueChange: " << Simulator::Now().GetSeconds() << "\t" << newQOcc);
}


/*
static void
CwndChange (std::string context, uint32_t oldCwnd, uint32_t newCwnd)
{
    NS_LOG_UNCOND(context << "CwndChange: " << Simulator::Now().GetSeconds() << "\t" << newCwnd);
}

static void
SSThreshChange (std::string context, uint32_t oldSSThresh, uint32_t newSSThresh) 
{
  NS_LOG_UNCOND(context << "SSThresh: " << Simulator::Now().GetSeconds() << "\t" << newSSThresh);
}
*/

int main (int argc, char *argv[])
{
  int i;
  char addr[20];
  uint32_t bytes = 102400000;

  LogComponentEnable("TcpSocketBase", LOG_LEVEL_WARN);
  LogComponentEnable("TcpNewReno", LOG_LEVEL_WARN);
  LogComponentEnable("DcTcp", LOG_LEVEL_WARN);
  LogComponentEnable("D2Tcp", LOG_LEVEL_DEBUG);
  Config::SetDefault("ns3::TcpL4Protocol::SocketType", StringValue("ns3::DcTcp"));
  Config::SetDefault("ns3::TcpSocket::SegmentSize", UintegerValue(1400 - 42)); // <G> 42 = Header Size IP 
  
  PointToPointHelper pointToPoint;
  //pointToPoint.SetQueue("ns3::DropTailQueueNotifier", "MaxBytes", UintegerValue(700000));
  pointToPoint.SetQueue("ns3::EcnQueue", "MaxBytes", UintegerValue(700000), "EcnThreshold", DoubleValue(0.05));
  pointToPoint.SetDeviceAttribute ("DataRate", StringValue ("1024Mbps"));
  pointToPoint.SetChannelAttribute ("Delay", StringValue (".050ms"));
  pointToPoint.SetDeviceAttribute("Mtu", UintegerValue(1500));

  NodeContainer nodes;
  nodes.Create(4);
  
  //building topology
  NodeContainer p2p[3];

  p2p[0].Add(nodes.Get(0));
  p2p[0].Add(nodes.Get(2));

  p2p[1].Add(nodes.Get(1));
  p2p[1].Add(nodes.Get(2));

  p2p[2].Add(nodes.Get(2));
  p2p[2].Add(nodes.Get(3));

  NetDeviceContainer devices[3];
  for(i = 0;i < 3; i++) {
    devices[i] = pointToPoint.Install (p2p[i]);
  }
  Ptr<PointToPointNetDevice> interestedNetDevice;
  interestedNetDevice = StaticCast<PointToPointNetDevice>(devices[2].Get(0));
  interestedNetDevice->GetQueue()->TraceConnectWithoutContext("QueueOccupancy", MakeCallback(&QueueOccupancyChange));
  
  InternetStackHelper stack;
  stack.Install(nodes);

  Ipv4AddressHelper address[3];
  for(i = 0; i < 3; i++)
  {
    sprintf(addr, "10.1.%d.0", i);
    address[i].SetBase(addr, "255.255.255.0");
  }

  Ipv4InterfaceContainer interfaces[3];
  for(i = 0; i < 3; i++) {
    interfaces[i] = address[i].Assign (devices[i]);
  }

  Ipv4GlobalRoutingHelper::PopulateRoutingTables();

  ofstream recvfile1("recv1.txt", ios::app);
  ofstream recvfile2("recv2.txt", ios::app);
 

  Address sinkAddress0(InetSocketAddress(interfaces[2].GetAddress(1), 8080));
  Ptr<Sender> sendapp = CreateObject<Sender>();
  Ptr<Socket> sock = Socket::CreateSocket(nodes.Get(0), TcpSocketFactory::GetTypeId());
  sock->SetDeadline(1500);
  //sock->TraceConnect("CongestionWindow", "Socket0: ", MakeCallback(&CwndChange));
  //sock->TraceConnect("SSThresh", "Socket0: ", MakeCallback(&SSThreshChange));
  //uint32_t bytes = 102400000;
  sendapp->Setup(sock, sinkAddress0, bytes);
  nodes.Get(0)->AddApplication(sendapp);
  sendapp->SetStartTime(Seconds(1.));
  sendapp->SetStopTime(Seconds(10000.));

  Ptr<TcpReceiver> recvapp = CreateObject<TcpReceiver>();
  recvapp->Setup(sinkAddress0, bytes, &recvfile1);
  nodes.Get(3)->AddApplication(recvapp);
  recvapp->SetStartTime(Seconds(0.));
  recvapp->SetStopTime(Seconds(10000.));

  Address sinkAddress1(InetSocketAddress(interfaces[2].GetAddress(1), 8081));
  Ptr<Sender> sendapp1 = CreateObject<Sender>();
  Ptr<Socket> sock1 = Socket::CreateSocket(nodes.Get(1), TcpSocketFactory::GetTypeId());
  sock1->SetDeadline(10000);
  //sock1->TraceConnect("CongestionWindow", "Socket1: ", MakeCallback(&CwndChange));
  sendapp1->Setup(sock1, sinkAddress1, bytes);
  nodes.Get(1)->AddApplication(sendapp1);
  sendapp1->SetStartTime(Seconds(1.));
  sendapp1->SetStopTime(Seconds(10000.));

  Ptr<TcpReceiver> recvapp1 = CreateObject<TcpReceiver>();
  recvapp1->Setup(sinkAddress1, bytes, &recvfile2);
  nodes.Get(3)->AddApplication(recvapp1);
  recvapp1->SetStartTime(Seconds(0.));
  recvapp1->SetStopTime(Seconds(10000.));
    
  // Flow Monitor
  Ptr<FlowMonitor> flowmon;
  FlowMonitorHelper flowmonHelper;
  flowmon = flowmonHelper.InstallAll ();

  AsciiTraceHelper ascii;
  pointToPoint.EnableAsciiAll(ascii.CreateFileStream("tcptopo.tr"));
  pointToPoint.EnablePcapAll("tcptopo");
  Simulator::Stop(Seconds(10000));
  Simulator::Run ();
  flowmon->SerializeToXmlFile ("tcptopo.flowmon", false, false);
  Simulator::Destroy ();
  return 0;
}
