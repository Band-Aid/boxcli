using System.Threading.Tasks;
using BoxCLI.BoxHome;
using BoxCLI.BoxPlatform.Service;
using BoxCLI.CommandUtilities;
using BoxCLI.CommandUtilities.Globalization;
using Microsoft.Extensions.CommandLineUtils;

namespace BoxCLI.Commands.FileSubCommand
{
    public class FileMoveCommand : FileSubCommandBase
    {
        private CommandArgument _fileId;
        private CommandArgument _parentFolderId;
        private CommandOption _etag;
        private CommandLineApplication _app;
        public FileMoveCommand(IBoxPlatformServiceBuilder boxPlatformBuilder, IBoxHome home, LocalizedStringsResource names)
            : base(boxPlatformBuilder, home, names)
        {
        }

        public override void Configure(CommandLineApplication command)
        {
            _app = command;
            command.Description = "Copy a file to a different folder.";
            _fileId = command.Argument("fileId",
                               "Id of file to manage");
            _parentFolderId = command.Argument("parentFolderId",
                                                "Id of new parent folder");
            _etag = command.Option("--etag", "Only move if etag value matches", CommandOptionType.SingleValue);
            command.OnExecute(async () =>
            {
                return await this.Execute();
            });
            base.Configure(command);
        }

        protected async override Task<int> Execute()
        {
            await this.RunMove();
            return await base.Execute();
        }

        private async Task RunMove()
        {
            base.CheckForFileId(this._fileId.Value, this._app);
            base.CheckForParentFolderId(this._parentFolderId.Value, this._app);
            var move = await base.MoveFile(this._fileId.Value, this._parentFolderId.Value, this._etag.Value());
            Reporter.WriteSuccess($"Moved file {this._fileId.Value} to folder {this._parentFolderId.Value}");
            base.PrintFile(move);
        }
    }
}