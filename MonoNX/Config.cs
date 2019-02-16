using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonoNX
{
    public static class Config
    {
        public static bool LoggingEnableInfo { get; private set; }
        public static bool LoggingEnableTrace { get; private set; }
        public static bool LoggingEnableDebug { get; private set; }
        public static bool LoggingEnableWarn { get; private set; }
        public static bool LoggingEnableError { get; private set; }
        public static bool LoggingEnableFatal { get; private set; }
        public static bool LoggingEnableLogFile { get; private set; }

        public static JoyCon FakeJoyCon { get; private set; }

        public static void Read()
        {
            //var iniFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            //var iniPath = Path.Combine(iniFolder, "Ryujinx.conf");
            //IniParser Parser = new IniParser(iniPath);

            LoggingEnableInfo    = true;
            LoggingEnableTrace   = false;
            LoggingEnableDebug   = false;
            LoggingEnableWarn    = true;
            LoggingEnableError   = true;
            LoggingEnableFatal   = true;
            LoggingEnableLogFile = false;

            FakeJoyCon = new JoyCon
            {
                Left = new JoyConLeft
                {
                    StickUp     = 91,
                    StickDown   = 93,
                    StickLeft   = 92,
                    StickRight  = 94,
                    StickButton = 0,
                    DPadUp      = 45,
                    DPadDown    = 46,
                    DPadLeft    = 47,
                    DPadRight   = 48,
                    ButtonMinus = 52,
                    ButtonL     = 0,
                    ButtonZL    = 0
                },

                Right = new JoyConRight
                {
                    StickUp     = 45,
                    StickDown   = 46,
                    StickLeft   = 47,
                    StickRight  = 48,
                    StickButton = 0,
                    ButtonA     = 83,
                    ButtonB     = 101,
                    ButtonX     = 106,
                    ButtonY     = 108,
                    ButtonPlus  = 49,
                    ButtonR     = 0,
                    ButtonZR    = 0
                }
            };
        }
    }

    // https://stackoverflow.com/a/37772571
    public class IniParser
    {
        private readonly Dictionary<string, string> Values;

        public IniParser(string Path)
        {
            Values = File.ReadLines(Path)
            .Where(Line => !string.IsNullOrWhiteSpace(Line) && !Line.StartsWith("#"))
            .Select(Line => Line.Split('=', (char)2))
            .ToDictionary(Parts => Parts[0].Trim(), Parts => Parts.Length > 1 ? Parts[1].Trim() : null);
        }

        /// <summary>
        /// Gets the setting value for the requested setting <see cref="Name"/>.
        /// </summary>
        /// <param name="Name">Setting Name</param>
        /// <param name="defaultValue">Default value of the setting</param>
        public string Value(string Name, string defaultValue = null)
        {
            return Values.TryGetValue(Name, out var value) ? value : defaultValue;
        }
    }
}
