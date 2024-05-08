public enum PathingLayers
{
    Static = 1<<0,
    Mobile = 1<<1,
    
    None = 0,
    All = Static | Mobile
}