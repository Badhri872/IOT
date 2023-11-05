namespace EnergyMeter
{
    public enum ParameterType
    {
        INT16,
        Int16U,
        Float32,
        UTF8,
        INT16U,
        INT32,
        INT32U,
        INT64,
        FLOAT32,
        FLOAT64,
        BITMAP,
        DATETIME,
        DATE,
        TIME,
        PORTAL,
        FourQFPPF,
    }

    public enum Register
    {
        PrimaryCurrent = 2999,
        SecondaryCurrent = 3001,
        TertiaryCurrent = 3003,
        PrimaryPower = 3053,
        SecondaryPower = 3055,
        TertiaryPower = 3057,
    }
}
