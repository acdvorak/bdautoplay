using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlurayAutoPlay
{
    class AutoplayController
    {
        private RegistryService registryService = new RegistryService();

        private List<IMediaPlayer> allMediaPlayers = new List<IMediaPlayer>()
        {
            new MediaPlayerClassic()
        };

        public AutoplayController()
        {
        }

        public List<IMediaPlayer> GetInstalledMediaPlayers()
        {
            return allMediaPlayers.Where(mediaPlayer => mediaPlayer.IsInstalled()).ToList();
        }

        public IMediaPlayer GetMediaPlayerByName(string name, List<IMediaPlayer> mediaPlayers = null)
        {
            mediaPlayers = mediaPlayers == null ? allMediaPlayers : mediaPlayers;
            List<IMediaPlayer> filtered = mediaPlayers.Where(mediaPlayer => mediaPlayer.GetName().Equals(name)).ToList();
            if (filtered.Count > 0)
            {
                return filtered.First();
            }
            return null;
        }

        public IMediaPlayer GetMediaPlayerByProgId(string progId, List<IMediaPlayer> mediaPlayers = null)
        {
            mediaPlayers = mediaPlayers == null ? allMediaPlayers : mediaPlayers;
            List<IMediaPlayer> filtered = allMediaPlayers.Where(mediaPlayer => mediaPlayer.GetProgId().Equals(progId)).ToList();
            if (filtered.Count > 0)
            {
                return filtered.First();
            }
            return null;
        }

        public IMediaPlayer GetMediaPlayerByHandlerName(string handlerName, List<IMediaPlayer> mediaPlayers = null)
        {
            mediaPlayers = mediaPlayers == null ? allMediaPlayers : mediaPlayers;
            List<IMediaPlayer> filtered = allMediaPlayers.Where(mediaPlayer => mediaPlayer.GetHandlerName().Equals(handlerName)).ToList();
            if (filtered.Count > 0)
            {
                return filtered.First();
            }
            return null;
        }

        public void ResetAutoplay()
        {
            registryService.RestoreBackup();
        }

        public void SetAutoplay(string mediaPlayerName)
        {
            IMediaPlayer mediaPlayer = GetMediaPlayerByName(mediaPlayerName);

            if (mediaPlayer == null)
            {
                ResetAutoplay();
            }
            else
            {
                registryService.CreateBackup();
                registryService.SetAutoplayHandler(mediaPlayer);
            }
        }

        public string GetSavedMediaPlayerName()
        {
            string handlerName = registryService.GetUserAutoplayHandlerName();
            IMediaPlayer mediaPlayer = GetMediaPlayerByHandlerName(handlerName);
            return mediaPlayer == null ? null : mediaPlayer.GetName();
        }
    }
}
