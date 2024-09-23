using System;

namespace Dman.GridGameTools.PathingData
{
    [Flags]
    public enum PathingLayers : uint
    {
        Static = 1<<0,
        Mobile = 1<<1,
        
        UserLayer00 = 1 << 4,
        UserLayer01 = 1 << 5,
        UserLayer02 = 1 << 6,
        UserLayer03 = 1 << 7,
        UserLayer04 = 1 << 8,
        UserLayer05 = 1 << 9,
        AllUserLayers = UserLayer00 | UserLayer01 | UserLayer02 | UserLayer03 | UserLayer04 | UserLayer05,
    
        None = 0,
        [Obsolete("Use AllCore or AllLayers, depending on if user-defined layers should be included or not.")]
        All = Static | Mobile,
        
        AllCore = Static | Mobile,
        AllLayers = AllCore | AllUserLayers,
    }
}