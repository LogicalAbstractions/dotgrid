namespace DotGrid.Binary2
{
    public enum ValueType : byte
    {
        Undefined,
        Null,
        Document,
        Object,
        Array,
        
        Boolean,
        String,
        Blob,
        
        Byte,
        Short,
        Int,
        Long,
        
        Float,
        Double
    }
}