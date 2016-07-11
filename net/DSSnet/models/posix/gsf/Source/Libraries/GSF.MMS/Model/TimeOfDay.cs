//
// This file was generated by the BinaryNotes compiler.
// See http://bnotes.sourceforge.net 
// Any modifications to this file will be lost upon recompilation of the source ASN.1. 
//

using System.Runtime.CompilerServices;
using GSF.ASN1;
using GSF.ASN1.Attributes;
using GSF.ASN1.Coders;
using GSF.ASN1.Types;

namespace GSF.MMS.Model
{
    
    [ASN1PreparedElement]
    [ASN1BoxedType(Name = "TimeOfDay")]
    public class TimeOfDay : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(TimeOfDay));
        private byte[] val;

        public TimeOfDay()
        {
        }

        public TimeOfDay(byte[] value)
        {
            Value = value;
        }

        public TimeOfDay(BitString value)
        {
            Value = value.Value;
        }

        [ASN1OctetString(Name = "TimeOfDay")]
        //[ASN1SizeConstraint ( Max = 4L )]
        public byte[] Value
        {
            get
            {
                return val;
            }
            set
            {
                val = value;
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