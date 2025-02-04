using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;

namespace TabSwitch;

internal class Program
{
    private static System.Timers.Timer switchTimer;
    public static int TabNr { get; set; } = 1;
    static void Main(string[] args)
    {
        InitializeScroll();
        InitializeTimer();
         Console.ReadLine();
    }

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, UIntPtr dwExtraInfo);
    // Constants for mouse events
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    // Constants for mouse events
    private const uint MOUSEEVENTF_WHEEL = 0x0800;
    
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern void SetCursorPos(int X, int Y);

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    static void TriggerMouseClick(int scrollAmount, int x = 0, int y = 0)
    {
        Console.WriteLine($"Mouse click triggered. x={x} and y={y}");
        if (x != 0 && y != 0)
        {
            SetCursorPos(x, y);
        }
        else {
            SetCursorPos(650, 550);
        }
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        System.Threading.Thread.Sleep(100);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

        GetCursorPositionOnClick();
    }
    static void TriggerMouseScroll(int scrollAmount)
    {
        Console.WriteLine("src: " + scrollAmount);
        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, scrollAmount, UIntPtr.Zero);
    }

    private static void InitializeTimer()
    {
        Console.WriteLine("Initializing timer");
        switchTimer = new System.Timers.Timer(30000); // 60000 milliseconds = 1 minute
        switchTimer.Elapsed += OnTimedEvent!;
        switchTimer.AutoReset = true;
        switchTimer.Enabled = true;
    }

    private static void InitializeScroll()
    {

        switchTimer = new System.Timers.Timer(3000); // 60000 milliseconds = 1 minute
        switchTimer.Elapsed += OnScrollEvent!;
        switchTimer.AutoReset = true;
        switchTimer.Enabled = true;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    static void GetCursorPositionOnClick()
    {
        while (true)
        {
            if ((GetAsyncKeyState(0x01) & 0x8000) != 0) // 0x01 is VK_LBUTTON
            {
                GetCursorPos(out POINT position);
                Console.WriteLine($"Cursor Position: X = {position.X}, Y = {position.Y}");
                break;
            }
            Thread.Sleep(100);
        }
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    private static void OnScrollEvent(Object source, ElapsedEventArgs e)
    {
        var rnd = new Random();
        var next = rnd.Next(100, 400);

        var randomBool = rnd.Next(20) == 10;

        Console.WriteLine("src: " + next);

        TriggerMouseScroll(randomBool ? -1 * next : next);

        TriggerMouseClick(randomBool ? -1 * next : next);
    }
   
    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
        //TriggerAltTab();
        TrigerChangeTab(TabNr);
    }


    //52/103 dhe 143/103
    private static void TrigerChangeTab(int nrTab)
    {

        Console.WriteLine($"TrigerChangeTab. tab = {nrTab}");
        if (nrTab == 1)
        {
            TabNr = 2;
            TriggerMouseClick(0, 143, 103);
        }
        else
        {
            TabNr = 1;
            TriggerMouseClick(0, 52, 103);
        }

    }
    private static void TriggerAltTab()
    {
        var inputs = new INPUT[4];

        // Alt key down
        inputs[0].type = 1; // INPUT_KEYBOARD
        inputs[0].u.ki.wVk = (ushort)Keys.Menu; // VK_MENU (Alt key)
        inputs[0].u.ki.dwFlags = 0; // Key down

        // Tab key down
        inputs[1].type = 1; // INPUT_KEYBOARD
        inputs[1].u.ki.wVk = (ushort)Keys.Tab; // VK_TAB
        inputs[1].u.ki.dwFlags = 0; // Key down

        // Tab key up
        inputs[2].type = 1; // INPUT_KEYBOARD
        inputs[2].u.ki.wVk = (ushort)Keys.Tab; // VK_TAB
        inputs[2].u.ki.dwFlags = 2; // Key up

        // Tab key down
        inputs[1].type = 1; // INPUT_KEYBOARD
        inputs[1].u.ki.wVk = (ushort)Keys.Tab; // VK_TAB
        inputs[1].u.ki.dwFlags = 0; // Key down

        // Tab key up
        inputs[2].type = 1; // INPUT_KEYBOARD
        inputs[2].u.ki.wVk = (ushort)Keys.Tab; // VK_TAB
        inputs[2].u.ki.dwFlags = 2; // Key up

        // Alt key up
        inputs[3].type = 1; // INPUT_KEYBOARD
        inputs[3].u.ki.wVk = (ushort)Keys.Menu; // VK_MENU (Alt key)
        inputs[3].u.ki.dwFlags = 2; // Key up

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    private enum Keys : ushort
    {
        Tab = 0x09,
        Tab2 = 0x09,
        Menu = 0x12 // Alt key
    }
}
