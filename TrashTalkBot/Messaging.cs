using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Net.Http;
using System.Windows.Input;
using static TrashTalkBot.Keyboard;
using static TrashTalkBot.KeyMessage;

namespace TrashTalkBot
{
    internal static class Messaging
    {
        public static bool isOffMessaging = false;
        public static readonly int WM_HOTKEY = 0x312;
        public static Key OpenChatKey = Key.T;
        public static LevelMessage CurrentLevelMessage = LevelMessage.Bad;
        public static bool IsLevelRandom = true;
        public static readonly Dictionary<int, string[]> TrashTalkDictionnary = new Dictionary<int, string[]>();

        private static readonly Random Rand = new Random((int)DateTime.Now.Ticks);
        private static double StandardVariation = 1.0;

        #region Enum Type and Level
        public enum TypeMessage
        {
            Hello,
            Hf,
            WhatASave,
            NiceShot,
            TrySmth,
            Gg,
            Ez,
            Offense,
            ProTip,
            FF,
            Other,
            Miss
        }

        public enum LevelMessage
        {
            Nice = 1,
            Okay,
            Bad,
            Harsh,
            Demoniac
        }
        #endregion

        #region Managing CSV
        public static string? RawText;
        /// <summary>
        /// Allows to parse the csv file into the TrashTalkDictionnary
        /// </summary>
        /// <exception cref="Exception">If RawText is not initialized before calling ParseCSV(), an exception is thrown</exception>
        private static void ParseCSV()
        {
            if (RawText == null)
                throw new Exception("CSV could not be parsed because its text input was null");
            using (TextFieldParser parser = new TextFieldParser((TextReader)new StringReader(RawText)))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {

                    string[]? fields = parser.ReadFields();
                    if (fields == null)
                        throw new Exception("parser variable is null");

                    if (fields.Length == 0) break;
                    var LevelRow = (LevelMessage)(Int32.Parse(fields[0].Last().ToString()));

                    //Check the type of message
                    var TypeString = fields[0].Remove(fields[0].Length - 1);
                    if (!Enum.TryParse<TypeMessage>(TypeString, out var TypeRow))
                        Console.WriteLine("One type could not be parsed. Type : " + TypeString);


                    //Checks if key is already attributed
                    if (!TrashTalkDictionnary.TryGetValue(getKey(TypeRow, LevelRow), out var a))
                        TrashTalkDictionnary.Add(getKey(TypeRow, LevelRow), fields.Skip(1).ToList().Where(s => !string.IsNullOrEmpty(s)).ToArray());
                }
            }
        }

        /// <summary>
        /// Reads the TrashTalkDatabase.csv file and calls ParseCSV()
        /// </summary>
        public static void InitDataset()
        {
            RawText = File.ReadAllText("TrashTalkDatabase.csv");
            ParseCSV();
        }
        #endregion

        #region Send
        /// <summary>
        /// Opens chat, writes message, types RETURN;
        /// </summary>
        /// <param name="msg"></param>
        private static void WriteMessage(string msg)
        {
            Type(OpenChatKey);
            Type(msg, 1);
            Type(Key.Enter);
        }

        /// <summary>
        /// Ch
        /// </summary>
        /// <param name="idKeyPressed">VirtualKey ID that has been pressed</param>
        /// <exception cref="Exception">If the Virtual Key was not created via the KeyMessage class an exception is thrown</exception>
        public static void SendMessage(int idKeyPressed)
        {
            if (isOffMessaging)
                return;

            var level = IsLevelRandom ? GetRandLevel() : CurrentLevelMessage;
            if (!KeyMessage.TryGetKeyMessage((Key)idKeyPressed, out var km))
                throw new Exception("idKeyPressed does not exist in dictionnary : " + idKeyPressed);

            if (TrashTalkDictionnary.TryGetValue(getKey(km.Type, level), out var msgList))
            {
                if (msgList == null || msgList.Count() == 0)
                    return;

                var r = Rand.Next(msgList.Length);
                WriteMessage(msgList[r]);
            }
        }

        /// <summary>
        /// Calculated a random output from a reduced centered law
        /// </summary>
        /// <returns>random LevelMessage centered on Bad</returns>
        private static LevelMessage GetRandLevel()
        {
            double u1 = 1.0 - Rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - Rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = 3.5 + StandardVariation * randStdNormal;
            switch (randNormal)
            {
                case < 2:
                    return LevelMessage.Nice;
                case < 3:
                    return LevelMessage.Okay;
                case < 4:
                    return LevelMessage.Bad;
                case < 5:
                    return LevelMessage.Harsh;
                case >= 5:
                    return LevelMessage.Demoniac;
                default:
                    Console.WriteLine("Error while calculating gaussian value");
                    return LevelMessage.Bad;
            }
        }
        #endregion
    }
}
