//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.573
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=1.1.4322.573.
// 
namespace Technosoftware.DaAeHdaClient.Cpx
{
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    [XmlRootAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/", IsNullable=false)]
    public class TypeDictionary {
        
        /// <remarks/>
        [XmlElementAttribute("TypeDescription")]
        public TypeDescription[] TypeDescription;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string DictionaryName;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool DefaultBigEndian = true;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("UCS-2")]
        public string DefaultStringEncoding = "UCS-2";
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(2)]
        public int DefaultCharWidth = 2;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("IEEE-754")]
        public string DefaultFloatFormat = "IEEE-754";
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class TypeDescription {
        
        /// <remarks/>
        [XmlElementAttribute("Field")]
        public FieldType[] Field;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string TypeID;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public bool DefaultBigEndian;
        
        /// <remarks/>
        [XmlIgnoreAttribute()]
        public bool DefaultBigEndianSpecified;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string DefaultStringEncoding;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public int DefaultCharWidth;
        
        /// <remarks/>
        [XmlIgnoreAttribute()]
        public bool DefaultCharWidthSpecified;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string DefaultFloatFormat;
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    [XmlIncludeAttribute(typeof(TypeReference))]
    [XmlIncludeAttribute(typeof(CharString))]
    [XmlIncludeAttribute(typeof(Unicode))]
    [XmlIncludeAttribute(typeof(Ascii))]
    [XmlIncludeAttribute(typeof(FloatingPoint))]
    [XmlIncludeAttribute(typeof(Double))]
    [XmlIncludeAttribute(typeof(Single))]
    [XmlIncludeAttribute(typeof(Integer))]
    [XmlIncludeAttribute(typeof(UInt64))]
    [XmlIncludeAttribute(typeof(UInt32))]
    [XmlIncludeAttribute(typeof(UInt16))]
    [XmlIncludeAttribute(typeof(UInt8))]
    [XmlIncludeAttribute(typeof(Int64))]
    [XmlIncludeAttribute(typeof(Int32))]
    [XmlIncludeAttribute(typeof(Int16))]
    [XmlIncludeAttribute(typeof(Int8))]
    [XmlIncludeAttribute(typeof(BitString))]
    public class FieldType {
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string Name;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string Format;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public int Length;
        
        /// <remarks/>
        [XmlIgnoreAttribute()]
        public bool LengthSpecified;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public int ElementCount;
        
        /// <remarks/>
        [XmlIgnoreAttribute()]
        public bool ElementCountSpecified;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string ElementCountRef;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string FieldTerminator;
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class TypeReference : FieldType {
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string TypeID;
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    [XmlIncludeAttribute(typeof(Unicode))]
    [XmlIncludeAttribute(typeof(Ascii))]
    public class CharString : FieldType {
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public int CharWidth;
        
        /// <remarks/>
        [XmlIgnoreAttribute()]
        public bool CharWidthSpecified;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string StringEncoding;
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string CharCountRef;
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class Unicode : CharString {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class Ascii : CharString {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    [XmlIncludeAttribute(typeof(Double))]
    [XmlIncludeAttribute(typeof(Single))]
    public class FloatingPoint : FieldType {
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        public string FloatFormat;
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class Double : FloatingPoint {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class Single : FloatingPoint {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    [XmlIncludeAttribute(typeof(UInt64))]
    [XmlIncludeAttribute(typeof(UInt32))]
    [XmlIncludeAttribute(typeof(UInt16))]
    [XmlIncludeAttribute(typeof(UInt8))]
    [XmlIncludeAttribute(typeof(Int64))]
    [XmlIncludeAttribute(typeof(Int32))]
    [XmlIncludeAttribute(typeof(Int16))]
    [XmlIncludeAttribute(typeof(Int8))]
    public class Integer : FieldType {
        
        /// <remarks/>
        [XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool Signed = true;
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class UInt64 : Integer {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class UInt32 : Integer {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class UInt16 : Integer {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class UInt8 : Integer {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class Int64 : Integer {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class Int32 : Integer {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class Int16 : Integer {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class Int8 : Integer {
    }
    
    /// <remarks/>
    [XmlTypeAttribute(Namespace="http://opcfoundation.org/OPCBinary/1.0/")]
    public class BitString : FieldType {
    }
}
