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

#include <fstream>
#include "ns3/core-module.h"
#include "ns3/network-module.h"
#include "ns3/internet-module.h"
#include "ns3/point-to-point-module.h"
#include "ns3/applications-module.h"
#include "ns3/packet.h"


using namespace ns3;

NS_LOG_COMPONENT_DEFINE ("RedTest");

class RedTest : public Application 
{
public:

  RedTest();
  virtual ~RedTest();

  void Setup(Ptr<Socket> socket, Address address, uint32_t packetSize, uint32_t nPackets, DataRate dataRate);
  uint32_t GetPacketsSent();

private:
  virtual void StartApplication(void);
  virtual void StopApplication(void);

  void ScheduleTx(void);
  void SendPacket(void);

  Ptr<Socket>     m_socket;
  Address         m_peer;
  uint32_t        m_packetSize;
  uint32_t        m_nPackets;
  DataRate        m_dataRate;
  EventId         m_sendEvent;
  bool            m_running;
  uint32_t        m_packetsSent;
};

RedTest::RedTest()
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

RedTest::~RedTest()
{
  m_socket = 0;
}

void
RedTest::Setup(Ptr<Socket> socket, Address address, uint32_t packetSize, uint32_t nPackets, DataRate dataRate)
{
  m_socket = socket;
  m_peer = address;
  m_packetSize = packetSize;
  m_nPackets = nPackets;
  m_dataRate = dataRate;
}

void
RedTest::StartApplication(void)
{
  m_running = true;
  m_packetsSent = 0;
  m_socket->Bind ();
  m_socket->Connect (m_peer);
  SendPacket ();
}

void 
RedTest::StopApplication(void)
{
  m_running = false;

  if (m_sendEvent.IsRunning()) {
      Simulator::Cancel(m_sendEvent);
  }

  if (m_socket) {
      m_socket->Close();
  }
}

// Asynchronous callback to send the packet 
void
RedTest::SendPacket(void)
{
  //NS_LOG_INFO("SOMETHING IT IS");
  Ptr<Packet> packet = Create<Packet>(m_packetSize);
  m_socket->Send(packet);
  //if (++m_packetsSent < m_nPackets) {
  ScheduleTx ();
  m_packetsSent += 1;
  //}
}

// Schedule a transmission event.
void 
RedTest::ScheduleTx(void)
{
  if (m_running) {
      Time tNext(Seconds(m_packetSize * 8 / static_cast<double>(m_dataRate.GetBitRate())));
      m_sendEvent = Simulator::Schedule(tNext, &RedTest::SendPacket, this);
  }
}

uint32_t
RedTest::GetPacketsSent()
{
  return m_packetsSent;
}

static void
CwndChange (std::string context, uint32_t oldCwnd, uint32_t newCwnd)
{
  NS_LOG_UNCOND(context << "CwndChange: " << Simulator::Now().GetSeconds() << "\t" << newCwnd);
}


/*static void
RxDrop (Ptr<const Packet> p)
{
  NS_LOG_UNCOND ("RxDrop at " << Simulator::Now ().GetSeconds ());
}*/

int 
main (int argc, char *argv[])
{
  NS_LOG_INFO("Creating nodes.");
  NodeContainer nodes;
  nodes.Create(2);

  Config::SetDefault("ns3::TcpL4Protocol::SocketType", StringValue("ns3::TcpNewReno"));
  Config::SetDefault("ns3::TcpSocket::SegmentSize", UintegerValue(1400 - 42)); // <G> 42 = Header size.

  //Ptr<RateErrorModel> em = CreateObject<RateErrorModel>();
  //em->SetAttribute("ErrorRate", DoubleValue(0.00001));
  //devices.Get (1)->SetAttribute ("ReceiveErrorModel", PointerValue (em));

  // Install network stacks on the nodes
  InternetStackHelper stack;
  stack.Install(nodes);
  Packet::EnablePrinting();
  // Collect an adjacency list of the nodes 
  std::vector<NodeContainer> nodeAdjacencyList(1);
  nodeAdjacencyList[0] = NodeContainer(nodes.Get(0), nodes.Get(1));

  // Create the channels without any IP addressing information
  NS_LOG_INFO("Create channels.");
  PointToPointHelper pointToPoint;
  pointToPoint.SetDeviceAttribute("DataRate", StringValue("128Mbps"));
  pointToPoint.SetChannelAttribute("Delay", StringValue("0.250ms"));
  pointToPoint.SetDeviceAttribute("Mtu", UintegerValue(1500));
  //pointToPoint.SetQueue("ns3::DropTailQueueNotifier", "MaxBytes", UintegerValue(50000));
  pointToPoint.SetQueue("ns3::EcnQueue", "MaxBytes", UintegerValue(50000));
  std::vector<NetDeviceContainer> deviceAdjacencyList(1);
  for (uint32_t i = 0; i < deviceAdjacencyList.size(); i++) {
    deviceAdjacencyList[i] = pointToPoint.Install(nodeAdjacencyList[i]);
  }
  // Later, we add IP Addresses.
  NS_LOG_INFO("Assign IP Addresses. ");
  Ipv4AddressHelper ipv4;
  std::vector<Ipv4InterfaceContainer> interfaceAdjacencyList(1);
  for (uint32_t i = 0; i < interfaceAdjacencyList.size(); i++) {
    std::ostringstream subnet;
    subnet << "10.1." << i + 1 << ".0";
    ipv4.SetBase(subnet.str().c_str(), "255.255.255.0");
    interfaceAdjacencyList[i] = ipv4.Assign(deviceAdjacencyList[i]);
  }
  
  // Turn on global static routing
  Ipv4GlobalRoutingHelper::PopulateRoutingTables();

  PacketSinkHelper packetSinkHelper0("ns3::TcpSocketFactory", InetSocketAddress(Ipv4Address::GetAny(), 8080));
  ApplicationContainer sinkApps0 = packetSinkHelper0.Install(nodes.Get(1));
  sinkApps0.Start(Seconds(0.));
  sinkApps0.Stop(Seconds(4.));

  Ptr<RedTest> app0 = CreateObject<RedTest>();
  Ptr<Socket> ns3TcpSocket0 = Socket::CreateSocket(nodes.Get(0), TcpSocketFactory::GetTypeId());
  ns3TcpSocket0->TraceConnect("CongestionWindow", "Socket0: ", MakeCallback(&CwndChange));
  Address sinkAddress0(InetSocketAddress(interfaceAdjacencyList[0].GetAddress(1), 8080));
  app0->Setup(ns3TcpSocket0, sinkAddress0, 1040, 1000, DataRate("1024Mbps"));
  nodes.Get(0)->AddApplication(app0);
  app0->SetStartTime(Seconds(1.));
  app0->SetStopTime(Seconds(4.));


  Simulator::Stop(Seconds(5));
  Simulator::Run();

  NS_LOG_UNCOND("Packets Sent by app0: " << app0->GetPacketsSent());
  Simulator::Destroy();

  return 0;
}

