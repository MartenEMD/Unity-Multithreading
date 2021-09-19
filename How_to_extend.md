# How to extend

1. Open the Multithreading script
2. Create a new Command class which derives from the BaseCommand class

`private class GetTextCommand : BaseCommand
    {
        public string Name { get; set; }
        public Result<string> Result { get; set; }

        public GetTextCommand()
        {
            CommandType = CommandType.GetText;
        }
    }`
