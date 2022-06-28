using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
    Singleton pattern allowing this game object and other nested game objects to 
    persist when reloading the same scene (fx when respawning the player).
 */
public class ScenePersist : MonoBehaviour
{
    static ScenePersist instance = null;
    int startingSceneIndex;

    void Start()
    {
        if (!instance) 
        {
            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            startingSceneIndex = SceneManager.GetActiveScene().buildIndex;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (startingSceneIndex != SceneManager.GetActiveScene().buildIndex)
        {
            instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }
    }
}

/* Explanation 
 * Quick:
 * OnSceneLoaded is called before Start. (This is because SceneManager.sceneLoaded is a subscription service that is called before Start, and when ScenePersist is initialized the OnSceneLoaded() method is added as a subscriber.)
 * 
 * 
 * So in OnSceneLoaded() we check if the active SceneIndex is the same as what we have in this ScenePersist object. If it is not the this object is no longer needed, so:
 *  - first the instance variable is set to null (to enable the new ScenePersist object to add itself in its Start() method)
 *  - then the OnSceneLoaded() method on this object is removed from the SceneManager.sceneLoaded list.
 *  - lastly this object is destroyed.
 * 
 * 
 * In Start we check if the instance variable is null - if it is we assign this ScenePersist object to it. We add the OnSceneLoaded() method to the SceneManager.sceneLoaded list. And we store the current scene index in a variable.
 * 
 * If the instance variable is not null And the instance variable is not the same is this ScenePersist object - then we destroy this object.
 *
 * 
 * 
 * 
 * 
 * 
 * 
 * From udemy:
 The two main functions are Start, and OnSceneLoaded. There is also a static variable of type "ScenePersist" named instance.

Check thess links for reference:

https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html

https://docs.unity3d.com/Manual/ExecutionOrder.html

The key is in the execution order: take into account that the function OnSceneLoaded is executed at the OnEnable step (one step after Awake, and two before Start function). This means that when a new scene is loaded, OnSceneLoaded will be executed before the Start function of the new instance of ScenePersist.

Ok now, when a new ScenePersist is created, the first function it executes is Start (there is still no event attached to OnSceneLoaded). This function checks if anything is stored in the static variable. Remember that static variables are class related (not instance related) which means the variable is physically the same memory location for all instances of the class. If the instance of the class ScenePersist sets a value for the "instance" variable, this value is visible to any other instance of the same class. So, getting back to the Start function, if the instance variable has a value, it means there is already a ScenePersist active. Thus, the current instance destroy itself. If the instance variable is null, then it proceeds to allocate itself as a reference in the instance variable, then proceeds to assign the event OnSceneLoaded.

The idea is simple: only one instance is allowed to persist and assign the OnSceneLoaded event. All other instances are deleted at Start step.

Now, OnSceneLoaded will always be executed before the Start function (once event listener is assigned with the function). Thus, this function is always executed before there is another attempt to create a new instance of the class. Since the instance is already in a new scene, it can happen two things:

1.- The new scene is the same build index as the one before: in this case, nothing is done. "instance" variable still contains the already created instance of ScenePersist, and when the new instance executes its Start, it will destroy itself.

or

2.- The new scene has a different build scene than the last scene. In this case, the instance variable is assigned with null again, then the OnSceneLoaded function is removed from the event listener, and finally, the object destroys itself. After this is completed, the new instance executes its Start function; since there is nothing in "instance", it proceeds to assign itself to "instance" and assign the OnSceneLoaded.

Hope this helps to clarify why this code works. Regards!
 
 
 */
