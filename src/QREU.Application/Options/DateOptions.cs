using System.CommandLine;

namespace Application.Options;
public sealed class StartDateOption : Option<DateTime?>
{
    public StartDateOption() : base("--start-date", "-s", "-start")
    {
        Description = "Start date for the research period (inclusive). Format: YYYY-MM-DD.";
    }
}

public sealed class EndDateOption : Option<DateTime?>
{
    public EndDateOption() : base("--end-date", "-e", "-end")
    {
        Description = "End date for the research period (inclusive). Format: YYYY-MM-DD.";
    }
}