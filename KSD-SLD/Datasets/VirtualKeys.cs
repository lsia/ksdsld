using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDSLD.Datasets
{
    public enum VirtualKeys : byte
    {
        VK_SHIFT = 0x10,

        VK_0 = 0x30,
        VK_9 = 0x39,

        VK_A = 0x41,
        VK_Z = 0x5A,

        VK_BACK = 0x08,
        VK_TAB = 0x09,
        VK_RETURN = 0x0D,

        VK_SPACE = 0x20,

        VK_PRIOR = 0x21,
        VK_NEXT = 0x22,
        VK_END = 0x23,
        VK_HOME = 0x24,
        
        VK_LEFT = 0x25,
        VK_UP = 0x26,
        VK_RIGHT = 0x27,
        VK_DOWN = 0x28,

        VK_OEM_1 = 0xBA,
        VK_OEM_PLUS = 0xBB,
        VK_OEM_COMMA = 0xBC,
        VK_OEM_MINUS = 0xBD,
        VK_OEM_PERIOD = 0xBE,
        VK_OEM_2 = 0xBF,
        VK_OEM_3 = 0xC0,

        VK_OEM_7 = 0xDE,

        VK_LSHIFT = 0xA0,
        VK_RSHIFT = 0xA1,
        VK_LCONTROL = 0xA2,
        VK_RCONTROL = 0xA3,
        VK_LMENU = 0xA4,
        VK_RMENU = 0xA5
    }
}
