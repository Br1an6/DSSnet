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
    [ASN1Choice(Name = "LoadUnitControlFromFile_Error")]
    public class LoadUnitControlFromFile_Error : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(LoadUnitControlFromFile_Error));
        private Identifier domain_;
        private bool domain_selected;
        private NullObject none_;
        private bool none_selected;


        private Identifier programInvocation_;
        private bool programInvocation_selected;

        [ASN1Null(Name = "none")]
        [ASN1Element(Name = "none", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public NullObject None
        {
            get
            {
                return none_;
            }
            set
            {
                selectNone(value);
            }
        }

        [ASN1Element(Name = "domain", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public Identifier Domain
        {
            get
            {
                return domain_;
            }
            set
            {
                selectDomain(value);
            }
        }


        [ASN1Element(Name = "programInvocation", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public Identifier ProgramInvocation
        {
            get
            {
                return programInvocation_;
            }
            set
            {
                selectProgramInvocation(value);
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


        public bool isNoneSelected()
        {
            return none_selected;
        }


        public void selectNone()
        {
            selectNone(new NullObject());
        }


        public void selectNone(NullObject val)
        {
            none_ = val;
            none_selected = true;


            domain_selected = false;

            programInvocation_selected = false;
        }


        public bool isDomainSelected()
        {
            return domain_selected;
        }


        public void selectDomain(Identifier val)
        {
            domain_ = val;
            domain_selected = true;


            none_selected = false;

            programInvocation_selected = false;
        }


        public bool isProgramInvocationSelected()
        {
            return programInvocation_selected;
        }


        public void selectProgramInvocation(Identifier val)
        {
            programInvocation_ = val;
            programInvocation_selected = true;


            none_selected = false;

            domain_selected = false;
        }
    }
}