namespace DotGrid.Binary
{
    public enum BinaryFormatValueType : int
    {
        Undefined = 0x00,
        Null,
        
        // Integer types
        Byte,
        Short,
        Int,
        Long,
        
        // Float types
        Float,
        Double,

        // String types
        String,
        
        // Additional types
        Boolean,
        Blob,
        
        // Complex types
        Array,
        Object,
        Document
    }
}