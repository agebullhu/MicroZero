namespace Agebull.Common.Tson
{
    /// <summary>
    /// ◊÷∂Œ¿‡–Õ
    /// </summary>
    public enum TsonDataType : byte
    {
        None,
        Bool,
        Byte,
        SByte,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        Decimal,
        Float,
        Double,
        Guid,
        DateTime,
        Object = 0xF0,
        Array = 0xF1,
        String = 0xF2,
        Empty = 0xFE,
        Nil = 0xFF
    };
}