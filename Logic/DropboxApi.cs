﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using AuthenticationAPI.Security;
using Dropbox.Api;
using Dropbox.Api.FileRequests;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;

namespace AuthenticationAPI.Logic
{
    public static class DropboxApi
    {
        public static async Task<string> Upload(string localFile, string saveFileAs, bool makePublic)
        {

            using var dbx = new DropboxClient(ConfigContex.GetDropboxApiKey());
            using var mem = new MemoryStream(File.ReadAllBytes(localFile));


            var updated = await dbx.Files.UploadAsync("/" + saveFileAs, WriteMode.Overwrite.Instance, body: mem);

            // returns a shared link if its public
            if (makePublic)
                return await GetSharingLink(saveFileAs);
            else
                return null;
        }

        private static async Task<string> GetSharingLink(string savedDropboxFile)
        {
            using var dbx = new DropboxClient(ConfigContex.GetDropboxApiKey());

            savedDropboxFile = "/"+savedDropboxFile;

            try
            {
                var sharingLink = await dbx.Sharing.CreateSharedLinkWithSettingsAsync(new CreateSharedLinkWithSettingsArg(savedDropboxFile));

                // Dropbox return in the end of its link id=0, to get a raw image we need to change it to raw=1.
                string rawSharingLink = sharingLink.Url;
                rawSharingLink = rawSharingLink.Remove(rawSharingLink.Length - 4);
                rawSharingLink = rawSharingLink + "raw=1";
                return rawSharingLink;
            }
            catch
            {
                return null;
            }
        }

        public static async Task DeleteFromDropbox(string dropboxSharedLink)
        {
            try
            {
                
                string[] pathRoutes = dropboxSharedLink.Split("/");
                dropboxSharedLink = pathRoutes[pathRoutes.Length - 1];
                dropboxSharedLink = dropboxSharedLink.Remove(dropboxSharedLink.Length - 6);

                if (pathRoutes[2] != "www.dropbox.com")
                    return;

                using var dbx = new DropboxClient(ConfigContex.GetDropboxApiKey());

                DeleteArg deleteArg = new DeleteArg("/" + dropboxSharedLink);
                await dbx.Files.DeleteV2Async(deleteArg);
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
