# Unity-Multithreading

**This script makes it possible to change scene objects from sub-threads in the Unity Engine.**

Unity has the problem of not supporting multithreading. For example, if you change the text of a gameobject from a sub-thread, Unity will throw an exception.
But sometimes it is neccessary to use multiple threads and access the GUI with them.
This script provides some basic methods to do this.

### Some examples which functionality the script provieds:
* Get text
* Set text
* Get component
* Find GameObject
* Change text color
* SetActive
* multiple more

### Extendable
You might run into the problem that you need a method that is not implemented by my script.
After a look into the 'How to extend' file you should be able to add your required functionality on the script.
Feel free to to add new functionality to this repo iva pull request.
