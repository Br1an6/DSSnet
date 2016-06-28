//
// This file was generated by the BinaryNotes compiler.
// See http://bnotes.sourceforge.net 
// Any modifications to this file will be lost upon recompilation of the source ASN.1. 
//

using System.Runtime.CompilerServices;
using GSF.ASN1;
using GSF.ASN1.Attributes;
using GSF.ASN1.Coders;

namespace GSF.MMS.Model
{
    
    [ASN1PreparedElement]
    [ASN1Sequence(Name = "InitializeJournal_Request", IsSet = false)]
    public class InitializeJournal_Request : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(InitializeJournal_Request));
        private ObjectName journalName_;


        private LimitSpecificationSequenceType limitSpecification_;

        private bool limitSpecification_present;

        [ASN1Element(Name = "journalName", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public ObjectName JournalName
        {
            get
            {
                return journalName_;
            }
            set
            {
                journalName_ = value;
            }
        }

        [ASN1Element(Name = "limitSpecification", IsOptional = true, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public LimitSpecificationSequenceType LimitSpecification
        {
            get
            {
                return limitSpecification_;
            }
            set
            {
                limitSpecification_ = value;
                limitSpecification_present = true;
            }
        }


        public void initWithDefaults()
        {
        }


        public IASN1PreparedElementData PreparedData
        {
            get
            {
                return preparedData;
            }
        }

        public bool isLimitSpecificationPresent()
        {
            return limitSpecification_present;
        }

        [ASN1PreparedElement]
        [ASN1Sequence(Name = "limitSpecification", IsSet = false)]
        public class LimitSpecificationSequenceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(LimitSpecificationSequenceType));
            private byte[] limitingEntry_;

            private bool limitingEntry_present;
            private TimeOfDay limitingTime_;

            [ASN1Element(Name = "limitingTime", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
            public TimeOfDay LimitingTime
            {
                get
                {
                    return limitingTime_;
                }
                set
                {
                    limitingTime_ = value;
                }
            }

            [ASN1OctetString(Name = "")]
            [ASN1Element(Name = "limitingEntry", IsOptional = true, HasTag = true, Tag = 1, HasDefaultValue = false)]
            public byte[] LimitingEntry
            {
                get
                {
                    return limitingEntry_;
                }
                set
                {
                    limitingEntry_ = value;
                    limitingEntry_present = true;
                }
            }


            public void initWithDefaults()
            {
            }

            public IASN1PreparedElementData PreparedData
            {
                get
                {
                    return preparedData;
                }
            }

            public bool isLimitingEntryPresent()
            {
                return limitingEntry_present;
            }
        }
    }
}