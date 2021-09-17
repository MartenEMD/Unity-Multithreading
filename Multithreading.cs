using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enables mutlithreading with keeping access to the Unity API
/// </summary>
/// <author>Marten van Hoorn</author>
public sealed class Multithreading
{
    /// <summary>
    /// Result of queued command
    /// </summary>
    public class Result : IDisposable
    {
        private bool isReady;
        private AutoResetEvent autoReset;
        private bool disposed = false;

        public Result()
        {
            autoReset = new AutoResetEvent(false);
        }

        /// <summary>
        /// True if result is ready
        /// </summary>
        public bool IsReady
        {
            get
            {
                return isReady;
            }
        }

        /// <summary>
        /// Called from the main thread when command is completed
        /// </summary>
        public void Ready()
        {
            isReady = true;
            autoReset.Set();
        }

        /// <summary>
        /// Waits until command is finished
        /// </summary>
        public void Wait()
        {
            autoReset.WaitOne();
        }

        /// <summary>
        /// Reset the instance
        /// </summary>
        public void Reset()
        {
            isReady = false;
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disbosing)
        {
            if(!disposed)
            {
                if(disbosing)
                {

                }

                //autoReset.Dispose();

                disposed = true;
            }
        }

        ~Result()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Result of queued command with return data
    /// </summary>
    /// <typeparam name="T">Dataype of return type</typeparam>
    public class Result<T> : IDisposable
    {
        private bool isReady;
        private T data;
        private AutoResetEvent autoReset;
        private bool disposed = false;

        public Result()
        {
            autoReset = new AutoResetEvent(false);
        }

        /// <summary>
        /// True if result is ready
        /// </summary>
        public bool IsReady
        {
            get
            {
                return isReady;
            }
        }

        /// <summary>
        /// Waits until command is finished and returns the data
        /// </summary>
        public T GetData
        {
            get
            {
                autoReset.WaitOne();
                return data;
            }
        }

        /// <summary>
        /// Called from the main thread when command is completed
        /// </summary>
        /// <param name="value">The result of the command</param>
        public void Ready(T value)
        {
            data = value;
            isReady = true;
            autoReset.Set();
        }

        /// <summary>
        /// Reset the instance
        /// </summary>
        public void Reset()
        {
            data = default(T);
            isReady = false;
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if(disposing)
                {

                }

                //autoReset.Dispose();
                //data = default;

                disposed = true;
            }
        }

        ~Result()
        {
            Dispose(false);
        }
    }


    /// <summary>
    /// List of all commands
    /// </summary>
    private enum CommandType
    {
        GetText,
        SetText,
        ThrowException,
        SetActive,
        ChangeTextColor,
        FindGameObject,
        FindChildObject,
        GetComponent,
        DestroyGameObject,
        SetPosition,
        CloneGameObject,
        GetComponentsInChildren,
        SetParent,
        PlayAudioSource
    }

    /// <summary>
    /// Holds a instance of CommandType which every command class inherit
    /// </summary>
    private abstract class BaseCommand
    {
        public CommandType CommandType { get; set; }
    }

    private class GetTextCommand : BaseCommand
    {
        public string Name { get; set; }
        public Result<string> Result { get; set; }

        public GetTextCommand()
        {
            CommandType = CommandType.GetText;
        }
    }

    private class SetTextCommand : BaseCommand
    {
        public Text Component { get; set; }
        public string Text { get; set; }
        public Result Result { get; set; }

        public SetTextCommand()
        {
            CommandType = CommandType.SetText;
        }
    }

    private class ThrowExceptionCommand : BaseCommand
    {
        public Exception Exception { get; set; }

        public ThrowExceptionCommand()
        {
            CommandType = CommandType.ThrowException;
        }
    }

    private class SetActiveCommand : BaseCommand
    {
        public GameObject GameObject { get; set; }
        public bool State { get; set; }
        public Result Result { get; set; }
        
        public SetActiveCommand()
        {
            CommandType = CommandType.SetActive;
        }
    }

