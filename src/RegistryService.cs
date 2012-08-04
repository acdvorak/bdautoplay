using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using Microsoft.Win32;
using System.Windows.Forms;

namespace BlurayAutoPlay
{
    class RegistryService
    {
        private Boolean showError = true;

        private const string DEFAULT_HANDLER_KEY_NAME = "EventHandlersDefaultSelection";
        private const string USER_HANDLER_KEY_NAME = "UserChosenExecuteHandlers";

        private const string DEFAULT_HANDLER_BACKUP_KEY_NAME = DEFAULT_HANDLER_KEY_NAME + "Backup";
        private const string USER_HANDLER_BACKUP_KEY_NAME = USER_HANDLER_KEY_NAME + "Backup";

        public RegistryService()
        {
        }

        public bool IsDefaultPlayer(string handlerName)
        {
            return handlerName != null && handlerName.Equals(ReadAutoplayHandler(USER_HANDLER_KEY_NAME));
        }

        public string GetSID()
        {
            NTAccount ntAccount = new NTAccount(Environment.UserName);
            SecurityIdentifier sid = (SecurityIdentifier)ntAccount.Translate(typeof(SecurityIdentifier));
            return sid.ToString();
        }

        public void CreateBackup()
        {
            BackupAutoplayHandler(DEFAULT_HANDLER_KEY_NAME, DEFAULT_HANDLER_BACKUP_KEY_NAME);
            BackupAutoplayHandler(USER_HANDLER_KEY_NAME, USER_HANDLER_BACKUP_KEY_NAME);
        }

        public void RestoreBackup()
        {
            RestoreAutoplayHandler(DEFAULT_HANDLER_KEY_NAME, DEFAULT_HANDLER_BACKUP_KEY_NAME);
            RestoreAutoplayHandler(USER_HANDLER_KEY_NAME, USER_HANDLER_BACKUP_KEY_NAME);
        }

        private void BackupAutoplayHandler(string handlerKeyName, string backupHandlerKeyName)
        {
            string handlerValue = ReadAutoplayHandler(handlerKeyName);
            string backupHandlerValue = ReadAutoplayHandler(backupHandlerKeyName);

            // Don't overwrite existing backup
            if (!AutoplayHandlerExists(backupHandlerKeyName))
            {
                WriteAutoplayHandler(backupHandlerKeyName, handlerValue);
            }
        }

