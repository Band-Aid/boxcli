using System.Threading.Tasks;
using BoxCLI.BoxHome;
using BoxCLI.BoxPlatform.Service;
using BoxCLI.CommandUtilities;
using BoxCLI.CommandUtilities.CommandOptions;
using BoxCLI.CommandUtilities.Globalization;
using Microsoft.Extensions.CommandLineUtils;

namespace BoxCLI.Commands.FileSubCommand
{
    public class FileUploadCommand : FileSubCommandBase
    {
        private CommandArgument _path;
        private CommandOption _parentFolderId;
        private CommandOption _name;
        private CommandOption _bulkPath;
        private CommandOption _idOnly;
        private CommandLineApplication _app;
        private IBoxHome _home;

        public FileUploadCommand(IBoxPlatformServiceBuilder boxPlatformBuilder, IBoxHome home, LocalizedStringsResource names)
            : base(boxPlatformBuilder, home, names)
        {
            _home = home;
        }

        public override void Configure(CommandLineApplication command)
        {
            _app = command;
            command.Description = "Upload a file";
            _parentFolderId = command.Option("-p|--parent-folder",
                                                "Id of folder to upload file to, defaults to the root folder",
                                                CommandOptionType.SingleValue);
            _path = command.Argument("filePath",
                                        "Local path to file");
            _name = command.Option("-n|--name",
                                        "Provide different name for uploaded file", CommandOptionType.SingleValue);
            _bulkPath = BulkFilePathOption.ConfigureOption(command);
            _idOnly = IdOnlyOption.ConfigureOption(command);
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
            if (this._bulkPath.HasValue())
            {
                var json = false;
                if (base._json.HasValue() || this._home.GetBoxHomeSettings().GetOutputJsonSetting())
                {
                    json = true;
                }
                await base.ProcessFileUploadsFromFile(this._bulkPath.Value(), this._asUser.Value(), json: json);
                return;
            }
            base.CheckForFilePath(this._path.Value, this._app);
            var file = await base.UploadFile(path: this._path.Value, parentId: this._parentFolderId.Value(), fileName: this._name.Value(), idOnly: this._idOnly.HasValue());
            if (this._idOnly.HasValue())
            {
                Reporter.WriteInformation(file.Id);
                return;
            }
            if (base._json.HasValue() || this._home.GetBoxHomeSettings().GetOutputJsonSetting())
            {
                base.OutputJson(file);
                return;
            }
            base.PrintFile(file);
        }
    }
}