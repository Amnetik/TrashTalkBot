using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using static TrashTalkBot.Messaging;
using static TrashTalkBot.MainWindow;
using System.IO;

namespace TrashTalkBot
{
    internal class KeyMessage
    {
        private static KeyMessage Empty = new KeyMessage();
        public static string ConfigFilePath = "KeyConfiguration.cfg";
        /// <summary>
        /// Calculates a singular key that codes for both TypeMessage and LevelMessage
        /// </summary>
        /// <param name="type">LevelMessage associated</param>
        /// <param name="level">TypeMessage</param>
        /// <returns></returns>
        public static int getKey(TypeMessage type, LevelMessage level) => Enum.GetNames<TypeMessage>().Length * (int)type + (int)level;

        private static List<KeyMessage> ListKeyMessages = new List<KeyMessage>();

        public Key Key { get; }
        public uint VirtualKey { get; }
        public TypeMessage Type { get; }

        #region KeyMessage
        private KeyMessage()
        {
            Key = Key.None;
            VirtualKey = 0;
            Type = TypeMessage.Other;
        }
        private KeyMessage(Key key, TypeMessage type)
        {
            this.Key = key;
            this.Type = type;
            this.VirtualKey = (uint)KeyInterop.VirtualKeyFromKey(key);
        }

        /// <summary>
        /// Creates a new KeyMessage and adds it to the ListKeyMessages
        /// </summary>
        /// <param name="key">Key to press</param>
        /// <param name="type">Type you want associated with the key</param>
        /// <returns>The new KeyMessage</returns>
        public static KeyMessage CreateKeyMessage(Key key, TypeMessage type)
        {
            var km = new KeyMessage(key, type);
            km.Register();
            ListKeyMessages.Add(km);
            return km;
        }
        /// <summary>
        /// Deletes the KeyMessage and removes it from the ListKeyMessages
        /// </summary>
        public void Dispose()
        {
            Unregister();
            ListKeyMessages.Remove(this);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Calls for handler in NativeMethods to handle key press
        /// </summary>
        public void Register()
        {
            uint modifiers = (uint)ModifierKeys.None;
            NativeMethods.RegisterHotKey(Handler, (int)Key, modifiers, VirtualKey);
        }
        /// <summary>
        /// Calls to suppress handler in NativeMethods to not handle key press
        /// </summary>
        public void Unregister()
        {
            NativeMethods.UnregisterHotKey(Handler, (int)Key);
        }
        #endregion

        #region TryGetKeyMessage
        /// <summary>
        /// Gets a KeyMessage associated with the Key if it exists in the ListKeyMessages
        /// </summary>
        /// <param name="key">Key associated</param>
        /// <param name="keyMessage">KeyMessage corresponding</param>
        /// <returns></returns>
        public static bool TryGetKeyMessage(Key key, out KeyMessage keyMessage)
        {
            keyMessage = KeyMessage.Empty;
            foreach(var km in ListKeyMessages)
            {
                if(km.Key == key)
                {
                    keyMessage = km;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Gets a KeyMessage associated with the Key if it exists in the ListKeyMessages
        /// </summary>
        /// <param name="virtualKey">Virtual Key associated</param>
        /// <param name="keyMessage">KeyMessage corresponding</param>
        /// <returns></returns>
        public static bool TryGetKeyMessage(uint virtualKey, out KeyMessage keyMessage)
        {
            keyMessage = KeyMessage.Empty;
            foreach (var km in ListKeyMessages)
            {
                if (km.VirtualKey == virtualKey)
                {
                    keyMessage = km;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Gets a KeyMessage associated with the Key if it exists in the ListKeyMessages
        /// </summary>
        /// <param name="type">TypeMessage associated</param>
        /// <param name="keyMessage">KeyMessage corresponding</param>
        /// <returns></returns>
        public static bool TryGetKeyMessage(TypeMessage type, out KeyMessage keyMessage)
        {
            keyMessage = KeyMessage.Empty;
            foreach (var km in ListKeyMessages)
            {
                if (km.Type == type)
                {
                    keyMessage = km;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Key Configuration
        public static KeyMessage ChangeKey(Key newKey, TypeMessage type)
        {
            if (TryGetKeyMessage(type, out var oldKm))
                oldKm.Dispose();

            return CreateKeyMessage(newKey, type);
        }

        /// <summary>
        /// Gets the key with the corresponding ID
        /// </summary>
        /// <param name="keyId">Id of a Key </param>
        /// <returns>Key with the corresponding ID</returns>
        private static Key GetKeyFromInt(int keyId) => Enum.GetValues<Key>().FirstOrDefault(x => (int)x == keyId);

        /// <summary>
        /// Reads and load the KeyMapping from the user.
        /// </summary>
        /// <exception cref="ArgumentNullException">If the config contains an empty line</exception>
        public static void InitKeyConfiguration()
        {
            if (!File.Exists(ConfigFilePath) || File.ReadAllLines(ConfigFilePath) == null)
                return;
                
            var data = File.ReadAllLines(ConfigFilePath);
            foreach (var line in data)
            {
                if(line == null)
                    throw new ArgumentNullException("line in InitKeyConfiguration");
                var args = line.Split(',');
                if (args[1] == "999")
                {
                    OpenChatKey = GetKeyFromInt(Int32.Parse(args[0]));
                    return;
                }
                ChangeKey(GetKeyFromInt(Int32.Parse(args[0])), (TypeMessage)Int32.Parse(args[1]));
            }

        }

        /// <summary>
        /// Saves the Key Mapping done by the user
        /// </summary>
        /// <returns>Returns True if the save was succesful, False otherwise</returns>
        public static bool SaveKeyConfiguration()
        {
            try
            {
                var save = new List<string>();
                foreach (var km in ListKeyMessages)
                {
                    var keyString = ((int)km.Key).ToString();
                    var typeString = ((int)km.Type).ToString();
                    save.Add(keyString + "," + typeString);
                }

                var chatKeyString = ((int)OpenChatKey).ToString();
                save.Add(chatKeyString + "," + "999");
                File.WriteAllLines(ConfigFilePath, save.ToArray());
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error while saving Key Configuration : " + ex.Message);
                return false;
            }
            return true;
        }

        #endregion
    }

}
