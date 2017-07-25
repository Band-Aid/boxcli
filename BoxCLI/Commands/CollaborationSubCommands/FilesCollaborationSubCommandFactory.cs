using System;
using BoxCLI.BoxHome;
using BoxCLI.BoxPlatform.Service;
using BoxCLI.CommandUtilities.Globalization;

namespace BoxCLI.Commands.CollaborationSubCommands
{
    public class FilesCollaborationSubCommandFactory : AbstractBoxSubCommandFactory
    {
        public FilesCollaborationSubCommandFactory(IBoxPlatformServiceBuilder builder, IBoxHome boxHome, LocalizedStringsResource names)
            : base(builder, boxHome, names)
        {
        }

        public override ISubCommand CreateSubCommand(string commandName)
        {
            if (commandName == base._names.SubCommandNames.Get) 
            {
                return new CollaborationGetCommand(_boxPlatformBuilder, _boxHome, _names, BoxType.file);
            }
            else if (commandName == base._names.SubCommandNames.Add)
            {
                return new CollaborationAddCommand(_boxPlatformBuilder, _boxHome, _names, BoxType.file);
            }
            else
            {
                throw new Exception("Command not registered.");
            }
        }
    }
}