        private void RestoreAutoplayHandler(string handlerKeyName, string backupHandlerKeyName)
        {
            string handlerValue = ReadAutoplayHandler(handlerKeyName);
            string backupHandlerValue = ReadAutoplayHandler(backupHandlerKeyName);

            WriteAutoplayHandler(handlerKeyName, ReadAutoplayHandler(backupHandlerKeyName));
            DeleteAutoplayHandler(backupHandlerKeyName);

            if (handlerValue != null && !handlerValue.Equals(""))
            {
                string progId = Read(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\" + handlerValue, "InvokeProgID");
                if (progId != null)
                {
                    DeleteSubKeyTree(Registry.ClassesRoot, progId);
                }
                DeleteKey(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\EventHandlers\PlayBluRayOnArrival", handlerValue);
                DeleteSubKeyTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\" + handlerValue);
            }
        }

        public void SetAutoplayHandler(IMediaPlayer mediaPlayer)
        {
            string handlerKeyName = mediaPlayer.GetHandlerName();
            string exePath = mediaPlayer.GetPath();
            string exeArgs = mediaPlayer.GetExeAutoplayArgs();
            string progId = mediaPlayer.GetProgId();
            string initCmdLine = mediaPlayer.GetInitCmdLine();
            string progName = mediaPlayer.GetName();

            WriteAutoplayHandler(DEFAULT_HANDLER_KEY_NAME, handlerKeyName);
            WriteAutoplayHandler(USER_HANDLER_KEY_NAME, handlerKeyName);

            string mpcHandlerKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\Handlers\" + handlerKeyName;

            Write(Registry.ClassesRoot, progId + @"\Shell\PlayVideoFiles\Command", "\"" + exePath + "\" " + exeArgs);
            Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\EventHandlers\PlayBluRayOnArrival", handlerKeyName, "");
            Write(Registry.LocalMachine, mpcHandlerKeyPath, "Action", "Play Blu-ray Video");
            Write(Registry.LocalMachine, mpcHandlerKeyPath, "Provider", progName);
            Write(Registry.LocalMachine, mpcHandlerKeyPath, "InvokeProgID", progId);
            Write(Registry.LocalMachine, mpcHandlerKeyPath, "InvokeVerb", "PlayVideoFiles");
            Write(Registry.LocalMachine, mpcHandlerKeyPath, "DefaultIcon", exePath + ",0");
            Write(Registry.LocalMachine, mpcHandlerKeyPath, "InitCmdLine", initCmdLine);
        }

        public string GetUserAutoplayHandlerName()
        {
            return ReadAutoplayHandler(USER_HANDLER_KEY_NAME);
        }

        protected string GetAutoplayHandlerKeyPath(string handlerKeyName, string subKeyName = @"\PlayBluRayOnArrival")
        {
            return GetSID() + @"\Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\" + handlerKeyName + subKeyName;
        }

        protected bool AutoplayHandlerExists(string handlerKeyName)
        {
            return Exists(Registry.Users, GetAutoplayHandlerKeyPath(handlerKeyName));
        }

        protected string ReadAutoplayHandler(string handlerKeyName)
        {
            return Read(Registry.Users, GetAutoplayHandlerKeyPath(handlerKeyName));
        }

        protected bool WriteAutoplayHandler(string handlerKeyName, string handlerValue)
        {
            return Write(Registry.Users, GetAutoplayHandlerKeyPath(handlerKeyName), handlerValue);
        }

        protected bool DeleteAutoplayHandler(string handlerKeyName)
        {
            return DeleteSubKeyTree(Registry.Users, GetAutoplayHandlerKeyPath(handlerKeyName, ""));
        }

        protected bool Exists(RegistryKey baseRegistryKey, string subKeyPath)
        {
            using (RegistryKey subKey = baseRegistryKey.OpenSubKey(subKeyPath))
            {
                return subKey != null;
            }
        }

        protected string Read(RegistryKey baseRegistryKey, string subKeyPath)
        {
            return Read(baseRegistryKey, subKeyPath, null);
        }

        protected string Read(RegistryKey baseRegistryKey, string subKeyPath, string keyName)
        {
            // Open a subKey as read-only
            using (RegistryKey subKey = baseRegistryKey.OpenSubKey(subKeyPath))
            {
                // If the RegistrySubKey doesn't exist -> (null)
                if (subKey == null)
                {
                    return null;
                }
                else
                {
                    string keyNameUpper = keyName == null ? null : keyName.ToUpper();
                    try
                    {
                        // If the RegistryKey exists I get its value
                        // or null is returned.
                        return (string)subKey.GetValue(keyNameUpper);
                    }
                    catch (Exception e)
                    {
                        // AAAAAAAAAAARGH, an error!
                        ShowErrorMessage(e, "Reading registry", baseRegistryKey, subKeyPath, keyNameUpper);
                        return null;
                    }
                }
            }
        }

        protected bool Write(RegistryKey baseRegistryKey, string subKeyPath, object keyValue)
        {
            return Write(baseRegistryKey, subKeyPath, null, keyValue);
        }

        protected bool Write(RegistryKey baseRegistryKey, string subKeyPath, string keyName, object keyValue)
        {
            string keyNameUpper = keyName == null ? null : keyName.ToUpper();
            try
            {
                // I have to use CreateSubKey 
                // (create or open it if already exits), 
                // 'cause OpenSubKey open a subKey as read-only
                using (RegistryKey subKey = baseRegistryKey.CreateSubKey(subKeyPath))
                {
                    if (keyValue == null)
                    {
                        subKey.SetValue(keyNameUpper, "");
                        subKey.DeleteValue(keyNameUpper, false);
                    }
                    else
                    {
                        subKey.SetValue(keyNameUpper, keyValue);
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Writing registry", baseRegistryKey, subKeyPath, keyNameUpper);
                return false;
            }
        }

        protected bool DeleteKey(RegistryKey baseRegistryKey, string subKeyPath, string keyName)
        {
            try
            {
                // Setting
                RegistryKey subKey = baseRegistryKey.CreateSubKey(subKeyPath);

                if (subKey != null)
                    subKey.DeleteValue(keyName, false);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Deleting key", baseRegistryKey, subKeyPath, keyName);
                return false;
            }
        }

        protected bool DeleteSubKeyTree(RegistryKey baseRegistryKey, string subKeyPath)
        {
            try
            {
                // Setting
                RegistryKey subKey = baseRegistryKey.CreateSubKey(subKeyPath);

                if (subKey != null)
                    baseRegistryKey.DeleteSubKeyTree(subKeyPath, false);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                ShowErrorMessage(e, "Deleting SubKey Tree", baseRegistryKey, subKeyPath, null);
                return false;
            }
        }

        private void Log(string description)
        {
            MessageBox.Show(description);
        }

        protected void ShowErrorMessage(Exception e, string operation, RegistryKey baseRegistryKey, string subKeyPath, string keyName)
        {
            ShowErrorMessage(e, operation + " [" + baseRegistryKey.ToString() + @"\" + subKeyPath + @"\" + (keyName == null ? "" : keyName.ToUpper()) + "]");
        }

        protected void ShowErrorMessage(Exception e, string Title)
        {
            if (showError)
                MessageBox.Show(e.Message,
                        Title
                        , MessageBoxButtons.OK
                        , MessageBoxIcon.Error);
        }
    }
}
