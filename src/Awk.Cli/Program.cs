using Awk.Commands;
using Awk.Generated;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("awork");

    config.AddCommand<DoctorCommand>("doctor")
        .WithDescription("Validate token and connectivity");
    GeneratedCli.Register(config);
});

return app.Run(args);
