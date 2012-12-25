#include "ns3/ecn-tag.h"

namespace ns3 {
  
  NS_OBJECT_ENSURE_REGISTERED (EcnTag);

  TypeId
  EcnTag::GetTypeId (void) {
    static TypeId tid = TypeId ("ns3::EcnTag")
      .SetParent<Tag> ()
      .AddConstructor<EcnTag>()
      ;
    return tid;
  }

  TypeId
  EcnTag::GetInstanceTypeId (void) const {
    return GetTypeId();
  }

  EcnTag::EcnTag() {
    m_ecn = 0;
  }

  void
  EcnTag::SetEcn(uint8_t value) {
    NS_ASSERT_MSG(value <= 3, "ECN must be less than 3");
    m_ecn = value;
  }

  uint8_t
  EcnTag::GetEcn() const {
    return m_ecn;
  }
  
  uint32_t
  EcnTag::GetSerializedSize (void) const {
    return 1;
  }

  void
  EcnTag::Serialize (TagBuffer i) const {
    i.WriteU8(m_ecn);
  }

  void
  EcnTag::Deserialize (TagBuffer i) {
    m_ecn = i.ReadU8();
  }

  void
  EcnTag::Print(std::ostream &os) const {
    os << "Ecn value: " << (uint16_t)m_ecn;
  }

} //namespace ns3


  
