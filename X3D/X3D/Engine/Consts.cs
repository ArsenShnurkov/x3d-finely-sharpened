using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Engine
{
    public class Consts
    {
        public static Dictionary<int, int> ScanCodeToKeyCodeMap = scanCodeToKeyCodeMap();

        private static Dictionary<int, int> scanCodeToKeyCodeMap()
        {
            Dictionary<int, int> k = new Dictionary<int, int>();

            // ~~~ OpenTK Scan Codes -> Javascript keycode map ~~~

            k.Add(0, 0);    // Unknown
            k.Add(1, 16);   // LShift
            k.Add(2, 16);   // RShift
            k.Add(3, 17);   // LControl
            k.Add(4, 17);   // RControl
            k.Add(5, 18);   // LAlt
            k.Add(6, 18);   // RAlt

            k.Add(7, 91);   // LWin OSKey
            k.Add(8, 91);   // RWin OSKey

            k.Add(9, 93);   // Menu CONTEXT_MENU

            k.Add(10, 112); // F1
            k.Add(11, 113); // F2
            k.Add(12, 114); // F3
            k.Add(13, 115); // F4
            k.Add(14, 116); // F5
            k.Add(15, 117); // F6
            k.Add(16, 118); // F7
            k.Add(17, 119); // F8
            k.Add(18, 120); // F9
            k.Add(19, 121); // F10
            k.Add(20, 122); // F11
            k.Add(21, 123); // F12
            k.Add(22, 124); // F13
            k.Add(23, 125); // F14
            k.Add(24, 126); // F15
            k.Add(25, 127); // F16
            k.Add(26, 128); // F17
            k.Add(27, 129); // F18
            k.Add(28, 130); // F19
            k.Add(29, 131); // F20
            k.Add(30, 132); // F21
            k.Add(31, 133); // F22
            k.Add(32, 134); // F23
            k.Add(33, 135); // F24
            k.Add(34, 136); // F25
            k.Add(35, 137); // F26
            k.Add(36, 138); // F27
            k.Add(37, 139); // F28
            k.Add(38, 140); // F29
            k.Add(39, 141); // F30
            k.Add(40, 142); // F31
            k.Add(41, 143); // F32
            k.Add(42, 144); // F33
            k.Add(43, 145); // F34
            k.Add(44, 146); // F35

            k.Add(45, 38);  // Up Arrow
            k.Add(46, 40);  // Down Arrow
            k.Add(47, 37);  // Left Arrow
            k.Add(48, 39);  // Right Arrow

            k.Add(49, 13);  // Enter
            k.Add(50, 27);  // Escape
            k.Add(51, 32);  // Space
            k.Add(52, 9);   // Tab
            k.Add(53, 8);   // Backspace
            k.Add(54, 45);  // Insert
            k.Add(55, 46);  // Delete
            k.Add(56, 33);  // PageUp
            k.Add(57, 34);  // PageDown
            k.Add(58, 36);  // Home
            k.Add(59, 35);  // End
            k.Add(60, 20);  // Capslock
            k.Add(61, 145); // Scrollock
            k.Add(62, 44);  // PrintScreen
            k.Add(63, 19);  // Pause
            k.Add(64, 144); // Numlock
            k.Add(65, 254); // Win Clear

            k.Add(66, 95);  // Sleep

            k.Add(67, 96);  // Keypad0
            k.Add(68, 97);  // Keypad1
            k.Add(69, 98);  // Keypad2
            k.Add(70, 99);  // Keypad3
            k.Add(71, 100); // Keypad4
            k.Add(72, 101); // Keypad5
            k.Add(73, 102); // Keypad6
            k.Add(74, 103); // Keypad7
            k.Add(75, 104); // Keypad8
            k.Add(76, 105); // Keypad9

            k.Add(77, 111); // KeypadDivide
            k.Add(78, 106); // KeypadMultiply
            k.Add(79, 109); // KeypadSubtract
            k.Add(80, 107); // KeypadAdd
            k.Add(81, 110); // KeypadPeriod
            k.Add(82, 13);  // KeypadEnter

            k.Add(83, 65);  // A
            k.Add(84, 66);  // B
            k.Add(85, 67);  // C
            k.Add(86, 68);  // D
            k.Add(87, 69);  // E
            k.Add(88, 70);  // F
            k.Add(89, 71);  // G
            k.Add(90, 72);  // H
            k.Add(91, 73);  // I
            k.Add(92, 74);  // J
            k.Add(93, 75);  // K
            k.Add(94, 76);  // L
            k.Add(95, 77);  // M
            k.Add(96, 78);  // N
            k.Add(97, 79);  // O
            k.Add(98, 80);  // P
            k.Add(99, 81);  // Q
            k.Add(100, 82); // R
            k.Add(101, 83); // S
            k.Add(102, 84); // T
            k.Add(103, 85); // U
            k.Add(104, 86); // V
            k.Add(105, 87); // W
            k.Add(106, 88); // X
            k.Add(107, 89); // Y
            k.Add(108, 90); // Z

            k.Add(109, 48); // NUMBER0
            k.Add(110, 49); // NUMBER1
            k.Add(111, 50); // NUMBER2
            k.Add(112, 51); // NUMBER3
            k.Add(113, 52); // NUMBER4
            k.Add(114, 53); // NUMBER5
            k.Add(115, 54); // NUMBER6
            k.Add(116, 55); // NUMBER7
            k.Add(117, 56); // NUMBER8
            k.Add(118, 57); // NUMBER9

            k.Add(119, 176); // Tilde
            k.Add(120, 189); // Minus
            k.Add(121, 171); // Plus
            k.Add(122, 219); // BracketLeft / Open Bracket
            k.Add(123, 221); // BracketRight / Close bracket
            k.Add(124, 186); // Semicolon
            k.Add(125, 222); // Quote
            k.Add(126, 188); // Comma
            k.Add(127, 190); // Peroid
            k.Add(128, 191); // Slash
            k.Add(129, 220); // BackSlash
            k.Add(130, 220); // NonUSBackSlash
            k.Add(131, 253); // LastKey

            return k;
        }

    }
}
