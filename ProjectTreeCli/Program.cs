using ProjectTreeCli.Commands;

var command = RootCommandBuilder.Build();

return await command.Parse(args).InvokeAsync();