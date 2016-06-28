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
    [ASN1Sequence(Name = "ErrorParameters", IsSet = false)]
    public class ErrorParameters : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(ErrorParameters));
        private MMSString additionalCode_;


        private AdditionalDetialSequenceType additionalDetial_;

        [ASN1Element(Name = "additionalCode", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public MMSString AdditionalCode
        {
            get
            {
                return additionalCode_;
            }
            set
            {
                additionalCode_ = value;
            }
        }

        [ASN1Element(Name = "additionalDetial", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public AdditionalDetialSequenceType AdditionalDetial
        {
            get
            {
                return additionalDetial_;
            }
            set
            {
                additionalDetial_ = value;
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

        [ASN1PreparedElement]
        [ASN1Sequence(Name = "additionalDetial", IsSet = false)]
        public class AdditionalDetialSequenceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(AdditionalDetialSequenceType));
            private long size_;


            private MMSString syntax_;

            [ASN1Integer(Name = "")]
            [ASN1Element(Name = "size", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
            public long Size
            {
                get
                {
                    return size_;
                }
                set
                {
                    size_ = value;
                }
            }

            [ASN1Element(Name = "syntax", IsOptional = false, HasTag = true, Tag = 3, HasDefaultValue = false)]
            public MMSString Syntax
            {
                get
                {
                    return syntax_;
                }
                set
                {
                    syntax_ = value;
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
        }
    }
}