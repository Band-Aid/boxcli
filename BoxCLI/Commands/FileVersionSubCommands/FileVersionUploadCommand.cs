using System.Threading.Tasks;
using BoxCLI.BoxHome;
using BoxCLI.BoxPlatform.Service;
using BoxCLI.CommandUtilities;
using BoxCLI.CommandUtilities.CommandOptions;
using BoxCLI.CommandUtilities.Globalization;
using Microsoft.Extensions.CommandLineUtils;

namespace BoxCLI.Commands.FileVersionSubCommands
{
    public class FileVersionUploadCommand : FileVersionSubCommandBase
    {
        private CommandArgument _fileId;
        private CommandArgument _path;
        private CommandOption _parentFolderId;
        private CommandOption _name;
        private CommandOption _bulkPath;
        private CommandLineApplication _app;
        public FileVersionUploadCommand(IBoxPlatformServiceBuilder boxPlatformBuilder, IBoxHome boxHome, LocalizedStringsResource names)
            : base(boxPlatformBuilder, boxHome, names)
        {
        }

        public override void Configure(CommandLineApplication command)
        {
            _app = command;
            command.Description = "Get a file's information.";
            _fileId = command.Argument("fileId",
                                        "Id of file");
            _path = command.Argument("filePath",
                                        "Local path to file");
            _name = command.Option("-n|--name",
                                        "Provide different name for local file", CommandOptionType.SingleValue);
            _parentFolderId = command.Option("-p|--parent-folder",
                                        "Id of folder to upload file to, defaults to the root folder", 
                                        CommandOptionType.SingleValue);
            _bulkPath = BulkFilePathOption.ConfigureOption(command);
            command.OnExecute(async () =>
            {
                return await this.Execute();
            });
            base.Configure(command);
        }

        protected async override Task<int> Execute()
        {
            await this.RunUpload();
            return await base.Execute();
        }

        private async Task RunUpload()
        {
            Reporter.WriteInformation("new file versions...");
            if (this._bulkPath.HasValue())
            {
                Reporter.WriteInformation("Using bulk for new file versions...");
                await base.ProcessFileUploadsFromFile(this._bulkPath.Value(), this._asUser.Value(), true);
                return;
            }
            base.CheckForFileId(this._fileId.Value, this._app);
            base.CheckForFilePath(this._path.Value, this._app);
            base.PrintFile(await base.UploadFile(this._path.Value, parentId: this._parentFolderId.Value(), fileName: this._name.Value(), isNewVersion: true));
        }
    }
}