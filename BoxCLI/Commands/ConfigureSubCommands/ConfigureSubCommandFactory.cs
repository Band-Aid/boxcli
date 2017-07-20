using System;
using BoxCLI.BoxHome;
using BoxCLI.BoxHome.BoxHomeFiles;
using BoxCLI.Commands.ConfigureSubCommands.ConfigureSettingsSubCommands;
using BoxCLI.CommandUtilities.Globalization;

namespace BoxCLI.Commands.ConfigureSubCommands
{
    public class ConfigureSubCommandFactory : ISubCommandFactory
    {
        private readonly IBoxHome _boxHome;
        private readonly BoxEnvironments _environments;
        private readonly BoxDefaultSettings _settings;
        private readonly LocalizedStringsResource _names;

        public ConfigureSubCommandFactory(IBoxHome boxHome, LocalizedStringsResource names)
        {
            _boxHome = boxHome;
            _environments = boxHome.GetBoxEnvironments();
            _settings = boxHome.GetBoxHomeSettings();
            _names = names;
        }

        public ISubCommand CreateSubCommand(string commandName)
        {
            if (commandName == _names.SubCommandNames.List)
            {
                return new ConfigureListCommand(_boxHome);
            }
            else if (commandName == _names.SubCommandNames.Add)
            {
                return new ConfigureAddCommand(_boxHome);
            }
            else if (commandName == _names.SubCommandNames.GetDefault)
            {
                return new ConfigureGetDefaultCommand(_boxHome);
            }
            else if (commandName == _names.SubCommandNames.SetDefault)
            {
                return new ConfigureSetDefaultCommand(_boxHome);
            }
            else
            {
                throw new Exception("Command not registered.");
            }
        }

    }
}