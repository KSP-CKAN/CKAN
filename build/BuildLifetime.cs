using System;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Frosting;

namespace Build;

public class BuildLifetime : FrostingLifetime<BuildContext>
{
    public override void Setup(BuildContext context, ISetupContext info)
    {
        var argConfiguration = context.Argument<string>("configuration", null);

        if (string.Equals(context.Target, "Release", StringComparison.OrdinalIgnoreCase))
        {
            if (argConfiguration != null)
                context.Warning($"Ignoring configuration argument: '{argConfiguration}'");

            context.BuildConfiguration = "Release";
        }
        else if (string.Equals(context.Target, "Debug", StringComparison.OrdinalIgnoreCase))
        {
            if (argConfiguration != null)
                context.Warning($"Ignoring configuration argument: '{argConfiguration}'");

            context.BuildConfiguration = "Debug";
        }
    }

    public override void Teardown(BuildContext context, ITeardownContext info)
    {
        var quote = context.GetQuote(context.Paths.RootDirectory.CombineWithFilePath("quotes.txt"));
        if (quote == null) return;
        
        using (context.NormalVerbosity())
        {
            context.Information(quote);
        }
    }
}