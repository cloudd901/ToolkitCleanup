using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace ToolkitCleanup
{
    public enum AnimationType
    {
        Linear,
        GarbledRandom,
        GarbledLinear,
        SlideIn
    }

    public class ConsoleWriter : IDisposable
    {
        public ConsoleColor AniTextColorExplicit = ConsoleColor.Green;
        public ConsoleColor TextColorExplicit = ConsoleColor.White;
        private bool disposedValue;

        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
        public string GarbledCharacterString { get; set; } = "!@#$%^&*()_+-=ABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*abcdefghijklmnopqrstuvwxyz!@#$%^&*0123456789!@#$%^&*";
        private AnimationType AniType { get; set; }
        private ConsoleColor[] CC1 { get; set; } = new ConsoleColor[1];
        private ConsoleColor[] CC2 { get; set; } = new ConsoleColor[1];
        private int LineWidth { get; set; } = 0;
        private string[] S1 { get; set; }
        private int Speed { get; set; }
        private int StartPosition { get; set; }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public ConsoleWriter WriteAnimated(string text, int speed = 100, ConsoleColor? textColor = null, ConsoleColor? animationColor = null, AnimationType aniType = AnimationType.Linear)
        {
            Writer(new string[1] { text }, speed, textColor != null ? new ConsoleColor?[1] { textColor } : null, animationColor != null ? new ConsoleColor?[1] { animationColor } : null, aniType);
            return this;
        }

        public ConsoleWriter WriteLineAnimated(string text, int speed = 100, ConsoleColor? textColor = null, ConsoleColor? animationColor = null, AnimationType aniType = AnimationType.Linear)
        {
            Writer(new string[1] { text }, speed, textColor != null ? new ConsoleColor?[1] { textColor } : null, animationColor != null ? new ConsoleColor?[1] { animationColor } : null, aniType);
            Console.WriteLine();
            return this;
        }

        public ConsoleWriter WriteLineAnimated(string[] text, int speed = 100, ConsoleColor?[] textColor = null, ConsoleColor?[] animationColor = null, AnimationType aniType = AnimationType.Linear)
        {
            Writer(text, speed, textColor, animationColor, aniType);
            Console.WriteLine();
            return this;
        }

        public ConsoleWriter WriteLineAnimated(string[] text, int speed = 100, ConsoleColor?[] textColor = null, ConsoleColor? animationColor = null, AnimationType aniType = AnimationType.Linear)
        {
            Writer(text, speed, textColor, animationColor != null ? new ConsoleColor?[1] { animationColor } : null, aniType);
            Console.WriteLine();
            return this;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CC1 = null;
                    CC2 = null;
                    S1 = null;
                }
                disposedValue = true;
            }
        }

        private void Writer(string[] s1, int speed = 100, ConsoleColor?[] cc1 = null, ConsoleColor?[] cc2 = null, AnimationType aniType = AnimationType.Linear)
        {
            // Set Parameters
            if (cc1 != null)
            {
                CC1 = new ConsoleColor[cc1.Length];
                for (int i = 0; i < cc1.Length; i++)
                {
                    CC1[i] = (ConsoleColor)cc1[i];
                }
            }
            else
            {
                CC1 = new ConsoleColor[] { TextColorExplicit };
            }

            if (cc2 != null)
            {
                CC2 = new ConsoleColor[cc2.Length];
                for (int i = 0; i < cc2.Length; i++)
                {
                    CC2[i] = (ConsoleColor)cc2[i];
                }
            }
            else
            {
                CC2 = new ConsoleColor[] { AniTextColorExplicit };
            }

            S1 = s1;
            Speed = speed;
            AniType = aniType;
            StartPosition = Console.CursorLeft;
            LineWidth = Console.BufferWidth - 2;

            ConsoleColor fcc = Console.ForegroundColor;
            ConsoleColor bcc = Console.BackgroundColor;
            Console.BackgroundColor = BackgroundColor;
            int lineLength = 0;
            for (int i = 0; i < s1.Length; i++)
            {
                lineLength += s1[i].Length;
            }

            Console.Write("".PadRight(LineWidth - Console.CursorLeft, ' '));
            Console.SetCursorPosition(StartPosition, Console.CursorTop);

            if (Speed > 0)
            {
                if (AniType == AnimationType.Linear)
                {
                    Linear();
                }
                else if (AniType == AnimationType.GarbledLinear || AniType == AnimationType.GarbledRandom)
                {
                    Garbled();
                }
                else if (AniType == AnimationType.SlideIn)
                {
                    SlideIn();
                }
            }

            Console.SetCursorPosition(StartPosition, Console.CursorTop);
            for (int i = 0; i < s1.Length; i++)
            {
                Console.ForegroundColor = CC1[i];
                Console.Write(s1[i]);
            }

            Console.BackgroundColor = bcc;
            Console.ForegroundColor = fcc;
        }

        #region Animation Effects

        private void Garbled()
        {
            Stopwatch sw = new Stopwatch();
            Random random = new Random();
            bool multiAnimationColor = CC2.Length > 1;

            for (int i = 0; i < S1.Length; i++)
            {
                // Create Garbled string
                char[] scrambledCopy = new char[S1[i].Length];
                for (int j = 0; j < S1[i].Length; j++)
                {
                    scrambledCopy[j] = (!char.IsWhiteSpace(S1[i][j]) ? GarbledCharacterString[random.Next(GarbledCharacterString.Length)] : ' ');
                }

                List<int> scrambledKeys = new List<int>();
                for (int j = 0; j < S1[i].Length; j++)
                {
                    scrambledKeys.Add(j);
                }

                // Write previously Animated strings
                int pos = StartPosition;
                for (int j = 0; j < i; j++)
                {
                    Console.SetCursorPosition(StartPosition, Console.CursorTop);
                    Console.ForegroundColor = CC1[j];
                    Console.Write(S1[j]);
                    pos += S1[j].Length;
                }

                if (AniType == AnimationType.GarbledRandom)
                {
                    // Animate current string [i]
                    for (int j = 0; j < S1[i].Length; j++)
                    {
                        Console.SetCursorPosition(pos, Console.CursorTop);

                        int key = scrambledKeys[random.Next(scrambledKeys.Count)];
                        scrambledKeys.Remove(key);

                        scrambledCopy[key] = S1[i][key];
                        string copy = new string(scrambledCopy);

                        Console.ForegroundColor = CC1[i];
                        Console.Write(copy.Remove(key));

                        Console.ForegroundColor = CC2[multiAnimationColor ? i : 0];
                        Console.Write(scrambledCopy[key]);

                        Console.ForegroundColor = CC1[i];
                        Console.Write(copy.Substring(key + 1));

                        sw.Restart();
                        while (sw.ElapsedMilliseconds < (Speed > 0 && !char.IsWhiteSpace(S1[i][j]) ? Speed : 0))
                        {
                            // Do nothing - no sleep
                        }
                    }
                }
                else if (AniType == AnimationType.GarbledLinear)
                {
                    // Animate current string [i]
                    for (int j = 0; j < S1[i].Length; j++)
                    {
                        Console.SetCursorPosition(pos, Console.CursorTop);
                        scrambledCopy[j] = S1[i][j];
                        string copy = new string(scrambledCopy);

                        Console.ForegroundColor = CC1[i];
                        Console.Write(copy.Remove(j));

                        Console.ForegroundColor = CC2[multiAnimationColor ? i : 0];
                        Console.Write(scrambledCopy[j]);

                        Console.ForegroundColor = CC1[i];
                        Console.Write(copy.Substring(j + 1));

                        sw.Restart();
                        while (sw.ElapsedMilliseconds < (Speed > 0 && !char.IsWhiteSpace(S1[i][j]) ? Speed : 0))
                        {
                            // Do nothing - no sleep
                        }
                    }
                }
            }

            sw.Stop();
        }

        private void Linear()
        {
            Stopwatch sw = new Stopwatch();
            bool multiAnimationColor = CC2.Length > 1;
            int startPos = Console.CursorLeft;
            for (int i = 0; i < S1.Length; i++)
            {
                StringBuilder temp = new StringBuilder();
                int pos = startPos;
                for (int j = 0; j < S1[i].Length; j++)
                {
                    // Write previously Animated strings
                    Console.SetCursorPosition(pos, Console.CursorTop);
                    for (int k = 0; k < i; k++)
                    {
                        Console.ForegroundColor = CC1[k];
                        Console.Write(S1[k]);
                    }

                    // Animate current string [i]
                    Console.ForegroundColor = CC1[i];
                    Console.Write(temp);
                    Console.ForegroundColor = CC2[multiAnimationColor ? i : 0];
                    Console.Write(char.ToUpperInvariant(S1[i][j]));
                    temp.Append(S1[i][j]);

                    sw.Restart();
                    while (sw.ElapsedMilliseconds < (Speed > 0 && !char.IsWhiteSpace(S1[i][j]) ? Speed : 0))
                    {
                        // Do nothing - no sleep
                    }
                }

                temp.Clear();
                temp = null;
            }

            sw.Stop();
        }

        private void SlideIn()
        {
            Stopwatch sw = new Stopwatch();
            bool multiAnimationColor = CC2.Length > 1;
            int startPos = Console.CursorLeft;
            for (int i = 0; i < S1.Length; i++)
            {
                StringBuilder temp = new StringBuilder();
                int pos = startPos;
                for (int j = 0; j < S1[i].Length; j++)
                {
                    // Write previously Animated strings
                    Console.SetCursorPosition(pos, Console.CursorTop);
                    for (int k = 0; k < i; k++)
                    {
                        Console.ForegroundColor = CC1[k];
                        Console.Write(S1[k]);
                    }

                    // Animate current string [i]
                    Console.ForegroundColor = CC1[i];
                    Console.Write(temp);
                    Console.ForegroundColor = CC2[multiAnimationColor ? i : 0];
                    if (!char.IsWhiteSpace(S1[i][j]))
                    {
                        int dest = Console.CursorLeft;
                        int space = LineWidth - dest;
                        int newSpeed;
                        if (Speed > 0)
                        {
                            newSpeed = (Speed / S1[i].Length);
                            if (newSpeed == 0)
                            {
                                newSpeed = 1;
                            }
                        }
                        else
                        {
                            newSpeed = 0;
                        }

                        for (int k = space; k >= 0; k--)
                        {
                            Console.SetCursorPosition(dest, Console.CursorTop);
                            string toWrite = char.ToUpperInvariant(S1[i][j]).ToString().PadLeft(k, ' ').PadRight(space, ' ');
                            Console.Write(toWrite);
                            sw.Restart();
                            while (sw.ElapsedMilliseconds < newSpeed)
                            {
                                // Do nothing - no sleep
                            }
                        }
                    }

                    temp.Append(S1[i][j]);
                }

                temp.Clear();
                temp = null;
            }

            sw.Stop();
        }

        #endregion Animation Effects
    }
}