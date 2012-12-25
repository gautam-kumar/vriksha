#ifndef ECN_TAG_H
#define ECN_TAG_H

#include "ns3/tag.h"

namespace ns3 {
  
  class EcnTag : public Tag {

  public:
    EcnTag();

    void SetEcn(uint8_t val);
    uint8_t GetEcn() const;

    static TypeId GetTypeId (void);
    virtual TypeId GetInstanceTypeId (void) const;
    virtual void Serialize (TagBuffer i) const;
    virtual void Deserialize (TagBuffer i);
    virtual uint32_t GetSerializedSize () const;
    virtual void Print (std::ostream &os) const;

  private:
    uint8_t m_ecn;
  };

} //namespace ns3

#endif /* ECN_TAG_H */
