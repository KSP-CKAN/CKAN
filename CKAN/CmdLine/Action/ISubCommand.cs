using System;

namespace CKAN.CmdLine
{
    internal interface ISubCommand
    {
        int RunSubCommand(SubCommandOptions options);
    }
}

