using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Frosting;

namespace Build;

public class BuildLifetime : FrostingLifetime<BuildContext>
{
    public override void Setup(BuildContext context, ISetupContext info)
    {
    }

    public override void Teardown(BuildContext context, ITeardownContext info)
    {
        var quote = context.GetQuote(context.Paths.RootDirectory.CombineWithFilePath("quotes.txt"));
        if (quote == null)
        {
            return;
        }

        using (context.NormalVerbosity())
        {
            context.Information(quote);
        }
    }
}
