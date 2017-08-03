using System;
using System.Threading.Tasks;
using Box.V2.Models;
using BoxCLI.BoxHome;
using BoxCLI.BoxPlatform.Service;
using BoxCLI.CommandUtilities;
using BoxCLI.CommandUtilities.CommandOptions;
using BoxCLI.CommandUtilities.Globalization;
using Microsoft.Extensions.CommandLineUtils;

namespace BoxCLI.Commands.FolderSubCommands
{
    public class FolderCreateCommand : FolderSubCommandBase
    {
        private CommandArgument _parentFolderId;
        private CommandArgument _name;
        private CommandOption _bulkPath;
		private CommandOption _filePath;
		private CommandOption _fileFormat;
		private CommandOption _save;
        private CommandLineApplication _app;
        public FolderCreateCommand(IBoxPlatformServiceBuilder boxPlatformBuilder, IBoxHome home, LocalizedStringsResource names)
            : base(boxPlatformBuilder, home, names)
        {
        }

        public override void Configure(CommandLineApplication command)
        {
            _app = command;
            command.Description = "Get information about a folder.";
            _bulkPath = BulkFilePathOption.ConfigureOption(command);
			_filePath = FilePathOption.ConfigureOption(command);
			_fileFormat = FileFormatOption.ConfigureOption(command);
			_save = SaveOption.ConfigureOption(command);
            _parentFolderId = command.Argument("parentFolderId",
                               "Id of parent folder to add new folder to, use '0' for the root folder");
            _name = command.Argument("name", "Name of new folder");
            command.OnExecute(async () =>
            {
                return await this.Execute();
            });
            base.Configure(command);
        }

        protected async override Task<int> Execute()
        {
            await this.RunCreate();
            return await base.Execute();
        }

        protected async Task RunCreate()
        {
            if(this._bulkPath.HasValue())
            {
                await this.CreateFoldersFromFile(this._bulkPath.Value(), base._asUser.Value(), this._save.HasValue(), 
                                                 this._filePath.Value(), this._fileFormat.Value());
                return;
            }
            base.CheckForParentId(this._parentFolderId.Value, this._app);
            try
            {
                var BoxClient = base.ConfigureBoxClient(base._asUser.Value());
                var folderRequest = new BoxFolderRequest();
                folderRequest.Parent = new BoxItemRequest();
                folderRequest.Parent.Id = this._parentFolderId.Value;
                folderRequest.Name = this._name.Value;
                var folder = await BoxClient.FoldersManager.CreateAsync(folderRequest);
                System.Console.WriteLine(folder.Name);
                foreach (var item in folder.ItemCollection.Entries)
                {
                    System.Console.WriteLine($"{item.Name} - {item.Id}");
                }
            }
            catch (Exception e)
            {
                Reporter.WriteError(e.Message);
            }
        }
    }
}