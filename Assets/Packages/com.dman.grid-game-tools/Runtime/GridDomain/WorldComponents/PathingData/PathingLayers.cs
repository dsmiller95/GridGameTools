using System;

namespace Dman.GridGameTools.PathingData
{
    public enum PathingLayers : int
    {
        Static = 1<<0,
        Mobile = 1<<1,
        
        UserLayer00 = 1 << 16,
        UserLayer01 = 1 << 17,
        UserLayer02 = 1 << 18,
        UserLayer03 = 1 << 19,
        UserLayer04 = 1 << 21,
        UserLayer05 = 1 << 22,
        UserLayer06 = 1 << 23,
        UserLayer07 = 1 << 24,
        UserLayer08 = 1 << 25,
        UserLayer09 = 1 << 26,
        UserLayer10 = 1 << 27,
        UserLayer11 = 1 << 28,
        UserLayer12 = 1 << 29,
        UserLayer13 = 1 << 30,
        AllUserLayers = UserLayer00 | UserLayer01 | UserLayer02 | UserLayer03 | UserLayer04 | UserLayer05 | UserLayer06 | UserLayer07 | UserLayer08 | UserLayer09 | UserLayer10 | UserLayer11 | UserLayer12 | UserLayer13,
    
        None = 0,
        [Obsolete("Use AllCore or AllLayers, depending on if user-defined layers should be included or not.")]
        All = Static | Mobile,
        
        AllCore = Static | Mobile,
        AllLayers = AllCore | AllUserLayers,
    }
}