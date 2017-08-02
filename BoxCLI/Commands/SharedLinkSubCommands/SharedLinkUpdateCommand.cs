﻿using System;
using System.Threading.Tasks;
using Box.V2.Models;
using BoxCLI.BoxHome;
using BoxCLI.BoxPlatform.Service;
using BoxCLI.CommandUtilities;
using BoxCLI.CommandUtilities.Globalization;
using Microsoft.Extensions.CommandLineUtils;

namespace BoxCLI.Commands.SharedLinkSubCommands
{
    public class SharedLinkUpdateCommand : SharedLinkSubCommandBase
    {
        private CommandArgument _id;
        private CommandOption _access;
        private CommandOption _password;
        private CommandOption _unsharedAt;
        private CommandOption _canDownload;
        private CommandLineApplication _app;

        public SharedLinkUpdateCommand(IBoxPlatformServiceBuilder boxPlatformBuilder, IBoxHome home, LocalizedStringsResource names, BoxType t)
            : base(boxPlatformBuilder, home, names, t)
        {
        }

        public override void Configure(CommandLineApplication command)
        {
            _app = command;
            command.Description = "Update a shared link.";

            _id = command.Argument("itemId", "Id of Box item to update");
            _access = command.Option("--access <ACCESS>", "Shared link access level", CommandOptionType.SingleValue);
            _password = command.Option("--password <PASSWORD>", "Shared link password", CommandOptionType.SingleValue);
            _unsharedAt = command.Option("--unshared-at <TIME>", "Time that this link will become disabled, use formatting like 03w for 3 weeks.", CommandOptionType.SingleValue);
            _canDownload = command.Option("--can-download", "Whether the shared link allows downloads", CommandOptionType.NoValue);

            command.OnExecute(async () =>
            {
                return await this.Execute();
            });
            base.Configure(command);
        }

        protected async override Task<int> Execute()
        {
            await this.RunUpdate();
            return await base.Execute();
        }

        private async Task RunUpdate()
        {
            base.CheckForId(this._id.Value, this._app);
            var boxClient = base.ConfigureBoxClient(base._asUser.Value());
            if (base._t == BoxType.file)
            {
                var fileRequest = new BoxFileRequest();
                fileRequest.SharedLink = new BoxSharedLinkRequest();
                if (this._access.HasValue())
                {
                    fileRequest.SharedLink.Access = base.ResolveSharedLinkAccessType(this._access.Value());
                }
                if (this._password.HasValue())
                {
                    fileRequest.SharedLink.Password = this._password.Value();
                }
                if (this._unsharedAt.HasValue())
                {
                    fileRequest.SharedLink.UnsharedAt = GeneralUtilities.GetDateTimeFromString(this._unsharedAt.Value());
                }
                if (this._canDownload.HasValue())
                {
                    fileRequest.SharedLink.Permissions = new BoxPermissionsRequest();
                    fileRequest.SharedLink.Permissions.Download = true;
                }
                var result = await boxClient.FilesManager.UpdateInformationAsync(fileRequest);
                Reporter.WriteSuccess("Updated shared link:");
                base.PrintItem(result);
            }
            else if (base._t == BoxType.folder)
            {
                var folderUpdateRequest = new BoxFolderRequest();
                folderUpdateRequest.SharedLink = new BoxSharedLinkRequest();
                if (this._access.HasValue())
                {
                    folderUpdateRequest.SharedLink.Access = base.ResolveSharedLinkAccessType(this._access.Value());
                }
                if (this._password.HasValue())
                {
                    folderUpdateRequest.SharedLink.Password = this._password.Value();
                }
                if (this._unsharedAt.HasValue())
                {
                    folderUpdateRequest.SharedLink.UnsharedAt = GeneralUtilities.GetDateTimeFromString(this._unsharedAt.Value());
                }
                if (this._canDownload.HasValue())
                {
                    folderUpdateRequest.SharedLink.Permissions = new BoxPermissionsRequest();
                    folderUpdateRequest.SharedLink.Permissions.Download = true;
                }
                var updated = await boxClient.FoldersManager.UpdateInformationAsync(folderUpdateRequest);
                Reporter.WriteSuccess("Updated shared link:");
                base.PrintItem(updated);
            }
            else
            {
                throw new Exception("Box type not supported for this command.");
            }
        }
    }
}
