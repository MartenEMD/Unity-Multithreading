# How to extend

1. Open the Multithreading script
2. Add command to the CommandType enum
3. Create a new Command class which derives from the BaseCommand class
    ```c#     
    private class GetTextCommand : BaseCommand
    {
        public string Name { get; set; }    //Parameter of the command
        public Result<string> Result { get; set; }  //The result type which returns a string in this case

        public GetTextCommand()
        {
            CommandType = CommandType.GetText;  //Set the command type from the enum
        }
    }     
    ```
4. Add a command pool stack and initialise it in the constuctor
5. Create the method like this:
```c#
    /// <summary>
    /// Get text from a text field
    /// </summary>
    /// <param name="name">Name of the GameObject</param>
    /// <param name="result">Instatnce of the generic Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void GetText(string name, Result<string> result)
    {
        if (name == null || result == null) //Check for null parameters
        {
            ThrowException(new NullReferenceException("Multithreading, GetText: One of the instances was null"));
            return;
        }

        result.Reset();     //Reset the result instance in case it was already used

        GetTextCommand command = GetFromPool(getTextCommandPool);   //Get the command from the created command pool
        command.Name = name;                                        //Assign all properties
        command.Result = result;
        QueueCommand(command);                                      //Queue the command to be executed in the Execute method
    }
```
6. Implement functionality in the execute method
    1. Add inside the switch your command like this:
    ```c#
    case CommandType.GetText:
                    {
                        GetTextCommand command = (GetTextCommand)baseCommand;   //Convert the base command to the specific command
                        string name = command.Name;                             //Assign the parameters
                        Result<string> result = command.Result;

                        command.Name = null;                                    //Set the command values to null
                        command.Result = null;

                        ReturnToPool(getTextCommandPool, command);              //Return the command to pool

                        GameObject gameObject = GameObject.Find(name);          //Do the functionality
                        Text GOText = gameObject.GetComponent<Text>();
                        string text = GOText.text;

                        result.Ready(text);                                     //Call the ready function and if needed return something
                        break;
                    }
    ```