    private class ChangeTextColorCommand : BaseCommand
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public Result Result { get; set; }

        public ChangeTextColorCommand()
        {
            CommandType = CommandType.ChangeTextColor;
        }
    }

    private class FindGameObjectCommand : BaseCommand
    {
        public string Name { get; set; }
        public Result<GameObject> Result { get; set; }

        public FindGameObjectCommand()
        {
            CommandType = CommandType.FindGameObject;
        }
    }

    private class FindChildObjectCommand : BaseCommand
    {
        public GameObject Parent { get; set; }
        public string ChildName { get; set; }
        public Result<GameObject> Result { get; set; }

        public FindChildObjectCommand()
        {
            CommandType = CommandType.FindChildObject;
        }
    }

    private class GetComponentCommand : BaseCommand
    {
        public GameObject GameObject { get; set; }
        public Type Type { get; set; }
        public object Result { get; set; }

        public GetComponentCommand()
        {
            CommandType = CommandType.GetComponent;
        }
    }

    private class DestroyGameObjectCommand : BaseCommand
    {
        public GameObject GameObject { get; set; }
        public Result Result { get; set; }

        public DestroyGameObjectCommand()
        {
            CommandType = CommandType.DestroyGameObject;
        }
    }

    private class SetPositionCommand : BaseCommand
    {
        public GameObject GameObject { get; set; }
        public Vector2 Position { get; set; }
        public Result Result { get; set; }

        public SetPositionCommand()
        {
            CommandType = CommandType.SetPosition;
        }
    }

    private class CloneGameObjectCommand : BaseCommand
    {
        public GameObject Prefab { get; set; }
        public Result<GameObject> Result { get; set; }

        public CloneGameObjectCommand()
        {
            CommandType = CommandType.CloneGameObject;
        }
    }

    private class GetComponentsInChildrenCommand : BaseCommand
    {
        public GameObject Parent { get; set; }
        public Type Type { get; set; }
        public object Result { get; set; }

        public GetComponentsInChildrenCommand()
        {
            CommandType = CommandType.GetComponentsInChildren;
        }
    }

    private class SetParentCommand : BaseCommand
    {
        public GameObject Child { get; set; }
        public GameObject Parent { get; set; }
        public Result Result { get; set; }

        public SetParentCommand()
        {
            CommandType = CommandType.SetParent;
        }
    }

    private class PlayAudioSourceCommand : BaseCommand
    {
        public AudioSource AudioSource { get; set; }
        public Result Result { get; set; }

        public PlayAudioSourceCommand()
        {
            CommandType = CommandType.PlayAudioSource;
        }
    }


    private readonly Queue<BaseCommand> commandQueue;

    private readonly Stack<GetTextCommand> getTextCommandPool;
    private readonly Stack<SetTextCommand> setTextCommandPool;
    private readonly Stack<ThrowExceptionCommand> throwExceptionCommandPool;
    private readonly Stack<SetActiveCommand> setActiveCommandPool;
    private readonly Stack<ChangeTextColorCommand> changeTextColorCommandPool;
    private readonly Stack<FindGameObjectCommand> findGameObjectCommandPool;
    private readonly Stack<FindChildObjectCommand> findChildObjectCommandPool;
    private readonly Stack<GetComponentCommand> getComponentCommandPool;
    private readonly Stack<DestroyGameObjectCommand> destroyGameObjectCommandPool;
    private readonly Stack<SetPositionCommand> setPositionCommandPool;
    private readonly Stack<CloneGameObjectCommand> cloneGameObjectCommandPool;
    private readonly Stack<GetComponentsInChildrenCommand> getComponentsInChildrenCommandPool;
    private readonly Stack<SetParentCommand> setParentCommandPool;
    private readonly Stack<PlayAudioSourceCommand> playAudioSourceCommandPool;


    public Multithreading()
    {
        commandQueue = new Queue<BaseCommand>();

        getTextCommandPool = new Stack<GetTextCommand>();
        setTextCommandPool = new Stack<SetTextCommand>();
        throwExceptionCommandPool = new Stack<ThrowExceptionCommand>();
        setActiveCommandPool = new Stack<SetActiveCommand>();
        changeTextColorCommandPool = new Stack<ChangeTextColorCommand>();
        findGameObjectCommandPool = new Stack<FindGameObjectCommand>();
        findChildObjectCommandPool = new Stack<FindChildObjectCommand>();
        getComponentCommandPool = new Stack<GetComponentCommand>();
        destroyGameObjectCommandPool = new Stack<DestroyGameObjectCommand>();
        setPositionCommandPool = new Stack<SetPositionCommand>();
        cloneGameObjectCommandPool = new Stack<CloneGameObjectCommand>();
        getComponentsInChildrenCommandPool = new Stack<GetComponentsInChildrenCommand>();
        setParentCommandPool = new Stack<SetParentCommand>();
        playAudioSourceCommandPool = new Stack<PlayAudioSourceCommand>();
    }


    private void QueueCommand(BaseCommand command)
    {
        lock(commandQueue)
        {
            commandQueue.Enqueue(command);
        }
    }

    private T GetFromPool<T>(Stack<T> pool) where T : new()
    {
        lock(pool)
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }
        }
        return new T();
    }

    private void ReturnToPool<T>(Stack<T> pool, T command)
    {
        lock(pool)
        {
            pool.Push(command);
        }
    }


    /// <summary>
    /// Get text from a text field
    /// </summary>
    /// <param name="name">Name of the GameObject</param>
    /// <param name="result">Instatnce of the generic Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void GetText(string name, Result<string> result)
    {
        if (name == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, GetText: One of the instances was null"));
            return;
        }

        result.Reset();

        GetTextCommand command = GetFromPool(getTextCommandPool);
        command.Name = name;
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Set text of a text field
    /// </summary>
    /// <param name="name">Name of the GameObject</param>
    /// <param name="text">The text that will be assigned to the text field</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void SetText(Text component, string text, Result result)
    {
        if (component == null || text == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, SetText: One of the instances was null"));
            return;
        }

        result.Reset();

        SetTextCommand command = GetFromPool(setTextCommandPool);
        command.Component = component;
        command.Text = text;
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Throws an exception
    /// </summary>
    /// <param name="ex">Exception to be thrown</param>
    public void ThrowException(Exception ex)
    {
        if (ex == null)
        {
            return;
        }

        ThrowExceptionCommand command = GetFromPool(throwExceptionCommandPool);
        command.Exception = ex;
        QueueCommand(command);
    }

    /// <summary>
    /// Enables or disables a GameObject
    /// </summary>
    /// <param name="gameObject">Instance of the GameObject</param>
    /// <param name="state">Whether the GameObject should be en- or disabled</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void SetActive(GameObject gameObject, bool state, Result result)
    {
        if (gameObject == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, SetActive: One of the instances was null"));
            return;
        }
        
        SetActiveCommand command = GetFromPool(setActiveCommandPool);
        command.GameObject = gameObject;
        command.State = state;
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Change the font color of a text
    /// </summary>
    /// <param name="Name">Name of the text field</param>
    /// <param name="color">Color the text should have</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void ChangeTextColor(string name, Color color, Result result)
    {
        if (name == null || color == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, ChangeTextColor: One of the instances was null"));
            return;
        }

        result.Reset();

        ChangeTextColorCommand command = GetFromPool(changeTextColorCommandPool);
        command.Name = name;
        command.Color = color;
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Finds a specific GameObject
    /// </summary>
    /// <param name="name">Name of the GameObjects</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    /// <remarks>The parameter "name" must match the exact name of the GameObject. </remarks>
    public void FindGameObject(string name, Result<GameObject> result)
    {
        if (name == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, FindGameObject: One of the instances was null"));
            return;
        }
        
        result.Reset();

        FindGameObjectCommand command = GetFromPool(findGameObjectCommandPool);
        command.Name = name;
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Finds a child GameObject
    /// </summary>
    /// <param name="parent">Parent GameObject</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void FindChildObject(GameObject parent, string childName, Result<GameObject> result)
    {
        if (parent == null || childName == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, FindChildObject: One of the instances was null"));
            return;
        }

        result.Reset();

        FindChildObjectCommand command = GetFromPool(findChildObjectCommandPool);
        command.Parent = parent;
        command.ChildName = childName;
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Get a specific component of a GameObject
    /// </summary>
    /// <typeparam name="T">The type of the component that should be returned</typeparam>
    /// <param name="gameObject">The instance of the GameObject the component is attached to</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void GetComponent<T>(GameObject gameObject, Result<T> result)
    {
        if (gameObject == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, GetComponent: One of the instances was null"));
            return;
        }
        
        result.Reset();

        GetComponentCommand command = GetFromPool(getComponentCommandPool);
        command.GameObject = gameObject;
        command.Type = typeof(T);
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Destroys a GameObject
    /// </summary>
    /// <param name="gameObject">The GameObject that should be destroyed</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void DestroyGameObject(GameObject gameObject, Result result)
    {
        if (gameObject == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, DestroyGameObject: One of the instances was null"));
            return;
        }

        result.Reset();

        DestroyGameObjectCommand command = GetFromPool(destroyGameObjectCommandPool);
        command.GameObject = gameObject;
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Moves the GameObject to a specific position
    /// </summary>
    /// <param name="gameObject">The GameObject taht should be moved</param>
    /// <param name="position">The position</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void SetPosition(GameObject gameObject, Vector2 position, Result result)
    {
        if (gameObject == null || position == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, SetPosition: One of the instances was null"));
            return;
        }

        result.Reset();

        SetPositionCommand command = GetFromPool(setPositionCommandPool);
        command.GameObject = gameObject;
        command.Position = position;
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Clones an excisting GameObject
    /// </summary>
    /// <param name="prefab">GameObject that should be cloned</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void CloneGameObject(GameObject prefab, Result<GameObject> result)
    {
        if (prefab == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, CloneGameObject: One of the instances was null"));
            return;
        }

        result.Reset();

        

        CloneGameObjectCommand command = GetFromPool(cloneGameObjectCommandPool);
        command.Prefab = prefab;
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Gets all components of a type in the children GameObjects
    /// </summary>
    /// <typeparam name="T">Type of the components</typeparam>
    /// <param name="parent">Parent GameObject</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void GetComponentsInChildren<T>(GameObject parent, Result<T> result)
    {
        if (parent == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, GetComponentsInChildren: One of the instances was null"));
            return;
        }

        result.Reset();

        GetComponentsInChildrenCommand command = GetFromPool(getComponentsInChildrenCommandPool);
        command.Parent = parent;
        command.Type = typeof(T);
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Assign a child GameObject to a parent GameObject
    /// </summary>
    /// <param name="child">Child GameObject</param>
    /// <param name="parent">Parent GameObject</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void SetParent(GameObject child, GameObject parent, Result result)
    {
        if (child == null || parent == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, GetComponentsInChildren: One of the instances was null"));
            return;
        }

        result.Reset();

        SetParentCommand command = GetFromPool(setParentCommandPool);
        command.Child = child;
        command.Parent = parent;
        command.Result = result;
        QueueCommand(command);
    }

    /// <summary>
    /// Calls the Play method of an Audio Source
    /// </summary>
    /// <param name="audioSource">Instance of the GameObject which Play method should be called</param>
    /// <param name="result">Instance of the Result class</param>
    /// <exception cref="NullReferenceException">Thrown if any of the parameters is null</exception>
    public void PlayAudioSource(AudioSource audioSource, Result result)
    {
        if (audioSource == null || result == null)
        {
            ThrowException(new NullReferenceException("Multithreading, GetComponentsInChildren: One of the instances was null"));
            return;
        }

        result.Reset();

        PlayAudioSourceCommand command = GetFromPool(playAudioSourceCommandPool);
        command.AudioSource = audioSource;
        command.Result = result;
        QueueCommand(command);
    }



    /// <summary>
    /// Execute all commands in the queue
    /// </summary>
    public void Execute()
    {
        while(true)
        {
            BaseCommand baseCommand;
            lock(commandQueue)
            {
                if (commandQueue.Count == 0)
                {
                    break;
                }
                baseCommand = commandQueue.Dequeue();
            }

            //Log.Write("Mutlithreading: " + baseCommand.CommandType);

            switch(baseCommand.CommandType)
            {
                case CommandType.GetText:
                    {
                        GetTextCommand command = (GetTextCommand)baseCommand;
                        string name = command.Name;
                        Result<string> result = command.Result;

                        command.Name = null;
                        command.Result = null;

                        ReturnToPool(getTextCommandPool, command);

                        GameObject gameObject = GameObject.Find(name);
                        Text GOText = gameObject.GetComponent<Text>();
                        string text = GOText.text;

                        result.Ready(text);
                        break;
                    }
                case CommandType.SetText:
                    {
                        SetTextCommand command = (SetTextCommand)baseCommand;
                        Text component = command.Component;
                        string text = command.Text;
                        Result result = command.Result;

                        command.Component = null;
                        command.Text = null;
                        command.Result = null;

                        ReturnToPool(setTextCommandPool, command);

                        component.text = text;
                        result.Ready();
                        break;
                    }
                case CommandType.ThrowException:
                    {
                        ThrowExceptionCommand command = (ThrowExceptionCommand)baseCommand;
                        Exception ex = command.Exception;

                        command.Exception = null;
                        ReturnToPool(throwExceptionCommandPool, command);

                        Log.Write("Exception: " + ex.ToString());
                        throw ex;
                    }
                case CommandType.SetActive:
                    {
                        SetActiveCommand command = (SetActiveCommand)baseCommand;
                        GameObject gameObject = command.GameObject;
                        bool state = command.State;
                        Result result = command.Result;

                        command.GameObject = null;
                        command.State = default;
                        command.Result = null;
                        ReturnToPool(setActiveCommandPool, command);

                        gameObject.SetActive(state);
                        result.Ready();
                        break;
                    }
                case CommandType.ChangeTextColor:
                    {
                        ChangeTextColorCommand command = (ChangeTextColorCommand)baseCommand;
                        string name = command.Name;
                        Color color = command.Color;
                        Result result = command.Result;

                        command.Name = null;
                        command.Color = default;
                        command.Result = null;
                        ReturnToPool(changeTextColorCommandPool, command);

                        GameObject gameObject = GameObject.Find(name);
                        Text textGO = gameObject.GetComponent<Text>();
                        textGO.color = color;
                        result.Ready();
                        break;
                    }
                case CommandType.FindGameObject:
                    {
                        FindGameObjectCommand command = (FindGameObjectCommand)baseCommand;
                        string name = command.Name;
                        Result<GameObject> result = command.Result;

                        command.Name = null;
                        command.Result = null;
                        ReturnToPool(findGameObjectCommandPool, command);

                        GameObject gameObject = GameObject.Find(name);
                        result.Ready(gameObject);
                        break;
                    }
                case CommandType.FindChildObject:
                    {
                        FindChildObjectCommand command = (FindChildObjectCommand)baseCommand;
                        GameObject parent = command.Parent;
                        string childName = command.ChildName;
                        Result<GameObject> result = command.Result;

                        command.Parent = null;
                        command.ChildName = null;
                        command.Result = null;
                        ReturnToPool(findChildObjectCommandPool, command);

                        result.Ready(parent.transform.Find(childName).gameObject);
                        break;
                    }
                case CommandType.GetComponent:
                    {
                        GetComponentCommand command = (GetComponentCommand)baseCommand;
                        GameObject gameObject = command.GameObject;
                        Type type = command.Type;
                        object result = command.Result;

                        command.GameObject = null;
                        command.Result = null;
                        ReturnToPool(getComponentCommandPool, command);
                        switch(type.FullName)
                        {
                            case "Alarm":
                                {
                                    (result as Result<Alarm>).Ready(gameObject.GetComponent<Alarm>());
                                    continue;
                                }
                            case "Calendar":
                                {
                                    (result as Result<Calendar>).Ready(gameObject.GetComponent<Calendar>());
                                    continue;
                                }
                            case "UnityEngine.UI.Text":
                                {
                                    (result as Result<Text>).Ready(gameObject.GetComponent<Text>());
                                    continue;
                                }
                            default:
                                {
                                    NotImplementedException ex = new NotImplementedException("GetComponent: The multithreading function has not been added yet.");
                                    Debug.LogWarning(ex);
                                    Log.Write($"Exception thrown: {ex}");
                                    throw ex;
                                }
                        }
                    }
                case CommandType.DestroyGameObject:
                    {
                        DestroyGameObjectCommand command = (DestroyGameObjectCommand)baseCommand;
                        GameObject gameObject = command.GameObject;
                        Result result = command.Result;

                        command.GameObject = null;
                        command.Result = null;
                        ReturnToPool(destroyGameObjectCommandPool, command);

                        GameObject.Destroy(gameObject);
                        result.Ready();
                        break;
                    }
                case CommandType.SetPosition:
                    {
                        SetPositionCommand command = (SetPositionCommand)baseCommand;
                        GameObject gameObject = command.GameObject;
                        Vector2 position = command.Position;
                        Result result = command.Result;

                        command.GameObject = null;
                        command.Position = default;
                        command.Result = null;
                        ReturnToPool(setPositionCommandPool, command);

                        //gameObject.transform.position = position;
                        Vector3 vector3 = new Vector3(position.x, position.y, 0);
                        gameObject.transform.Translate(vector3);
                        result.Ready();
                        break;
                    }
                case CommandType.CloneGameObject:
                    {
                        CloneGameObjectCommand command = (CloneGameObjectCommand)baseCommand;
                        GameObject prefab = command.Prefab;
                        Result<GameObject> result = command.Result;

                        command.Prefab = null;
                        command.Result = null;
                        ReturnToPool(cloneGameObjectCommandPool, command);

                        result.Ready(GameObject.Instantiate(prefab));
                        break;
                    }
                case CommandType.GetComponentsInChildren:
                    {
                        GetComponentsInChildrenCommand command = (GetComponentsInChildrenCommand)baseCommand;
                        GameObject parent = command.Parent;
                        Type type = command.Type;
                        object result = command.Result;

                        command.Parent = null;
                        command.Type = null;
                        command.Result = null;
                        ReturnToPool(getComponentsInChildrenCommandPool, command);

                        switch(type.FullName)
                        {
                            case "UnityEngine.UI.Text[]":   
                                {
                                    (result as Result<Text[]>).Ready(parent.GetComponentsInChildren<Text>());
                                    continue;
                                }
                            default:
                                {
                                    NotImplementedException ex = new NotImplementedException("GetComponentsInChildren: The multithreading function has not been added yet.");
                                    Debug.LogWarning(ex);
                                    Log.Write($"Exception thrown: {ex}");
                                    throw ex;
                                }
                        }
                    }
                case CommandType.SetParent:
                    {
                        SetParentCommand command = (SetParentCommand)baseCommand;
                        GameObject child = command.Child;
                        GameObject parent = command.Parent;
                        Result result = command.Result;
                        
                        command.Child = null;
                        command.Parent = null;
                        command.Result = null;
                        ReturnToPool(setParentCommandPool, command);

                        child.transform.SetParent(parent.transform);
                        result.Ready();
                        break;
                    }
                case CommandType.PlayAudioSource:
                    {
                        PlayAudioSourceCommand command = (PlayAudioSourceCommand)baseCommand;
                        AudioSource audioSource = command.AudioSource;
                        Result result = command.Result;

                        command.AudioSource = null;
                        command.Result = null;
                        ReturnToPool(playAudioSourceCommandPool, command);

                        audioSource.Play();
                        result.Ready();
                        break;
                    }
            }
        }
    }
}
