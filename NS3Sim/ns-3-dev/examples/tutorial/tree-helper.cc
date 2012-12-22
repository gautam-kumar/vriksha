/*
 * Author: Gautam Kumar <gautamk@outlook.com>
 */

#include <sstream>
#include <stdint.h>
#include <stdlib.h>

#include "hose-tree-helper.h"

#include "ns3/log.h"
#include "ns3/internet-stack-helper.h"
#include "ns3/ipv4-address-helper.h"
#include "ns3/queue.h"
#include "ns3/cp-net-device.h"
#include "ns3/rp-net-device.h"
#include "ns3/uinteger.h"
#include "ns3/point-to-point-channel.h"
#include "ns3/ipv4-address-generator.h"
#include "ns3/ipv4-hash-routing-helper.h"
#include "ns3/config.h"
#include "ns3/abort.h"

namespace ns3 {
NS_LOG_COMPONENT_DEFINE("HoseTreeHelper");
NS_OBJECT_ENSURE_REGISTERED(HoseTreeHelper);

unsigned TreeHelper::numHostNodes = 0;

TypeId
HoseTreeHelper::GetTypeId(void) 
{
	// TODO: Copy from FatTreeHelper
}

HoseTreeHelper::HoseTreeHelper(unsigned N)
{
	numHostNodes = N;
	channelFactory.SetTypeId("ns3::PointToPointChannel");
	rpFactory.SetTypeId("ns3::RpNetDevice");
	cpFactory.SetTypeId("ns3::CpNetDevice");
}

HoseTreeHelper::~HoseTreeHelper()
{
}

// Create the whole topology
void
HoseTreeHelper::Create()
{
	const unsigned numTotal = numHostNodes + 1; // Add 1 for the Hose switch. 
	allNodes.Create(numTotal);
	switchNodes.Add(allNodes.Get(0); // 0 is the id for the switch.
	for (int i = 1; i < numTotal; i++) {
		hostNodes.Add(allNodes.Get(i));
	}

	NS_LOG_LOGIC("Creating connections and set-u addresses.")
	Ipv4HashRoutingHelper hashHelper;
	InternetStackHelper internet;
	internet.SetRoutingHelper(hashHelper);
	internet.Install(allNodes);

	rpFactory.Set("DataRate", DataRateValue(linkRate));
	cpFactory.Set("DataRate", DataRateValue(linkRate));
	channelFactory.Set("Delay", TimeValue(linkDelay));
	for (int j = 0; j < numTotal; j++) {
		Ptr<Node> switchNode = switchNodes.Get(0);
		// Connect the hostNode to the switch.
		Ptr<Node> h = hostNodes.Get(j);
		NetDeviceContainer devices = InstalCpRp(switchNode, h);
		// Set routing for end host: Default route only
		Ptr<HashRouting> hr = hashHelper.GetHashRouting(
			hNode->GetObject<Ipv4>());
		hr->AddRoute(Ipv4Address(0U), Ipv4Mask(0U), 1);
		// Set IP address for end host
		uint32_t address = 0; //TODO: Fix
		AssignIp(devices.Get(1), address, hostIface);
		// Set routing for edge switch
		hr = hashHelper.GetHashRouting(switchNode->GetObject<Ipv4>());
		hr->AddRoute(Ipv4Address(address), Ipv4Mask(0xFFFFFFFU), 0); // TODO: See how this is working
		// Set IP address for edge switch
		address = 0;
		AssignIp(devices.Get(0), address, edgeIface);
	}
} // HoseTreeHelper::Create()


// Assign IPs to nodes
void
HoseTreeHelper::AssignIp(Ptr<NetDevice> c, uint32_t address, 
		Ipv4InterfaceContainer &con)
{
	NS_LOG_FUNCTION_NOARGS(); // TODO: What does this mean?
	Ptr<Node> node = c->GetNode();
	NS_ASSERT_MSG(node, "HoseTreeHelper::AssignIp(): Bad node");
	Ptr<Ipv4> ipv4 = node->GetObject<Ipv4>();
	NS_ASSERT_MSG(ipv4, "HoseTreeHelper::AssignIp(): Bad ipv4");

	int32_t ifIndex = ipv4->GetInterfaceForDevice(c);
	if (ifIndex == -1) {
		ifIndex = ipv4->AddInterface(c);
	}
	NS_ASSERT_MSG(ifIndex >= 0, "HoseTreeHelper::AssignIp(): Interface index 
			not found");

	Ipv4Address addr(address);
	Ipv4InterfaceAddress ifAddr(addr, 0xFFFFFFFF);
	ipv4->AddAddress(ifIndex, ifAddr);
	ipv4->SetMetric(ifIndex, 1);
	ipv4->SetUp(ifIndex);
	con.Add(ipv4, ifIndex);
	Ipv4AddressGenerator::AddAllocated(addr);
}

// Create nodes and connections
NetDeviceContainer
HoseTreeHelper::InstallCpRp(Ptr<Node> a, Ptr<Node> b)
{
	NetDeviceContainer container;
	Ptr<CpNetDevice> devA = cpFactory.Create<CpNetDevice>();
	devA->SetAddress(Mac48Address::Allocate());
	a->AddDevice(devA);
	Ptr<RpNetDevice> devB = rpFactory.Create<RpNetDevice>();
	devB->SetAddress(Mac48Address::Allocate());
	b->AddDevice(devB);
	Ptr<PointToPointChannel> channel = channelFactory.Create<PointToPointChannel>();
	devA->Attach(channel);
	devB->Attach(channel);
	container.Add(devA);
	container.Add(devB);
	return container;
}

NetDeviceContainer
HoseTreeHelper:InstallCpCp(Ptr<Node> a, Ptr<Node> b)
{
}



}

			
